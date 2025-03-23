using System;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using VContainer;
using Unity.Assets.Scripts.UnityServices.Lobbies;
using Unity.Assets.Scripts.Network;
using Unity.Assets.Scripts.Auth;
using System.Threading.Tasks;
using Unity.Assets.Scripts.Scene;
using System.Collections;

public class LobbyUIMediator : MonoBehaviour
    {
   
        // 로비 생성 완료 이벤트
        public event Action OnLobbyCreated;
        
        // 로비 참가 완료 이벤트
        public event Action OnLobbyJoined;

        AuthManager m_AuthManager;
        LobbyServiceFacade m_LobbyServiceFacade;
        LocalLobbyUser m_LocalUser;
        LocalLobby m_LocalLobby;
        ConnectionManager m_ConnectionManager;
        ISubscriber<ConnectStatus> m_ConnectStatusSubscriber;

        [Inject] DebugClassFacade m_DebugClassFacade;

        [Inject] SceneManagerEx _sceneManagerEx;
        const string k_DefaultLobbyName = "no-name";

        [Inject]
        void InjectDependenciesAndInitialize(
            AuthManager authenticationServiceFacade,
            LobbyServiceFacade lobbyServiceFacade,
            LocalLobbyUser localUser,
            LocalLobby localLobby,
            ISubscriber<ConnectStatus> connectStatusSub,
            ConnectionManager connectionManager,
            SceneManagerEx sceneManagerEx,
            DebugClassFacade debugClassFacade
        )
        {
            m_AuthManager = authenticationServiceFacade;
            m_LocalUser = localUser;
            m_LobbyServiceFacade = lobbyServiceFacade;
            m_LocalLobby = localLobby;
            m_ConnectionManager = connectionManager;
            m_ConnectStatusSubscriber = connectStatusSub;
            RegenerateName();
            m_ConnectStatusSubscriber.Subscribe(OnConnectStatus);
            _sceneManagerEx = sceneManagerEx;
            m_DebugClassFacade = debugClassFacade;
        }

        void OnConnectStatus(ConnectStatus status)
        {
            if (status is ConnectStatus.GenericDisconnect or ConnectStatus.StartClientFailed)
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        public async void ClosedLobbyByHost()
        {
            // await m_LobbyServiceFacade.DeleteLobbyAsync();
            UnblockUIAfterLoadingIsComplete();

                //    try
                // {
                //     if (!string.IsNullOrEmpty(lobbyId))
                //     {
                //         await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                //         Debug.Log("Lobby destroyed " + lobbyId);
                //     }
                //     if (NetworkManager.Singleton.IsHost)
                //     {
                //         NetworkManager.Singleton.Shutdown();
                //         Matching_Object.SetActive(false);
                //     }
                // }
                // catch(System.Exception e) 
                // {
                //     Debug.LogError("Failed to destroy lobby " + e.Message);

                // }
        }

        void OnDestroy()
        {
            if (m_ConnectStatusSubscriber != null)
            {
                m_ConnectStatusSubscriber.Unsubscribe(OnConnectStatus);
            }
        }

        //Lobby and Relay calls done from UI

        public async void CreateLobbyRequest(string lobbyName, bool isPrivate)
        {
            // before sending request to lobby service, populate an empty lobby name, if necessary
            if (string.IsNullOrEmpty(lobbyName))
            {
                lobbyName = k_DefaultLobbyName;
            }

            BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await m_AuthManager.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            var lobbyCreationAttempt = await m_LobbyServiceFacade.TryCreateLobbyAsync(lobbyName, m_LobbyServiceFacade.MaxConnectedPlayers, isPrivate);

            if (lobbyCreationAttempt.Success)
            {
                m_LocalUser.IsHost = true;
                m_LobbyServiceFacade.SetRemoteLobby(lobbyCreationAttempt.Lobby);

                Debug.Log($"Created lobby with ID: {m_LocalLobby.LobbyID} and code {m_LocalLobby.LobbyCode}");
                m_ConnectionManager.StartHostLobby(m_LocalUser.DisplayName);
                
                // 로비 생성 완료 이벤트 발생
                OnLobbyCreated?.Invoke();
            }
            // else
            // {
            //     UnblockUIAfterLoadingIsComplete();
            // }
        }

        public async Task<(bool Success, Unity.Services.Lobbies.Models.Lobby Lobby)> QueryLobbiesRequest(bool blockUI)
        {
            Debug.Log($"[LobbyUIMediator] 로비 검색 요청 - UI 블록: {blockUI}");
            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.LogWarning("[LobbyUIMediator] Unity Services가 초기화되지 않음");
                return (false, null);
            }



            bool playerIsAuthorized = await m_AuthManager.EnsurePlayerIsAuthorized();

            if (blockUI && !playerIsAuthorized)
            {
                Debug.LogWarning("[LobbyUIMediator] 플레이어 인증 실패");
                UnblockUIAfterLoadingIsComplete();
                return (false, null);
            }

            var (success, lobby) = await m_LobbyServiceFacade.FindAvailableLobby();
            Debug.Log($"[LobbyUIMediator] 로비 검색 완료 - 성공: {success}");


            return (success, lobby);
        }

        public async void JoinLobbyWithCodeRequest(string lobbyCode)
        {
            BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await m_AuthManager.EnsurePlayerIsAuthorized();
            Debug.Log($"[LobbyUIMediator] 플레이어 인증 결과: {playerIsAuthorized}#####################################");
            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }
            Debug.Log($"[LobbyUIMediator] 로비 참가 요청 - 로비 코드: {lobbyCode}");
            var (success, lobby) = await m_LobbyServiceFacade.TryJoinLobbyAsync(null, lobbyCode);
            Debug.Log($"[LobbyUIMediator] 로비 참가 요청 결과 - 성공: {success}");

            m_DebugClassFacade.LogInfo(GetType().Name, $"JoinLobbyWithCodeRequest 요청 - 로비 코드@!@@@@: {lobbyCode}");

            if (success == true && lobby != null)
            {
                OnJoinedLobby(lobby);
            }
            else
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        public async void JoinLobbyRequest(LocalLobby lobby)
        {
            BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await m_AuthManager.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await m_LobbyServiceFacade.TryJoinLobbyAsync(lobby.LobbyID, lobby.LobbyCode);

            if (result.Success)
            {
                OnJoinedLobby(result.Lobby);
            }
            else
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        public async void QuickJoinRequest()
        {
            BlockUIWhileLoadingIsInProgress();

            bool playerIsAuthorized = await m_AuthManager.EnsurePlayerIsAuthorized();
            Debug.Log($"[LobbyUIMediator] 플레이어 인증 결과: {playerIsAuthorized}아 시방방");
            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            var result = await m_LobbyServiceFacade.TryQuickJoinLobbyAsync();
            Debug.Log($"[LobbyUIMediator] QuickJoinRequest() 로비 참가 요청 결과 - 성공: {result.Success}");
            if (result.Success)
            {
                OnJoinedLobby(result.Lobby);
            }
            else
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        void OnJoinedLobby(Unity.Services.Lobbies.Models.Lobby remoteLobby)
        {
            m_LobbyServiceFacade.SetRemoteLobby(remoteLobby);
            Debug.Log($"[LobbyUIMediator]  Joined lobby with code: {m_LocalLobby.LobbyCode}");
            
            // 로비 참가 완료 이벤트 발생
            OnLobbyJoined?.Invoke();
            
            if (m_LocalUser.IsHost)
            {
                m_ConnectionManager.StartHostLobby(m_LocalUser.DisplayName);
            }
            else
            {
                m_ConnectionManager.StartClientLobby(m_LocalUser.DisplayName);
            }

            
            // 모든 플레이어가 준비되면 씬 전환
            //m_LobbyServiceFacade.SceneLoad();
        }

 
      

     
    

  
        public void RegenerateName()
        {
            m_LocalUser.DisplayName = "Player" ;
        }
        public static event Action<bool> OnWaitingStateChanged; // true: 대기 시작, false: 대기 종료

        void BlockUIWhileLoadingIsInProgress()
        {
            // m_CanvasGroup.interactable = false;
            // m_LoadingSpinner.SetActive(true);
            OnWaitingStateChanged?.Invoke(true);
        }
        void UnblockUIAfterLoadingIsComplete()
        {
            //this callback can happen after we've already switched to a different scene
            //in that case the canvas group would be null
            // if (m_CanvasGroup != null)
            // {
            //     m_CanvasGroup.interactable = true;
            //     m_LoadingSpinner.SetActive(false);
            // }
            OnWaitingStateChanged?.Invoke(false);
        }
    }
