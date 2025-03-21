using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 클라이언트가 서버에 연결된 상태를 나타내는 클래스
    /// 
    /// 이 상태에서는 로컬 플레이어가 서버에 연결되어 게임 세션에 참여합니다.
    /// 게임 플레이 중 서버와의 연결 상태를 모니터링하고, 필요 시 재연결을 시도합니다.
    /// </summary>
    public class ClientConnectedState : ConnectionState
    {
        // 세션 매니저 접근자
        private SessionManager<SessionPlayerData> m_SessionManager => SessionManager<SessionPlayerData>.Instance;
        private string m_LocalPlayerId;
        private bool m_IsConnected = false;

        // [Inject]
        // protected NetworkManager m_NetworkManager;


        // [Inject] protected DebugClassFacade m_DebugClassFacade;

        // [Inject] private IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

        // [Inject]
        // protected ConnectionManager m_ConnectionManager;

        // [Inject]
        // protected IPublisher<ConnectStatus> m_ConnectStatusPublisher;

        public override void Enter()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientConnectedState] 시작: Enter");
            m_IsConnected = true;
            m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
            
            // 세션 데이터 확인
            CheckSessionData();
            m_ConnectStatusPublisher?.Publish(ConnectStatus.Connected);
        }
        
        private void CheckSessionData()
        {
            // 로컬 플레이어의 세션 데이터 확인
            var localClientId = m_NetworkManager.LocalClientId;
            var playerData = m_SessionManager.GetPlayerData(localClientId);
            
            if (playerData != null)
            {
                m_LocalPlayerId = m_SessionManager.GetPlayerId(localClientId);
                m_DebugClassFacade?.LogInfo(GetType().Name, $"[ClientConnectedState] 플레이어 세션 데이터 확인: {playerData.Value.PlayerName}, ID: {m_LocalPlayerId}");
            }
            else
            {
                m_DebugClassFacade?.LogWarning(GetType().Name, "[ClientConnectedState] 플레이어 세션 데이터가 없습니다");
            }
        }

        public override void Exit()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientConnectedState] 종료: Exit");
            m_IsConnected = false;
            if (m_NetworkManager)
            {
                m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            }
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (!m_IsConnected) return;
            
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[ClientConnectedState] 클라이언트 연결 해제: {clientId}");
            
            // 세션 매니저에 연결 해제 알림
            m_SessionManager.DisconnectClient(clientId);
            
            if (clientId == m_NetworkManager.LocalClientId)
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientConnectedState] 로컬 클라이언트 연결 해제");
                
                // 연결 해제 이벤트 발행
                m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Disconnected });
                
                // 재연결을 위한 상태 변경
                m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientReconnecting);
            }
        }

        public override void OnTransportFailure(ulong clientId)
        {
            if (!m_IsConnected) return;

            m_DebugClassFacade?.LogError(GetType().Name, "[ClientConnectedState] 네트워크 오류");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Failed });
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        public void Disconnect()
        {
            if (!m_IsConnected) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, "[ClientConnectedState] 연결 해제");
            m_IsConnected = false;
            m_NetworkManager.Shutdown();
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}