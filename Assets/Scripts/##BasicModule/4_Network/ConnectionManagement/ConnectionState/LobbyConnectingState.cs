using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using VContainer;
using System.Collections.Generic;
using Unity.Assets.Scripts.Auth;
using Unity.Assets.Scripts.Scene;
using Unity.VisualScripting;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 로비 연결 상태 클래스
    /// 
    /// 로비 연결 상태를 관리하며, 매칭 서비스를 통해 로비 생성/참가를 처리합니다.
    /// </summary>
    public class LobbyConnectingState : ConnectionState
    {
        
        private readonly float k_LobbyApiCooldown = 2.0f; // 2초 쿨다운
        private float m_LastLobbyApiCallTime = 0f;
        private bool m_IsConnecting = false;
        private float m_ConnectionStartTime;
        private const float k_ConnectionTimeout = 30f;

        // 로비 관련 변수 추가
        private Lobby currentLobby;
        private const int maxPlayers = 2; // 최대 플레이어 수 (필요에 따라 조정)
        
        // 플레이어 세션 관련 변수
        private string m_LocalPlayerId;
        private string m_LocalPlayerName = "Player"; // 기본 이름
        private SessionManager<SessionPlayerData> m_SessionManager => SessionManager<SessionPlayerData>.Instance;
              
        // [Inject] protected DebugClassFacade m_DebugClassFacade;
        [Inject] private SceneManagerEx _sceneManagerEx;
        // [Inject] private IObjectResolver m_Resolver;
        // [Inject] protected ConnectionManager m_ConnectionManager;


        // [Inject]
        // protected NetworkManager m_NetworkManager;
        // [Inject]
        // protected IPublisher<ConnectStatus> m_ConnectStatusPublisher;

        // [Inject]
        // protected IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

        public override void Enter()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 로비 연결 시작");
            m_IsConnecting = true;
            m_ConnectionStartTime = Time.time;

            // m_LocalPlayerId 초기화
            m_LocalPlayerId = System.Guid.NewGuid().ToString();
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 로컬 플레이어 ID 초기화: {m_LocalPlayerId}");
     
            //  if (m_AuthManager.IsAuthenticated)
            // {
            //     m_LocalPlayerId =  m_AuthManager.PlayerId;
            //     // // 인증된 플레이어라면 DB에서 데이터 가져오기
            //     // PlayerData data = await DatabaseService.GetPlayerData(m_LocalPlayerId);
            //     // if (data != null)
            //     // {
            //     //     m_LocalPlayerName = data.playerName;
            //     //     // 기타 데이터 로드
            //     // }
            // }
                    // 로컬 플레이어 ID 생성 (고유 ID)

            PublishConnectStatus(ConnectStatus.Connecting);
        }

        public override void Exit()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 로비 연결 종료");
            m_IsConnecting = false;
        }

     


        public override void OnClientConnected(ulong clientId)
        {
            if (!m_IsConnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 클라이언트 연결됨: {clientId}");
            
            // 호스트인 경우 연결된 클라이언트의 세션 데이터를 초기화
            if (m_NetworkManager.IsHost && clientId != m_NetworkManager.LocalClientId)
            {
                // 클라이언트가 연결되면 CreatePlayerData 메서드에서 세션 데이터 생성
                // 이 단계에서는 아직 클라이언트 데이터를 받지 않았으므로 기본값으로 설정
                m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 클라이언트({clientId})의 세션 데이터 초기화 준비");
            }
            
            // 자신이 로컬 클라이언트인 경우 세션 데이터 초기화
            if (clientId == m_NetworkManager.LocalClientId)
            {
                InitializeLocalPlayerSessionData();
            }
            
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Connected });
            OnPlayerJoined();
        }

        private void InitializeLocalPlayerSessionData()
        {
            // 세션 매니저가 null인지 확인
            if (m_SessionManager == null)
            {
                m_DebugClassFacade?.LogError(GetType().Name, "[LobbyConnectingState] 세션 매니저가 초기화되지 않았습니다.");
                return;
            }

            // 로컬 플레이어의 세션 데이터 생성
            SessionPlayerData playerData = new SessionPlayerData(
                m_NetworkManager.LocalClientId,
                m_LocalPlayerName,
                true
            );
            
            // 세션 매니저에 등록
            m_SessionManager.SetupConnectingPlayerSessionData(
                m_NetworkManager.LocalClientId,
                m_LocalPlayerId,
                playerData
            );
            
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 로컬 플레이어 세션 데이터 초기화: {m_LocalPlayerName}, ID: {m_LocalPlayerId}");
        }


        public override void OnClientDisconnect(ulong clientId)
        {
            if (!m_IsConnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 클라이언트 연결 해제: {clientId}");
            
            // 세션 매니저에 연결 해제 알림
            m_SessionManager.DisconnectClient(clientId);
            
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ClientId = clientId, ConnectStatus = ConnectStatus.Disconnected });
            OnConnectionFailed();
        }

        public override void OnTransportFailure(ulong clientId)
        {
            if (!m_IsConnecting) return;

            m_DebugClassFacade?.LogError(GetType().Name, "[LobbyConnectingState] 네트워크 오류");
            m_ConnectionEventPublisher?.Publish(new ConnectionEventMessage { ConnectStatus = ConnectStatus.Failed });
            OnConnectionFailed();
        }

        private void OnConnectionFailed()
        {
            if (!m_IsConnecting) return;

            m_IsConnecting = false;
            if (m_ConnectionManager == null)
            {
                m_DebugClassFacade?.LogError(GetType().Name, "[LobbyConnectingState] ConnectionManager가 초기화되지 않았습니다.");
                return;
            }
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        public void CancelConnection()
        {
            if (!m_IsConnecting) return;

            m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 연결 취소");
            m_IsConnecting = false;
            
            // 로비가 존재하면 삭제
            if (currentLobby != null)
            {
                DestroyLobby(currentLobby.Id);
            }
            
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        // 여기서부터 주석 해제된 코드 구현

        public async override void StartHostLobby()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 호스트 로비 시작");
            
            // 대기 시간 추가
            if (Time.time - m_LastLobbyApiCallTime < k_LobbyApiCooldown)
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] API 쿨다운 중, 잠시 대기합니다.");
                await Task.Delay(2000); // 2초 대기
            }
            
            // 기존 로직 유지
            currentLobby = await FindAvailableLobby();

            if (currentLobby == null)
            {
                await CreateNewLobby();
            }
            else
            {
                await JoinLobby(currentLobby.Id);
            }
        }
        private async Task<Lobby> FindAvailableLobby()
        {
            try
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 가용 로비 검색");
                var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
                if (queryResponse.Results.Count > 0)
                {
                    m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 로비 발견: {queryResponse.Results[0].Id}");
                    return queryResponse.Results[0];
                }
            }
            catch (LobbyServiceException e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"로비 조회 중 오류 발생: {e.Message}");
            }
            m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 가용 로비 없음");
            return null;
        }




        private async void DestroyLobby(string lobbyId)
        {
            try
            {
                if (!string.IsNullOrEmpty(lobbyId))
                {
                    m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 로비 삭제: {lobbyId}");
                    await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                    Debug.Log("Lobby destroyed " + lobbyId);
                }
                if (m_NetworkManager.IsHost)
                {
                    m_NetworkManager.Shutdown();
                    // Matching_Object.SetActive(false); // UI 관련 코드는 필요에 따라 활성화
                }
            }
            catch(Exception e) 
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"로비 삭제 중 오류 발생: {e.Message}");
            }
        }

        public override void StartClientIP(string playerName, string ipaddress, int port)
        {
            // IP 연결은 별도 구현 필요
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] IP 연결 시작: {ipaddress}:{port}");
            // 플레이어 이름 저장
            m_LocalPlayerName = playerName;
            // var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, m_ConnectionManager, m_ProfileManager, playerName);
            // m_ConnectionManager.m_ClientReconnecting.Configure(connectionMethod);
            // m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting.Configure(connectionMethod));
        }

        private async Task CreateNewLobby()
        {
            try
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 새 로비 생성");
                string randomLobbyName = GenerateRandomLobbyName();
                
                // 로비 옵션 생성 - 플레이어 데이터 포함
                var lobbyOptions = new CreateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        // 호스트 플레이어 정보 추가
                        { "HostPlayerId", new DataObject(DataObject.VisibilityOptions.Member, m_LocalPlayerId) },
                    }
                };
                
                currentLobby = await LobbyService.Instance.CreateLobbyAsync(randomLobbyName, maxPlayers, lobbyOptions);
                m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 로비 생성 완료: {currentLobby.Id}");
                
                await AllocateRelayServerAndJoin(currentLobby);
                // CancelButton.onClick.AddListener(() => DestroyLobby(currentLobby.Id)); // UI 관련 코드는 필요에 따라 활성화
                StartHost();
                
                // 세션 시작 표시
                m_SessionManager.OnSessionStarted();
            }
            catch (LobbyServiceException e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"로비 생성 중 오류 발생: {e.Message}");
                OnConnectionFailed();
            }
        }

        private async Task JoinLobby(string lobbyId)
        {
            try
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 로비 참가: {lobbyId}");
                
                // 로비 참가 옵션에 플레이어 데이터 추가
                var joinOptions = new JoinLobbyByIdOptions
                {
                    Player = new Player
                    {
                        Data = new Dictionary<string, PlayerDataObject>
                        {
                            { "PlayerId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, m_LocalPlayerId) },
                        }
                    }
                };
                
                currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
                
                // 로비에서 호스트 정보 확인
                if (currentLobby.Data != null && 
                    currentLobby.Data.TryGetValue("hostId", out var hostIdData))
                {
                    string hostId = hostIdData.Value;
                    m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 호스트 정보: ID: {hostId}");
                }
                
                // 로비에서 릴레이 코드 가져오기
                if (currentLobby.Data != null && currentLobby.Data.TryGetValue("RelayJoinCode", out var relayJoinCodeData))
                {
                    string relayJoinCode = relayJoinCodeData.Value;
                    m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 릴레이 코드: {relayJoinCode}");
                    
                    // 릴레이 코드로 연결
                    await JoinRelayServer(relayJoinCode);
                }
                else
                {
                    m_DebugClassFacade?.LogError(GetType().Name, "[LobbyConnectingState] 릴레이 코드를 찾을 수 없습니다");
                }
                
                StartClientLobby();
            }
            catch (LobbyServiceException e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"로비 조인 중 오류 발생: {e.Message}");
                OnConnectionFailed();
            }
        }
        
        private async Task JoinRelayServer(string joinCode)
        {
            try
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 릴레이 서버 연결 시도: {joinCode}");
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
          
            }
            catch (RelayServiceException e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"릴레이 서버 참가 중 오류 발생: {e.Message}");
                OnConnectionFailed();
            }
        }

        private void StartHost()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 호스트 시작");
            if (m_NetworkManager == null)
            {
                m_DebugClassFacade?.LogError(GetType().Name, "[LobbyConnectingState] NetworkManager가 초기화되지 않았습니다");

            }
            m_NetworkManager.StartHost();

            m_NetworkManager.OnClientConnectedCallback += OnClientConnected;
            m_NetworkManager.OnClientDisconnectCallback += OnHostDisconnected;
            
            // 클라이언트 상태 변경
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Hosting);
        }

        public void OnPlayerJoined()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 플레이어 참가, 현재 인원: {m_NetworkManager.ConnectedClients.Count}");
            
            // 플레이어 수 업데이트를 로비에 반영 (호스트인 경우)
            if (m_NetworkManager.IsHost && currentLobby != null)
            {
                UpdateLobbyPlayerCount();
            }
            
            if(m_NetworkManager.ConnectedClients.Count >= maxPlayers)
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 최대 인원 도달, 게임 씬 전환");
                _sceneManagerEx.ChangeSceneForAllPlayers(Unity.Assets.Scripts.Scene.EScene.BasicGame);
            }
        }
        
        private async void UpdateLobbyPlayerCount()
        {
            try
            {
                // 로비 데이터 업데이트 - 현재 플레이어 수 정보
                var lobbyData = new Dictionary<string, DataObject>();
                lobbyData.Add("PlayerCount", new DataObject(
                    visibility: DataObject.VisibilityOptions.Public,
                    value: m_NetworkManager.ConnectedClients.Count.ToString()
                ));
                
                // 로비 데이터 업데이트
                var updateLobbyOptions = new UpdateLobbyOptions { Data = lobbyData };
                await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateLobbyOptions);
                m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 로비 플레이어 수 업데이트: {m_NetworkManager.ConnectedClients.Count}");
            }
            catch (LobbyServiceException e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"로비 데이터 업데이트 중 오류 발생: {e.Message}");
            }
        }

        public override void OnHostDisconnected(ulong clientId)
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 호스트 연결 해제: {clientId}");
            if(clientId == m_NetworkManager.LocalClientId && m_NetworkManager.IsHost)
            {
                m_NetworkManager.OnClientConnectedCallback -= OnClientConnected;
                m_NetworkManager.OnClientDisconnectCallback -= OnHostDisconnected;
                
                // 세션 종료 처리
                m_SessionManager.OnSessionEnded();
            }
        }

        private async Task AllocateRelayServerAndJoin(Lobby lobby)
        {
            try
            {
                m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] Relay 서버 할당");
                var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                // 필요한 경우 UI에 조인 코드 표시
                // JoinCodeText.text = joinCode;
                m_DebugClassFacade?.LogInfo(GetType().Name, $"Relay 연결 코드: {joinCode}");
                
                // 로비 데이터에 릴레이 코드 추가
                try
                {
                    // 로비 데이터 준비
                    var lobbyData = new Dictionary<string, DataObject>();
                    lobbyData.Add("RelayJoinCode", new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: joinCode
                    ));
                    
                    // 로비 데이터 업데이트
                    var updateLobbyOptions = new UpdateLobbyOptions { Data = lobbyData };
                    await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateLobbyOptions);
                    m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 로비 데이터에 릴레이 코드 저장 완료");
                }
                catch (LobbyServiceException e)
                {
                    m_DebugClassFacade?.LogError(GetType().Name, $"로비 데이터 업데이트 중 오류 발생: {e.Message}");
                }
                
                var transport = m_NetworkManager.GetComponent<UnityTransport>();
                
                // UnityTransport 컴포넌트가 없으면 추가
                if (transport == null)
                {
                    m_DebugClassFacade?.LogWarning(GetType().Name, "[LobbyConnectingState] UnityTransport 컴포넌트가 없어 추가합니다");
                    transport = m_NetworkManager.gameObject.AddComponent<UnityTransport>();
                    
                    if (transport == null)
                    {
                        m_DebugClassFacade?.LogError(GetType().Name, "[LobbyConnectingState] UnityTransport 컴포넌트 추가 실패");
                        OnConnectionFailed();
                        return;
                    }
                }
                
                // 할당값 직접 전달
                // transport.SetHostRelayData(
                //     allocation.RelayServer.IpV4,
                //     (ushort)allocation.RelayServer.Port,
                //     allocation.AllocationIdBytes,
                //     allocation.Key,
                //     allocation.ConnectionData
                // );
                
                m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] Relay 서버 데이터 설정 완료");
            }
            catch (RelayServiceException e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"Relay 연결 코드 생성 중 오류 발생: {e.Message}");
                OnConnectionFailed();
            }
            catch (NullReferenceException e)
            {
                m_DebugClassFacade?.LogError(GetType().Name, $"NullReferenceException 발생: {e.Message}");
                OnConnectionFailed();
            }
        }

        public override void StartClientLobby()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 클라이언트 로비 시작");
            m_NetworkManager.StartClient();

            // 클라이언트 상태 변경
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting);
        }

        public override void StartRelayConnection()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 릴레이 연결 시작");
            // Relay 연결 코드 구현 필요
            // var connectionMethod = new ConnectionMethodRelay(
            //     m_LobbyServiceFacade, 
            //     m_LocalLobby, 
            //     m_ConnectionManager, 
            //     m_ProfileManager, 
            //     m_PlayerName
            // );
            // m_ConnectionManager.m_ClientReconnecting.Configure(connectionMethod);
            // m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting.Configure(connectionMethod));
        }

        private string GenerateRandomLobbyName()
        {
            // 사용할 문자 집합: 알파벳 대문자와 숫자
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            // 로비 이름 길이 (예: 6자리)
            int nameLength = 6;
            
            System.Random random = new System.Random();
            char[] lobbyName = new char[nameLength];
            
            for (int i = 0; i < nameLength; i++)
            {
                lobbyName[i] = chars[random.Next(chars.Length)];
            }
            
            return new string(lobbyName);
        }
    }
}