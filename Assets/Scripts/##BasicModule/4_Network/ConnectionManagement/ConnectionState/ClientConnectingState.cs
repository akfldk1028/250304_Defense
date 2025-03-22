using System;
using System.Threading.Tasks;
using Unity.Assets.Scripts.Scene;
using Unity.Assets.Scripts.UnityServices.Lobbies;
using UnityEngine;
using VContainer;

/// <summary>
/// Connection state corresponding to when a client is attempting to connect to a server. Starts the client when
/// entering. If successful, transitions to the ClientConnected state. If not, transitions to the Offline state.
/// </summary>
class ClientConnectingState : OnlineState
    {
        protected ConnectionMethodBase m_ConnectionMethod;

        [Inject]
        protected LocalLobby m_LocalLobby;

        [Inject] SceneManagerEx _sceneManagerEx;

    public ClientConnectingState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            m_ConnectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
#pragma warning disable 4014
            ConnectClientAsync();
#pragma warning restore 4014

           if (m_LocalLobby.LobbyUsers.Count >= MaxConnectedPlayers)
           {
            Debug.Log("ÃP¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾¤¾");
            _sceneManagerEx.LoadScene(EScene.BasicGame.ToString(), useNetworkSceneManager: true);

           }

        }

        public override void Exit() { }

        public override void OnClientConnected(ulong _)
        {
            m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
        }

        public override void OnClientDisconnect(ulong _)
        {
            // client ID is for sure ours here
            StartingClientFailed();
        }

        public override void OnTransportFailure()
        {
            StartingClientFailed();
        }

        void StartingClientFailed()
        {
            var disconnectReason = m_ConnectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason))
            {
                m_ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                m_ConnectStatusPublisher.Publish(connectStatus);
            }
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_LobbyConnecting);
        }


        internal async Task ConnectClientAsync()
        {
            try
            {
                // Setup NGO with current connection method
                await m_ConnectionMethod.SetupClientConnectionAsync();

                // NGO's StartClient launches everything
                if (!m_ConnectionManager.NetworkManager.StartClient())
                {
                    throw new Exception("NetworkManager StartClient failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error connecting client, see following exception");
                Debug.LogException(e);
                StartingClientFailed();
                throw;
            }
        }
    }

