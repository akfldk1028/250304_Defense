using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Assets.Scripts.Scene;
using Unity.Assets.Scripts.UnityServices.Lobbies;
using Unity.Netcode.Transports.UTP;
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
            ConnectClientAsync();
            Debug.Log("WHY SO SLOW");

        //    if (m_LocalLobby.LobbyUsers.Count >= m_ConnectionManager.MaxConnectedPlayers)
        //    {
        //     Debug.Log("ClientConnectingState");
        //     _sceneManagerEx.LoadScene(EScene.BasicGame.ToString(), useNetworkSceneManager: true);

        //    }

        }

        public override void Exit() { }

        public override void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[ClientConnectingState] OnClientConnected 호출됨: ClientID={clientId}");
            try
            {
                m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
                Debug.Log("[ClientConnectingState] ConnectStatus.Success 발행됨");
                m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
                Debug.Log("[ClientConnectingState] ClientConnected 상태로 전환 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ClientConnectingState] OnClientConnected 처리 중 오류: {e.Message}");
                StartingClientFailed();
            }
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


        public async Task ConnectClientAsync()
        {
            try
            {
                Debug.Log("[ClientConnectingState] 클라이언트 연결 시도 시작");
                
                // 1. 연결 설정
                await m_ConnectionMethod.SetupClientConnectionAsync();
                Debug.Log("[ClientConnectingState] SetupClientConnectionAsync 성공");
                
                // 2. NetworkManager가 준비되었는지 확인
                if (m_ConnectionManager.NetworkManager == null)
                {
                    throw new Exception("NetworkManager가 null입니다");
                }

                var transport = m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
                Debug.Log($"[ClientConnectingState] Transport 상태: {transport.GetType().Name}");
                
                // 3. 클라이언트 시작
                Debug.Log($"[ClientConnectingState] StartClient 시도 전 상태 - IsClient: {m_ConnectionManager.NetworkManager.IsClient}, IsConnectedClient: {m_ConnectionManager.NetworkManager.IsConnectedClient}, IsListening: {m_ConnectionManager.NetworkManager.IsListening}");
                
                if (!m_ConnectionManager.NetworkManager.StartClient())
                {
                    throw new Exception("NetworkManager StartClient failed");
                }
                Debug.Log("[ClientConnectingState] NetworkManager.StartClient 성공");
                
                // 4. 연결 상태 확인
                Debug.Log($"[ClientConnectingState] StartClient 이후 상태 - IsClient: {m_ConnectionManager.NetworkManager.IsClient}, IsConnectedClient: {m_ConnectionManager.NetworkManager.IsConnectedClient}, IsListening: {m_ConnectionManager.NetworkManager.IsListening}");
                
                if (!m_ConnectionManager.NetworkManager.IsClient)
                {
                    throw new Exception("클라이언트가 시작되지 않았습니다");
                }
                Debug.Log("[ClientConnectingState] 클라이언트 상태 확인 완료");
                
                // 5. Transport 설정 확인
                var utp = transport as UnityTransport;
                if (utp != null)
                {
                    Debug.Log($"[ClientConnectingState] Transport 설정 - ServerIP: {utp.ConnectionData.Address}, Port: {utp.ConnectionData.Port}, IsRelayEnabled: {utp.Protocol == UnityTransport.ProtocolType.RelayUnityTransport}");
                }
                
                // 여기서 상태를 변경하지 않고 OnClientConnected 콜백을 기다립니다
                Debug.Log("[ClientConnectingState] 연결 설정 완료, 콜백 대기 중...");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ClientConnectingState] 연결 실패: {e.Message}\n{e.StackTrace}");
                StartingClientFailed();
            }
        }
    }

