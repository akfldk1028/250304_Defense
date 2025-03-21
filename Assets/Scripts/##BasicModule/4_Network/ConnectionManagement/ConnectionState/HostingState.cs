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
    /// 호스트 모드로 실행 중인 상태를 나타내는 클래스
    /// 
    /// 이 상태에서는 로컬 플레이어가 서버와 클라이언트 역할을 모두 수행합니다.
    /// 다른 클라이언트의 연결 요청을 처리하고, 게임 세션을 관리합니다.
    /// NetworkBehaviour 기능을 활용하여 서버 역할을 수행합니다.
    /// </summary>
    public class HostingState : OnlineState
    {

        //  [Inject] protected DebugClassFacade m_DebugClassFacade;
        //  [Inject] private IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;
        // [Inject] private SceneManagerEx _sceneManagerEx;
        // [Inject] protected ConnectionManager m_ConnectionManager;


        private const int k_MaxConnectPayload = 1024;
        private const int k_MaxPlayers = 2;
        private bool m_IsHosting = false;

        public override void Enter()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[HostingState] 시작: Enter");
            m_IsHosting = true;
            PublishConnectStatus(ConnectStatus.Connected);
            SetupHost();
        }

        private void SetupHost()
        {
            if (!m_ConnectionManager.NetworkManager.IsHost)
            {
                m_DebugClassFacade?.LogError(GetType().Name, "[HostingState] 호스트가 아닌 상태에서 호스트 설정 시도");
                m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
                return;
            }

            m_ConnectionManager.NetworkManager.OnClientConnectedCallback += OnClientConnected;
            m_ConnectionManager.NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
            m_DebugClassFacade?.LogInfo(GetType().Name, "[HostingState] 호스트 설정 완료");
        }

        public override void Exit()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[HostingState] 종료: Exit");
            m_IsHosting = false;

            if (m_ConnectionManager.NetworkManager.IsHost)
            {
                m_ConnectionManager.NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                m_ConnectionManager.NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            }
        }

        public override void OnClientConnected(ulong clientId)
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[HostingState] 클라이언트 연결됨: {clientId}");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Connected });
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[HostingState] 클라이언트 연결 해제: {clientId}");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Disconnected });
        }

        public override void OnPlayerJoined(ulong clientId)
        {
            if (m_ConnectionManager.NetworkManager.ConnectedClients.Count >= k_MaxPlayers)
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, "[HostingState] 최대 플레이어 수 도달");
                // _sceneManagerEx.ChangeSceneForAllPlayers(EScene.BasicGame);
            }
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            
            // SessionPlayerData를 사용하여 플레이어 연결 처리
            var sessionManager = SessionManager<SessionPlayerData>.Instance;
            
            // 중복 연결 확인
            if (sessionManager.IsDuplicateConnection(connectionPayload.playerId))
            {
                m_DebugClassFacade?.LogWarning(GetType().Name, $"[HostingState] 중복 연결 시도: {connectionPayload.playerId}");
                response.Approved = false;
                response.Reason = "중복된 플레이어 ID로 연결 시도";
                return;
            }
            
            // 새 플레이어의 세션 데이터 생성
            var sessionPlayerData = new SessionPlayerData(
                request.ClientNetworkId,
                connectionPayload.playerName,
                true
            );
            
            // 세션 매니저에 등록
            sessionManager.SetupConnectingPlayerSessionData(
                request.ClientNetworkId,
                connectionPayload.playerId,
                sessionPlayerData
            );
            
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[HostingState] 플레이어 연결 승인: {connectionPayload.playerName}, ID: {connectionPayload.playerId}");
            
            // 연결 승인
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
        }

        public override void OnServerStopped(bool indicator)
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[HostingState] 서버 중지");
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }
}