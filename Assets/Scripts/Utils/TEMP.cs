// using System;
// using UnityEngine;
// using VContainer;
// // using Unity.Assets.Scripts.UnityServices.Lobbies;
// // using Unity.Assets.Scripts.Utils;
// using UnityEngine.SceneManagement;
// // using Unity.Assets.Scripts.Gameplay.UI;
// using Unity.Netcode;
// using Unity.Services.Lobbies.Models;
// using Unity.Services.Lobbies;
// using Unity.Services.Relay;
// using System.Threading.Tasks;
// using Unity.Assets.Scripts.Scene;


// namespace Unity.Assets.Scripts.Network
// {
//     /// <summary>
//     /// 로비 연결 상태를 나타내는 클래스
//     /// 
//     /// 이 상태는 멀티플레이 연결 전 로비에서 대기하는 상태를 관리합니다.
//     /// 로비 서비스를 통해 다른 플레이어와 연결하기 전의 준비 단계입니다.
//     /// NetworkBehaviour 기능은 아직 활성화되지 않았지만, 
//     /// 로비 연결 후 네트워크 기능을 활성화하기 위한 준비를 수행합니다.
//     /// </summary>
//     internal class LobbyConnectingState : ConnectionState
//     {
//         [Inject] private DebugClassFacade _debugClassFacade;

//         [Inject] private NetworkManager m_NetworkManager;

//         [Inject] private SceneManagerEx _sceneManagerEx;
//         private const int maxPlayers = 2;
//         private Lobby currentLobby;

//         // [Inject]
//         // LobbyServiceFacade m_LobbyServiceFacade;
        
//         // [Inject]
//         // ProfileManager m_ProfileManager;
        
//         // [Inject]
//         // LocalLobby m_LocalLobby;

//         private string m_PlayerName;
//         private Action m_LoadingCompleteCallback;

//         public LobbyConnectingState Configure(string playerName)
//         {
//             m_PlayerName = playerName;
//             return this;
//         }

//         public override void Enter()
//         {
//             _debugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 시작: Enter");
//             // 로딩 완료 콜백 설정
            
//             m_LoadingCompleteCallback += OnLoadingComplete;
//         }

//         private void OnLoadingComplete()
//         {
//             _debugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 로딩 완료, 리소스 정리 및 메인 메뉴 전환 시작");
//         }

//         public async override void StartHostLobby()
//         {
//             currentLobby = await FindAvailableLobby();
//             // Matching_Object.SetActive(true);

//             if (currentLobby == null)
//             {
//                 await CreateNewLobby();
//             }
//             else
//             {
//                 await JoinLobby(currentLobby.Id);
//             }
//         }



//         private async Task<Lobby> FindAvailableLobby()
//         {
//             try
//             {
//                 var queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
//                 if (queryResponse.Results.Count > 0)
//                 {
//                     return queryResponse.Results[0];
//                 }
//             }
//             catch (LobbyServiceException e)
//             {
//                 _debugClassFacade?.LogError(GetType().Name, $"로비 조회 중 오류 발생: {e.Message}");
//             }
//             return null;
//         }

//        private async void DestroyLobby(string lobbyId)
//         {
//             try
//             {
//                 if (!string.IsNullOrEmpty(lobbyId))
//                 {
//                     await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
//                     Debug.Log("Lobby destroyed " + lobbyId);
//                 }
//                 if (m_NetworkManager.IsHost)
//                 {
//                     m_NetworkManager.Shutdown();
//                     // Matching_Object.SetActive(false);
//                 }
//             }
//             catch(System.Exception e) 
//             {
//                 _debugClassFacade?.LogError(GetType().Name, $"로비 삭제 중 오류 발생: {e.Message}");
//             }
//         }



//         public override void Exit()
//         {
//             Debug.Log("[LobbyConnectingState] 로비 연결 상태 종료");
//         }
//        /// <summary>
//         /// IP 주소를 통한 클라이언트 연결 시작 메서드
//         /// 
//         /// 오프라인 상태에서 IP 주소를 통해 서버에 직접 연결을 시도합니다.
//         /// 이 메서드는 ClientConnectingState로 전환하여 연결 프로세스를 시작해야 합니다.
//         /// </summary>
//         public override void StartClientIP(string playerName, string ipaddress, int port)
//         {
//             // var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port, m_ConnectionManager, m_ProfileManager, playerName);
//             // m_ConnectionManager.m_ClientReconnecting.Configure(connectionMethod);
//             // m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting.Configure(connectionMethod));
//         }


//         private async Task CreateNewLobby()
//         {
//         try
//             {
//                 string randomLobbyName = GenerateRandomLobbyName();
                
//                 currentLobby = await LobbyService.Instance.CreateLobbyAsync(randomLobbyName, maxPlayers);
//                 await AllocateRelayServerAndJoin(currentLobby);
//                 // CancelButton.onClick.AddListener(() => DestroyLobby(currentLobby.Id));  
//                 StartHost();
//             }
//             catch (LobbyServiceException e)
//             {
//                 _debugClassFacade?.LogError(GetType().Name, $"로비 생성 중 오류 발생: {e.Message}");
//             }
//         }

//         private async Task JoinLobby(string lobbyId)
//         {
//             try
//             {
//                 currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
//                 StartClientLobby();
//             }
//             catch (LobbyServiceException e)
//             {
//                 _debugClassFacade?.LogError(GetType().Name, $"로비 조인 중 오류 발생: {e.Message}");
//             }
//         }


//         private void StartHost()
//         {
//             m_NetworkManager.StartHost();

//             m_NetworkManager.OnClientConnectedCallback += OnClientConnected;
//             m_NetworkManager.OnClientDisconnectCallback += OnHostDisconnected;
//         }

//         public override void OnClientConnected(ulong clientId)
//         {
//             OnPlayerJoined();
//         }


//         public void OnPlayerJoined()
//         {
//             if(m_NetworkManager.ConnectedClients.Count >= maxPlayers)
//             {
//                 _sceneManagerEx.ChangeSceneForAllPlayers(EScene.BasicGame);
//             }
//         }


//         public override void OnHostDisconnected(ulong clientId)
//         {
//             if(clientId == m_NetworkManager.LocalClientId && m_NetworkManager.IsHost)
//             {
//                 NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
//                 NetworkManager.Singleton.OnClientDisconnectCallback -= OnHostDisconnected;
//             }
//         }


//         private async Task AllocateRelayServerAndJoin(Lobby lobby)
//             {
//                 //return;

//                 try
//                 {
//                     var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
//                     var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
//                     //JoinCodeText.text = joinCode;
//                     _debugClassFacade?.LogInfo(GetType().Name, $"Relay 연결 코드: {joinCode}");
//                 }
//                 catch (RelayServiceException e)
//                 {
//                     _debugClassFacade?.LogError(GetType().Name, $"Relay 연결 코드 생성 중 오류 발생: {e.Message}");
//                 }
//             }



//         public override void StartClientLobby(){
//             m_NetworkManager.StartClient();

//                         // var connectionMethod = new ConnectionMethodRelay(m_LobbyServiceFacade, m_LocalLobby, m_ConnectionManager, m_ProfileManager, playerName);
//             // m_ConnectionManager.m_ClientReconnecting.Configure(connectionMethod);
//             // m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting.Configure(connectionMethod));
//         }
    

//         public override void StartRelayConnection()
//         {
//             _debugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 릴레이 연결 시작");
//             // var connectionMethod = new ConnectionMethodRelay(
//             //     m_LobbyServiceFacade, 
//             //     m_LocalLobby, 
//             //     m_ConnectionManager, 
//             //     m_ProfileManager, 
//             //     m_PlayerName
//             // );
//             // m_ConnectionManager.m_ClientReconnecting.Configure(connectionMethod);
//             // m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting.Configure(connectionMethod));
//         }

//                 private string GenerateRandomLobbyName()
//         {
//             // 사용할 문자 집합: 알파벳 대문자와 숫자
//             const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
//             // 로비 이름 길이 (예: 6자리)
//             int nameLength = 6;
            
//             System.Random random = new System.Random();
//             char[] lobbyName = new char[nameLength];
            
//             for (int i = 0; i < nameLength; i++)
//             {
//                 lobbyName[i] = chars[random.Next(chars.Length)];
//             }
            
//             return new string(lobbyName);
//         }



//     }
// }


// private async Task CreateNewLobby(int playerLevel)
// {
//     try
//     {
//         _debugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 새 로비 생성");
//         string randomLobbyName = GenerateRandomLobbyName();
        
//         // 로비 데이터에 레벨 정보 추가
//         var lobbyOptions = new CreateLobbyOptions
//         {
//             Data = new Dictionary<string, DataObject>
//             {
//                 { "MinLevel", new DataObject(DataObject.VisibilityOptions.Public, (playerLevel - 5).ToString()) },
//                 { "MaxLevel", new DataObject(DataObject.VisibilityOptions.Public, (playerLevel + 5).ToString()) },
//                 { "HostLevel", new DataObject(DataObject.VisibilityOptions.Public, playerLevel.ToString()) }
//             }
//         };
        
//         currentLobby = await LobbyService.Instance.CreateLobbyAsync(randomLobbyName, maxPlayers, lobbyOptions);
//         // 나머지 코드는 동일
//     }
//     catch (LobbyServiceException e)
//     {
//         // 에러 처리
//     }
// }

// private async Task<Lobby> FindAvailableLobby(int playerLevel)
// {
//     try
//     {
//         _debugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 레벨에 맞는 로비 검색");
        
//         // 레벨 범위 설정 (예: 플레이어 레벨 ±5)
//         int minLevel = playerLevel - 5;
//         int maxLevel = playerLevel + 5;
        
//         // 로비 검색 옵션 설정
//         QueryLobbiesOptions options = new QueryLobbiesOptions
//         {
//             Filters = new List<QueryFilter>
//             {
//                 new QueryFilter(
//                     field: QueryFilter.FieldOptions.AvailableSlots,
//                     op: QueryFilter.OpOptions.GT,
//                     value: "0"),
                
//                 // 레벨 범위 필터 추가
//                 new QueryFilter(
//                     field: QueryFilter.FieldOptions.Data,
//                     op: QueryFilter.OpOptions.GE,
//                     key: "MinLevel", 
//                     value: minLevel.ToString()),
//                 new QueryFilter(
//                     field: QueryFilter.FieldOptions.Data,
//                     op: QueryFilter.OpOptions.LE,
//                     key: "MaxLevel",
//                     value: maxLevel.ToString())
//             }
//         };
        
//         var queryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
//         if (queryResponse.Results.Count > 0)
//         {
//             _debugClassFacade?.LogInfo(GetType().Name, $"[LobbyConnectingState] 적합한 로비 발견: {queryResponse.Results[0].Id}");
//             return queryResponse.Results[0];
//         }
//     }
//     catch (LobbyServiceException e)
//     {
//         _debugClassFacade?.LogError(GetType().Name, $"로비 조회 중 오류 발생: {e.Message}");
//     }
    
//     _debugClassFacade?.LogInfo(GetType().Name, "[LobbyConnectingState] 적합한 로비 없음");
//     return null;
// }
