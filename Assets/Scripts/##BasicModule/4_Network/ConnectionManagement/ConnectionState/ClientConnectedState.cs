using UnityEngine;
using VContainer;
using Unity.Assets.Scripts.UnityServices.Lobbies;
using Unity.Assets.Scripts.Scene;

/// <summary>
/// Connection state corresponding to a connected client. When being disconnected, transitions to the
/// ClientReconnecting state if no reason is given, or to the Offline state.
/// </summary>
    class ClientConnectedState : OnlineState
    {
        [Inject]
        protected LobbyServiceFacade m_LobbyServiceFacade;

        [Inject]
        protected LocalLobby m_LocalLobby;

        [Inject] SceneManagerEx _sceneManagerEx;

        public override void Enter()
        {


            Debug.Log("[ClientConnectedState]");

            if (m_LobbyServiceFacade.CurrentUnityLobby != null)
            {
                    m_LobbyServiceFacade.BeginTracking();
            }

            if (m_LocalLobby.LobbyUsers.Count >= m_ConnectionManager.MaxConnectedPlayers)
           {
            Debug.Log("ClientConnectingState");
            _sceneManagerEx.LoadScene(EScene.BasicGame.ToString(), useNetworkSceneManager: true);

           }

        }

        public override void Exit() { }
        // public override void OnClientConnected(ulong _)
        // {
        //     if(m_NetworkManager.ConnectedClients.Count >= m_ConnectionManager.MaxConnectedPlayers)
        //     {
        //         ChangeSceneForAllPlayers();
        //     }
        // }
        // private void ChangeSceneForAllPlayers()
        // {
        //     // if (m_NetworkManager.IsHost)
        //     // {
        //     //     exsc.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        //     // }
        // }
        public override void OnClientDisconnect(ulong _)
        {
            var disconnectReason = m_ConnectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason) ||
                disconnectReason == "Disconnected due to host shutting down.")
            {
                m_ConnectStatusPublisher.Publish(ConnectStatus.Reconnecting);
                m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientReconnecting);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                m_ConnectStatusPublisher.Publish(connectStatus);
                m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
            }
        }
    }

