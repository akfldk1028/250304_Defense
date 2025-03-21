using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using VContainer;
// using Unity.Assets.Scripts.ConnectionManagement;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 클라이언트 연결 상태 클래스
    /// 
    /// 클라이언트 연결 시도 상태를 관리하며, 서버 연결 및 승인을 처리합니다.
    /// </summary>
    public class ClientConnectingState : ConnectionState
    {
        private bool m_IsConnecting = false;
        private float m_ConnectionStartTime;
        private const float k_ConnectionTimeout = 30f;

        // [Inject]
        // protected ConnectionManager m_ConnectionManager;

        // [Inject]
        // protected IPublisher<ConnectStatus> m_ConnectStatusPublisher;



        // [Inject]
        // protected NetworkManager m_NetworkManager;

        // [Inject]
        // protected IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;
        //  [Inject] protected DebugClassFacade m_DebugClassFacade;

        public override void Enter()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientConnectingState] 클라이언트 연결 시작");
            m_IsConnecting = true;
            m_ConnectionStartTime = Time.time;
            PublishConnectStatus(ConnectStatus.Connecting);
            ConnectClientAsync();
        }

        public override void Exit()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientConnectingState] 클라이언트 연결 종료");
            m_IsConnecting = false;
        }


        private async void ConnectClientAsync()
        {
            try
            {
                if (!m_NetworkManager.StartClient())
                {
                    m_DebugClassFacade?.LogError(GetType().Name, "[ClientConnectingState] 클라이언트 연결 실패");
                    OnConnectionFailed();
                    return;
                }

                m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientConnectingState] 클라이언트 연결 성공");
            }
            catch (Exception e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"[ClientConnectingState] 클라이언트 연결 중 오류: {e.Message}");
                OnConnectionFailed();
            }
        }

        public override void OnClientConnected(ulong clientId)
        {
            if (!m_IsConnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, $"[ClientConnectingState] 클라이언트 연결됨: {clientId}");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Connected });
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (!m_IsConnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, $"[ClientConnectingState] 클라이언트 연결 해제: {clientId}");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Disconnected });
            OnConnectionFailed();
        }

        public override void OnTransportFailure(ulong clientId)
        {
            if (!m_IsConnecting) return;

            m_DebugClassFacade?.LogError(GetType().Name, "[ClientConnectingState] 네트워크 오류");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Failed });
            OnConnectionFailed();
        }

        private void OnConnectionFailed()
        {
            if (!m_IsConnecting) return;

            m_IsConnecting = false;
            m_NetworkManager.Shutdown();
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        public void CancelConnection()
        {
            if (!m_IsConnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientConnectingState] 연결 취소");
            m_IsConnecting = false;
            m_NetworkManager.Shutdown();
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}