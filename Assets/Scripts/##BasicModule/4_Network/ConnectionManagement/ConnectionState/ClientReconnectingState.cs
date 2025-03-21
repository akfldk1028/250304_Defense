using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using VContainer;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 클라이언트 재연결 상태 클래스
    /// 
    /// 클라이언트 재연결 시도 상태를 관리하며, 연결 실패 후 재연결을 처리합니다.
    /// </summary>
    public class ClientReconnectingState : ConnectionState
    {
        private bool m_IsReconnecting = false;
        private float m_ReconnectStartTime;
        private const float k_ReconnectTimeout = 30f;
        private int m_ReconnectAttempts = 0;
        private const int k_MaxReconnectAttempts = 3;

        // [Inject]
        // protected ConnectionManager m_ConnectionManager;

        // [Inject]
        // protected IPublisher<ConnectStatus> m_ConnectStatusPublisher;

        // [Inject]
        // protected NetworkManager m_NetworkManager;

        // [Inject]
        // protected IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

        // [Inject] protected DebugClassFacade m_DebugClassFacade;

        public override void Enter()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientReconnectingState] 클라이언트 재연결 시작");
            m_IsReconnecting = true;
            m_ReconnectStartTime = Time.time;
            PublishConnectStatus(ConnectStatus.Connecting);
            StartReconnect();
        }

        public override void Exit()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientReconnectingState] 클라이언트 재연결 종료");
            m_IsReconnecting = false;
            m_ReconnectAttempts = 0;
        }

   

        private async void StartReconnect()
        {
            try
            {
                if (m_ReconnectAttempts >= k_MaxReconnectAttempts)
                {
                    m_DebugClassFacade?.LogError(GetType().Name, "[ClientReconnectingState] 최대 재연결 시도 횟수 초과");
                    OnReconnectFailed();
                    return;
                }

                m_ReconnectAttempts++;
                m_DebugClassFacade?.LogInfo(GetType().Name, $"[ClientReconnectingState] 재연결 시도 {m_ReconnectAttempts}/{k_MaxReconnectAttempts}");

                if (!m_NetworkManager.StartClient())
                {
                    m_DebugClassFacade?.LogError(GetType().Name, "[ClientReconnectingState] 재연결 실패");
                    OnReconnectFailed();
                    return;
                }

                m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientReconnectingState] 재연결 성공");
            }
            catch (Exception e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"[ClientReconnectingState] 재연결 중 오류: {e.Message}");
                OnReconnectFailed();
            }
        }

        public override void OnClientConnected(ulong clientId)
        {
            if (!m_IsReconnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, $"[ClientReconnectingState] 클라이언트 재연결됨: {clientId}");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Connected });
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (!m_IsReconnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, $"[ClientReconnectingState] 클라이언트 연결 해제: {clientId}");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Disconnected });
            OnReconnectFailed();
        }

        public override void OnTransportFailure(ulong clientId)
        {
            if (!m_IsReconnecting) return;

            m_DebugClassFacade?.LogError(GetType().Name, "[ClientReconnectingState] 네트워크 오류");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Failed });
            OnReconnectFailed();
        }

        private void OnReconnectFailed()
        {
            if (!m_IsReconnecting) return;

            m_IsReconnecting = false;
            m_NetworkManager.Shutdown();
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        public void CancelReconnect()
        {
            if (!m_IsReconnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientReconnectingState] 재연결 취소");
            m_IsReconnecting = false;
            m_NetworkManager.Shutdown();
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}