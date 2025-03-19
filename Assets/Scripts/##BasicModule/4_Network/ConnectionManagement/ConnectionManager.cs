using System;
using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;


/// <summary>
/// 네트워크 연결 시 전달되는 페이로드 클래스
/// 플레이어 식별 및 디버그 정보 포함
/// </summary>
[Serializable]
public class ConnectionPayload
{
    public string playerId;    // 플레이어 고유 ID
    public string playerName;  // 플레이어 이름
    public bool isDebug;       // 디버그 모드 여부
}

/// <summary>
/// 네트워크 연결 관리자 클래스
/// Unity NGO(Netcode for GameObjects)를 사용한 네트워크 연결 관리
/// 상태 패턴을 사용하여 다양한 연결 상태 처리
/// 
/// NetworkBehaviour를 상속받아 RPC 기능을 제공하며, 상태 패턴과 통합되어 있습니다.
/// 각 상태 클래스는 이 클래스의 RPC 메서드를 간접적으로 호출하여 네트워크 통신을 수행합니다.
/// 
/// 주요 기능:
/// 1. 네트워크 연결 상태 관리 (상태 패턴)
/// 2. 서버-클라이언트 간 통신 (RPC)
/// 3. 연결 승인 및 재연결 처리
/// 4. 네트워크 이벤트 처리
/// </summary>
/// 

namespace Unity.Assets.Scripts.Network
{
    public class ConnectionManager : NetworkBehaviour
    {
        
        [SerializeField]
        private ConnectionMode m_ConnectionMode = ConnectionMode.Hybrid;  // 기본값은 혼합 모드

        // 현재 연결 상태를 관리하는 상태 객체
        ConnectionState m_CurrentState;

        [Inject]
        NetworkManager m_NetworkManager;          // Unity NGO의 네트워크 매니저
        public NetworkManager NetworkManager => m_NetworkManager;

        [SerializeField]
        int m_NbReconnectAttempts = 2;           // 최대 재연결 시도 횟수
        public int NbReconnectAttempts => m_NbReconnectAttempts;

        [Inject]
        IObjectResolver m_Resolver;               // VContainer의 의존성 해결사

        // 최대 동시 접속 플레이어 수
        public int MaxConnectedPlayers = 8;

        // 상태 패턴을 위한 상태 객체들
        internal readonly OfflineState m_Offline = new OfflineState();                    // 오프라인 상태
        internal readonly LobbyConnectingState m_LobbyConnecting = new LobbyConnectingState();    // 로비 연결 상태
        internal readonly ClientConnectingState m_ClientConnecting = new ClientConnectingState();  // 클라이언트 연결 중
        internal readonly ClientConnectedState m_ClientConnected = new ClientConnectedState();    // 클라이언트 연결됨
        internal readonly ClientReconnectingState m_ClientReconnecting = new ClientReconnectingState();  // 재연결 중
        internal readonly StartingHostState m_StartingHost = new StartingHostState();      // 호스트 시작 중
        internal readonly HostingState m_Hosting = new HostingState();                    // 호스팅 중

        /// <summary>
        /// 씬 전환 시에도 파괴되지 않도록 설정
        /// </summary>
        void Awake()
        {
            Debug.Log("[ConnectionManager] 시작: Awake");
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ConnectionManager] 종료: Awake");
        }

        /// <summary>
        /// 초기화 및 이벤트 핸들러 등록
        /// </summary>
        void Start()
        {
            Debug.Log("[ConnectionManager] 시작: Start");
            if (NetworkManager == null)
            {
                // Debug.LogError("[ConnectionManager] NetworkManager가 할당되지 않았습니다!");
                // PassIdentifier
                return;
            }

            Debug.Log("[ConnectionManager] 상태 객체 초기화 시작");
            List<ConnectionState> states = new()
            {
                m_Offline,
                m_ClientConnecting,
                m_ClientConnected,
                m_ClientReconnecting,
                m_StartingHost,
                m_Hosting,
                m_LobbyConnecting
            };

            foreach (var connectionState in states)
            {
                Debug.Log($"[ConnectionManager] 상태 주입: {connectionState.GetType().Name}");
                m_Resolver.Inject(connectionState);
            }

            Debug.Log("[ConnectionManager] 초기 상태 설정: Offline");
            m_CurrentState = m_Offline;

            Debug.Log("[ConnectionManager] 네트워크 이벤트 핸들러 등록");
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.OnTransportFailure += OnTransportFailure;
            NetworkManager.OnServerStopped += OnServerStopped;
            Debug.Log("[ConnectionManager] 종료: Start");
        }

        /// <summary>
        /// NetworkBehaviour의 OnNetworkSpawn 메서드 오버라이드
        /// 네트워크 오브젝트가 스폰될 때 호출됨
        /// 
        /// 서버, 클라이언트, 호스트에 따라 다른 초기화 로직을 수행합니다.
        /// 이 메서드는 네트워크 오브젝트가 생성된 후 자동으로 호출됩니다.
        /// </summary>
    /// <summary>
    /// 연결 상태 변경 메서드
    /// 상태 패턴의 핵심 구현부
    /// </summary>
    internal void ChangeState(ConnectionState nextState)
    {
        if (m_ConnectionMode == ConnectionMode.OnlineRequired)
            {
                if (nextState is OfflineState)
                {
                    Debug.LogError("[ConnectionManager] 온라인 전용 모드에서는 오프라인 상태로 전환할 수 없습니다. 재연결을 시도합니다.");
                    // 대신 재연결 시도
                    m_CurrentState = m_ClientReconnecting;
                    m_CurrentState.Enter();
                    return;
                }
            }



        Debug.Log($"[ConnectionManager] 상태 변경: {m_CurrentState.GetType().Name} -> {nextState.GetType().Name}");
        
        if (m_CurrentState != null)
        {
            Debug.Log($"[ConnectionManager] 이전 상태 종료: {m_CurrentState.GetType().Name}");
            m_CurrentState.Exit();
        }
        m_CurrentState = nextState;
        Debug.Log($"[ConnectionManager] 새로운 상태 시작: {nextState.GetType().Name}");
        m_CurrentState.Enter();
    }

    // NGO 이벤트 핸들러들
    void OnClientDisconnectCallback(ulong clientId)
    {
        Debug.Log($"[ConnectionManager] 클라이언트 연결 해제: ClientID={clientId}");
        m_CurrentState.OnClientDisconnect(clientId);
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"[ConnectionManager] 클라이언트 연결됨: ClientID={clientId}");
        m_CurrentState.OnClientConnected(clientId);
    }

    void OnServerStarted()
    {
        Debug.Log("[ConnectionManager] 서버 시작됨");
        m_CurrentState.OnServerStarted();
    }

    void OnTransportFailure()
    {
        Debug.LogError("[ConnectionManager] 전송 실패 발생");
        m_CurrentState.OnTransportFailure();
    }

    void OnServerStopped(bool _)
    {
        Debug.Log("[ConnectionManager] 서버 중지됨");
        m_CurrentState.OnServerStopped();
    }

    /// <summary>
    /// 클라이언트 연결 승인 검사
    /// </summary>
    void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log($"[ConnectionManager] 연결 승인 검사: ClientID={request.ClientNetworkId}");
        m_CurrentState.ApprovalCheck(request, response);
    }

    /// <summary>
    /// 로비를 통한 클라이언트 연결 시작
    /// </summary>
    public void StartClientLobby(string playerName)
    {
        Debug.Log($"[ConnectionManager] 로비 클라이언트 시작: PlayerName={playerName}");
        m_CurrentState.StartClientLobby(playerName);
    }

    /// <summary>
    /// IP 주소를 통한 직접 연결 시작
    /// </summary>
    public void StartClientIp(string playerName, string ipaddress, int port)
    {
        Debug.Log($"[ConnectionManager] IP 클라이언트 시작: PlayerName={playerName}, IP={ipaddress}, Port={port}");
        m_CurrentState.StartClientIP(playerName, ipaddress, port);
    }
    }
}

