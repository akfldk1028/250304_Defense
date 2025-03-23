// using System;
// using Unity.Netcode;
// using UnityEngine;
// using VContainer;
// using Unity.Assets.Scripts.UnityServices.Lobbies;
// using Unity.Assets.Scripts.Scene;


// /// <summary>
// /// Connection state corresponding to a listening host. Handles incoming client connections. When shutting down or
// /// being timed out, transitions to the Offline state.
// /// </summary>
//     class HostingState : OnlineState
//     {

//         [Inject]
//         protected LocalLobby m_LocalLobby;

//         [Inject] SceneManagerEx _sceneManagerEx;

//         [Inject]
//         LobbyServiceFacade m_LobbyServiceFacade;

//         [Inject]
//         IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

//         [Inject] DebugClassFacade m_DebugClassFacade;
//         // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
//         const int k_MaxConnectPayload = 1024;

//         public override void Enter()
//         {
//         //The "BossRoom" server always advances to CharSelect immediately on start. Different games
//         //may do this differently.
//         // SceneLoaderWrapper.Instance.LoadScene("CharSelect", useNetworkSceneManager: true);

//         if (m_LocalLobby.LobbyUsers.Count >= m_ConnectionManager.MaxConnectedPlayers)
//         {
//             Debug.Log(" HostingState HostingState HostingState HostingStateㅎㅎ HostingState");
//             _sceneManagerEx.LoadScene(EScene.BasicGame.ToString(), useNetworkSceneManager: true);

//         }


//         if (m_LobbyServiceFacade.CurrentUnityLobby != null)
//             {
//                 m_LobbyServiceFacade.BeginTracking();
//             }
//         }

//         public override void Exit()
//         {
//             SessionManager<SessionPlayerData>.Instance.OnServerEnded();
//         }

//         public override void OnClientConnected(ulong clientId)
//         {
//             Debug.Log("[HostingState] OnClientConnected 호출됨: ClientID=" + clientId);
//             // var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
//             // if (playerData != null)
//             // {
//             //     m_ConnectionEventPublisher.Publish(new ConnectionEventMessage() { ConnectStatus = ConnectStatus.Success, ClientId = clientId });
//             // }
//             // else
//             // {
//             //     // This should not happen since player data is assigned during connection approval
//             //     m_DebugClassFacade.LogInfo(GetType().Name,$"No player data associated with client {clientId}");
//             //     var reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
//             //     m_ConnectionManager.NetworkManager.DisconnectClient(clientId, reason);
//             // }
//             // Debug.Log($"[HostingState] playerData {playerData}");


//             if (m_LocalLobby.LobbyUsers.Count >= m_ConnectionManager.MaxConnectedPlayers)
//             {
//             Debug.Log(" HostingState HostingState HostingState HostingStateㅎㅎ HostingState");
//             _sceneManagerEx.LoadScene(EScene.BasicGame.ToString(), useNetworkSceneManager: true);
//             }
//         }

//         public override void OnClientDisconnect(ulong clientId)
//         {
//             if (clientId != m_ConnectionManager.NetworkManager.LocalClientId)
//             {
//                 var playerId = SessionManager<SessionPlayerData>.Instance.GetPlayerId(clientId);
//                 if (playerId != null)
//                 {
//                     var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(playerId);
//                     if (sessionData.HasValue)
//                     {
//                         m_ConnectionEventPublisher.Publish(new ConnectionEventMessage() { ConnectStatus = ConnectStatus.GenericDisconnect, ClientId=clientId});
//                     }
//                     SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
//                 }
//             }
//         }

//         public override void OnUserRequestedShutdown()
//         {
//             var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
//             for (var i = m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
//             {
//                 var id = m_ConnectionManager.NetworkManager.ConnectedClientsIds[i];
//                 if (id != m_ConnectionManager.NetworkManager.LocalClientId)
//                 {
//                     m_ConnectionManager.NetworkManager.DisconnectClient(id, reason);
//                 }
//             }
//             m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
//         }

//         public override void OnServerStopped()
//         {
//             m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
//             m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
//         }

//         /// <summary>
//         /// This logic plugs into the "ConnectionApprovalResponse" exposed by Netcode.NetworkManager. It is run every time a client connects to us.
//         /// The complementary logic that runs when the client starts its connection can be found in ClientConnectingState.
//         /// </summary>
//         /// <remarks>
//         /// Multiple things can be done here, some asynchronously. For example, it could authenticate your user against an auth service like UGS' auth service. It can
//         /// also send custom messages to connecting users before they receive their connection result (this is useful to set status messages client side
//         /// when connection is refused, for example).
//         /// Note on authentication: It's usually harder to justify having authentication in a client hosted game's connection approval. Since the host can't be trusted,
//         /// clients shouldn't send it private authentication tokens you'd usually send to a dedicated server.
//         /// </remarks>
//         /// <param name="request"> The initial request contains, among other things, binary data passed into StartClient. In our case, this is the client's GUID,
//         /// which is a unique identifier for their install of the game that persists across app restarts.
//         ///  <param name="response"> Our response to the approval process. In case of connection refusal with custom return message, we delay using the Pending field.
      
//         public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
//         {
//             Debug.Log($"[HostingState] 클라이언트 승인 요청: ClientID={request.ClientNetworkId}");
            
//             // 간단한 테스트를 위해 모든 클라이언트 승인
//             response.Approved = true;

//             Debug.Log($"연결 승인 요청 - ClientID: {request.ClientNetworkId}, 요청 데이터 길이: {request.Payload.Length}");


//             Debug.Log($"[HostingState] 클라이언트 승인 완료: ClientID={request.ClientNetworkId}");
//         }
      
//         // public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
//         // {
//         //     Debug.Log("[HostingState] ApprovalCheck 호출됨");
//         //     var connectionData = request.Payload;
//         //     var clientId = request.ClientNetworkId;
//         //     if (connectionData.Length > k_MaxConnectPayload)
//         //     {
//         //         // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
//         //         // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
//         //         response.Approved = false;
//         //         return;
//         //     }

//         //     var payload = System.Text.Encoding.UTF8.GetString(connectionData);
//         //     var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
//         //     var gameReturnStatus = GetConnectStatus(connectionPayload);

//         //     if (gameReturnStatus == ConnectStatus.Success)
//         //     {
//         //         SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
//         //             new SessionPlayerData(clientId, connectionPayload.playerName, new NetworkGuid(), 0, true));

//         //         // connection approval will create a player object for you
//         //         response.Approved = true;
//         //         response.CreatePlayerObject = true;
//         //         response.Position = Vector3.zero;
//         //         response.Rotation = Quaternion.identity;
//         //         return;
//         //     }

//         //     response.Approved = false;
//         //     response.Reason = JsonUtility.ToJson(gameReturnStatus);
//         //     if (m_LobbyServiceFacade.CurrentUnityLobby != null)
//         //     {
//         //         m_LobbyServiceFacade.RemovePlayerFromLobbyAsync(connectionPayload.playerId);
//         //     }
//         // }

//         ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
//         {
//             if (m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= m_ConnectionManager.MaxConnectedPlayers)
//             {
//                 return ConnectStatus.ServerFull;
//             }

//             if (connectionPayload.isDebug != Debug.isDebugBuild)
//             {
//                 return ConnectStatus.IncompatibleBuildType;
//             }

//             return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId) ?
//                 ConnectStatus.LoggedInAgain : ConnectStatus.Success;
//         }
//     }

using System;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Unity.Assets.Scripts.UnityServices.Lobbies;
using Unity.Assets.Scripts.Scene;


/// <summary>
/// Connection state corresponding to a listening host. Handles incoming client connections. When shutting down or
/// being timed out, transitions to the Offline state.
/// </summary>
class HostingState : OnlineState
{
    [Inject]
    protected LocalLobby m_LocalLobby;

    [Inject] SceneManagerEx _sceneManagerEx;

    [Inject]
    LobbyServiceFacade m_LobbyServiceFacade;

    [Inject]
    IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

    [Inject] DebugClassFacade m_DebugClassFacade;
    
    // 연결 상태 체크 코루틴
    private Coroutine m_ConnectionStatusCheckCoroutine;
    private float lastSceneLoadAttemptTime = 0f;

    public override void Enter()
    {
        Debug.Log("[HostingState] Enter - 호스트 모드로 진입");
        
        // 로비 추적 시작
        if (m_LobbyServiceFacade.CurrentUnityLobby != null)
        {
            m_LobbyServiceFacade.BeginTracking();
            Debug.Log("[HostingState] 로비 추적 시작");
        }

        // 플레이어 수 확인 및 씬 전환
        CheckPlayersAndLoadScene();
        
        // 연결 상태 모니터링 시작
        StartPlayerCountMonitoring();
    }

    // 플레이어 수 확인 및 필요시 씬 전환
    private void CheckPlayersAndLoadScene()
    {
        int currentPlayerCount = m_LocalLobby.LobbyUsers.Count;
        Debug.Log($"[HostingState] 현재 플레이어 수: {currentPlayerCount}/{m_ConnectionManager.MaxConnectedPlayers}");

        // 충분한 플레이어가 있고, 마지막 씬 로드 시도 후 5초가 지났다면
        if (currentPlayerCount >= m_ConnectionManager.MaxConnectedPlayers && 
            Time.time - lastSceneLoadAttemptTime > 5f)
        {
            Debug.Log("[HostingState] 플레이어 수 충족 - 게임 씬으로 전환");
            lastSceneLoadAttemptTime = Time.time;
            _sceneManagerEx.LoadScene(EScene.BasicGame.ToString(), useNetworkSceneManager: true);
        }
    }

    // 주기적으로 플레이어 수 확인
    private void StartPlayerCountMonitoring()
    {
        m_ConnectionStatusCheckCoroutine = m_ConnectionManager.StartCoroutine(
            MonitorPlayerCount());
    }

    private System.Collections.IEnumerator MonitorPlayerCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.0f);
            CheckPlayersAndLoadScene();
        }
    }

    public override void Exit()
    {
        // 코루틴 정리
        if (m_ConnectionStatusCheckCoroutine != null)
        {
            m_ConnectionManager.StopCoroutine(m_ConnectionStatusCheckCoroutine);
            m_ConnectionStatusCheckCoroutine = null;
        }
        
        // 세션 정리
        SessionManager<SessionPlayerData>.Instance.OnServerEnded();
    }

    public override void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[HostingState] 클라이언트 연결됨: ClientID={clientId}");
        
        // 간단한 세션 데이터 생성
        try
        {
            // 기본 세션 데이터 생성 (자세한 검증 없이)
            string playerId = clientId.ToString(); // 간단하게 클라이언트 ID를 플레이어 ID로 사용
            string playerName = $"Player_{clientId}"; // 기본 이름 부여
            Debug.Log($"[HostingState] 클라이언트 {clientId} 세션 데이터 설정 중 ####################################");
            // 세션 데이터 설정
            SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(
                clientId, 
                playerId,
                new SessionPlayerData(clientId, playerName, new NetworkGuid(), 0, true)
            );
            
            // 연결 성공 이벤트 발행
            m_ConnectionEventPublisher.Publish(new ConnectionEventMessage 
            { 
                ConnectStatus = ConnectStatus.Success, 
                ClientId = clientId 
            });
            
            Debug.Log($"[HostingState] 클라이언트 {clientId} 세션 데이터 설정 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[HostingState] 클라이언트 {clientId} 세션 데이터 설정 중 오류: {e.Message}");
        }
        
        // 플레이어 수 확인 및 씬 전환
        CheckPlayersAndLoadScene();
    }

    public override void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"[HostingState] 클라이언트 연결 해제: ClientID={clientId}");
        
        if (clientId != m_ConnectionManager.NetworkManager.LocalClientId)
        {
            try
            {
                // 연결 해제 이벤트 발행
                m_ConnectionEventPublisher.Publish(new ConnectionEventMessage 
                { 
                    ConnectStatus = ConnectStatus.GenericDisconnect, 
                    ClientId = clientId 
                });
                
                // 세션에서 클라이언트 제거
                SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
                Debug.Log($"[HostingState] 클라이언트 {clientId} 세션에서 제거됨");
            }
            catch (Exception e)
            {
                Debug.LogError($"[HostingState] 클라이언트 {clientId} 연결 해제 처리 중 오류: {e.Message}");
            }
        }
    }

    public override void OnUserRequestedShutdown()
    {
        var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
        for (var i = m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
        {
            var id = m_ConnectionManager.NetworkManager.ConnectedClientsIds[i];
            if (id != m_ConnectionManager.NetworkManager.LocalClientId)
            {
                m_ConnectionManager.NetworkManager.DisconnectClient(id, reason);
            }
        }
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }

    public override void OnServerStopped()
    {
        m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }

    public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log($"[HostingState] 클라이언트 승인 요청: ClientID={request.ClientNetworkId}");
        
        try
        {
            // 무조건 승인 - 가장 단순한 방식으로 모든 클라이언트 허용
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
            
            Debug.Log($"[HostingState] 클라이언트 승인 완료: ClientID={request.ClientNetworkId}");
            
            // 추가: 플레이어 데이터 설정 시도
            // if (request.Payload != null && request.Payload.Length > 0)
            // {
            //     try
            //     {
            //         var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            //         var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
                    
            //         if (connectionPayload != null && !string.IsNullOrEmpty(connectionPayload.playerId))
            //         {
            //             // 페이로드에서 플레이어 데이터를 추출하여 설정
            //             SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(
            //                 request.ClientNetworkId, 
            //                 connectionPayload.playerId,
            //                 new SessionPlayerData(
            //                     request.ClientNetworkId, 
            //                     connectionPayload.playerName ?? $"Player_{request.ClientNetworkId}", 
            //                     new NetworkGuid(), 
            //                     0, 
            //                     true
            //                 )
            //             );
                        
            //             Debug.Log($"[HostingState] 클라이언트 {request.ClientNetworkId}의 플레이어 데이터 설정됨: {connectionPayload.playerName}");
            //         }
            //     }
            //     catch (Exception e)
            //     {
            //         // 페이로드 파싱 중 오류가 발생해도 연결은 승인
            //         Debug.LogWarning($"[HostingState] 페이로드 파싱 중 오류 (무시됨): {e.Message}");
            //     }
            // }
        }
        catch (Exception ex)
        {
            // 오류가 발생해도 최대한 연결 승인 시도
            Debug.LogError($"[HostingState] 승인 처리 중 오류: {ex.Message}, 연결 강제 승인 시도");
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
        }
    }
}