using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using VContainer;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 호스트 시작 상태 클래스
    /// 
    /// 네트워크 호스트 시작 상태를 관리하며, 호스트 초기화 및 연결 승인을 처리합니다.
    /// </summary>
    public class StartingHostState : ConnectionState
    {

        //  [Inject] protected DebugClassFacade m_DebugClassFacade;

        // [Inject] protected ConnectionManager m_ConnectionManager;
        // [Inject] protected NetworkManager m_NetworkManager;
        // [Inject] protected IPublisher<ConnectStatus> m_ConnectStatusPublisher;
        // [Inject] protected IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

        private bool m_IsStarting = false;
        private float m_StartTime;
        private const float k_HostStartTimeout = 10f;

    
        public override void Enter()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[StartingHostState] 호스트 시작");
            m_IsStarting = true;
            m_StartTime = Time.time;
            PublishConnectStatus(ConnectStatus.Connecting);
            StartHostAsync();
        }

        public override void Exit()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[StartingHostState] 호스트 시작 종료");
            m_IsStarting = false;
        }

    

        private async void StartHostAsync()
        {
            try
            {
                if (!m_NetworkManager.StartHost())
                {
                    m_DebugClassFacade?.LogError(GetType().Name, "[StartingHostState] 호스트 시작 실패");
                    OnHostStartFailed();
                    return;
                }

                m_DebugClassFacade?.LogInfo(GetType().Name, "[StartingHostState] 호스트 시작 성공");
                m_ConnectionManager.ChangeState(m_ConnectionManager.m_Hosting);
            }
            catch (Exception e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"[StartingHostState] 호스트 시작 중 오류: {e.Message}");
                OnHostStartFailed();
            }
        }

        public override void OnClientConnected(ulong clientId)
        {
            if (!m_IsStarting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, $"[StartingHostState] 클라이언트 연결됨: {clientId}");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Connected });
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (!m_IsStarting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, $"[StartingHostState] 클라이언트 연결 해제: {clientId}");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Disconnected });
        }

        public override void OnTransportFailure(ulong clientId)
        {
            if (!m_IsStarting) return;

            m_DebugClassFacade?.LogError(GetType().Name, "[StartingHostState] 네트워크 오류");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ConnectStatus = ConnectStatus.Failed });
            OnHostStartFailed();
        }

        private void OnHostStartFailed()
        {
            if (!m_IsStarting) return;

            m_IsStarting = false;
            m_NetworkManager.Shutdown();
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        public void CancelHostStart()
        {
            if (!m_IsStarting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, "[StartingHostState] 호스트 시작 취소");
            m_IsStarting = false;
            m_NetworkManager.Shutdown();
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}

