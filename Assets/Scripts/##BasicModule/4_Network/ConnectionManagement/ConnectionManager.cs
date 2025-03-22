using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;

    /// <summary>
    /// 앱의 연결 모드를 정의하는 열거형
    /// </summary>
    public enum ConnectionMode
    {
        OfflineOnly,    // 오프라인 전용
        OnlineRequired, // 온라인 필수
        Hybrid          // 혼합 모드
    }
    /// <summary>
    /// 네트워크 연결 상태를 나타내는 열거형
    /// 클라이언트와 호스트의 다양한 연결 상태를 정의
    /// </summary>
    public enum ConnectStatus
    {
        Undefined,               // 초기 상태
        Success,                // 연결 성공
        ServerFull,            // 서버가 가득 참
        LoggedInAgain,         // 다른 곳에서 로그인됨
        UserRequestedDisconnect, // 사용자가 연결 종료 요청
        GenericDisconnect,     // 일반적인 연결 종료
        Reconnecting,          // 재연결 시도 중
        IncompatibleBuildType, // 빌드 타입 불일치
        HostEndedSession,      // 호스트가 세션 종료
        StartHostFailed,       // 호스트 시작 실패
        StartClientFailed,      // 클라이언트 시작 실패
          Disconnected,
        Connecting,
        Connected,
        Failed,
 
    }

    /// <summary>
    /// 재연결 시도 정보를 담는 구조체
    /// 현재 시도 횟수와 최대 시도 횟수를 포함
    /// </summary>
    public struct ReconnectMessage
    {
        public int CurrentAttempt;  // 현재 재연결 시도 횟수
        public int MaxAttempt;      // 최대 재연결 시도 횟수

        public ReconnectMessage(int currentAttempt, int maxAttempt)
        {
            CurrentAttempt = currentAttempt;
            MaxAttempt = maxAttempt;
        }
    }

    /// <summary>
    /// 연결 이벤트 메시지 구조체
    /// 네트워크로 직렬화 가능한 연결 상태 정보
    /// </summary>
    public struct ConnectionEventMessage : INetworkSerializeByMemcpy
    {
        public ulong ClientId;

        public ConnectStatus ConnectStatus;  // 현재 연결 상태
    }

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
    public class ConnectionManager : MonoBehaviour
    {
        // 연결 상태 변경 이벤트
        public int MaxConnectedPlayers = 2;
        public event System.Action<ConnectStatus> OnConnectionStatusChanged;

        [SerializeField]
        private ConnectionMode m_ConnectionMode = ConnectionMode.OnlineRequired;  // 기본값은 혼합 모드

        // 현재 연결 상태를 관리하는 상태 객체
        ConnectionState m_CurrentState;
        [Inject] private DebugClassFacade m_DebugClassFacade;

        [Inject] private NetworkManager m_NetworkManager;          // Unity NGO의 네트워크 매니저
        [Inject] IObjectResolver m_Resolver;               // VContainer의 의존성 해결사
        [Inject] private IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;
        [Inject] protected IPublisher<ConnectStatus> m_ConnectStatusPublisher;
        public NetworkManager NetworkManager => m_NetworkManager;

        [SerializeField]
        int m_NbReconnectAttempts = 2;           // 최대 재연결 시도 횟수
        public int NbReconnectAttempts => m_NbReconnectAttempts;

        

        // 최대 동시 접속 플레이어 수

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
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 초기화 및 이벤트 핸들러 등록
        /// </summary>
        void Start()
        {
            if (NetworkManager == null)
            {
                return;
            }

            m_DebugClassFacade?.LogInfo(GetType().Name, "네트워크 매니저 초기화 시작");
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

            m_DebugClassFacade?.LogInfo(GetType().Name, "초기 상태 설정: Offline");
            m_CurrentState = m_Offline;

            m_DebugClassFacade?.LogInfo(GetType().Name, "네트워크 이벤트 핸들러 등록");
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.OnTransportFailure += OnTransportFailure;
            NetworkManager.OnServerStopped += OnServerStopped;
            
            // ConnectionState 상태 객체들에 종속성 주입
            foreach (var connectionState in states)
            {
                m_Resolver.Inject(connectionState);
            }
            
            m_DebugClassFacade?.LogInfo(GetType().Name, "종료: Start");
        }
        void OnDestroy()
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.OnServerStarted -= OnServerStarted;
            NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.OnTransportFailure -= OnTransportFailure;
            NetworkManager.OnServerStopped -= OnServerStopped;
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
            Debug.Log($"{name}: Changed connection state from {m_CurrentState.GetType().Name} to {nextState.GetType().Name}.");

            if (m_CurrentState != null)
            {
                m_CurrentState.Exit();
            }
            m_CurrentState = nextState;
            m_CurrentState.Enter();
        }

    // NGO 이벤트 핸들러들
    void OnClientDisconnectCallback(ulong clientId)
    {
        m_DebugClassFacade?.LogInfo(GetType().Name, $"[ConnectionManager] OnClientDisconnectCallback: ClientID={clientId}");
        m_CurrentState.OnClientDisconnect(clientId);
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        m_DebugClassFacade?.LogInfo(GetType().Name, $"[ConnectionManager]OnClientConnectedCallback ClientID={clientId}");
        m_CurrentState.OnClientConnected(clientId);
    }

    void OnServerStarted()
    {
        m_DebugClassFacade?.LogInfo(GetType().Name, "[ConnectionManager] 매치 서버 시작됨");
        m_CurrentState.OnServerStarted();
    }
      void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
      {
          m_CurrentState.ApprovalCheck(request, response);
      }
    void OnTransportFailure()
    {
        m_DebugClassFacade?.LogError(GetType().Name, "[ConnectionManager] 전송 실패 발생");
        m_CurrentState.OnTransportFailure();
    }

    void OnServerStopped(bool _)
    {
        m_DebugClassFacade?.LogInfo(GetType().Name, "[ConnectionManager] 매치 서버 중지됨");
        m_CurrentState.OnServerStopped();
    }

    /// <summary>
    /// 네트워크 상태를 비동기적으로 확인하는 메서드
    /// </summary>
    /// <returns>네트워크가 준비되었는지 여부를 나타내는 Task</returns>
    public async Task<bool> CheckNetworkStatusAsync()
    {
        try
        {
            // 1. 인터넷 연결 상태 확인
            bool isOnline = Application.internetReachability != NetworkReachability.NotReachable;
            if (!isOnline)
            {
                m_DebugClassFacade?.LogWarning(GetType().Name, "인터넷 연결이 없습니다.");
                return false;
            }

            // 2. 현재 연결 상태 확인
            if (m_CurrentState is OfflineState)
            {
                // 오프라인 상태인 경우, 로비 연결 상태로 전환
                m_DebugClassFacade?.LogInfo(GetType().Name, "오프라인 상태에서 로비 연결 상태로 전환");
                ChangeState(m_LobbyConnecting);
                await System.Threading.Tasks.Task.Delay(1000); // 상태 전환 대기
                return m_CurrentState is not OfflineState;
            }

            // 3. 현재 상태가 오프라인이 아닌 경우
            return true;
        }
        catch (System.Exception e)
        {
            m_DebugClassFacade?.LogError(GetType().Name, $"네트워크 상태 확인 중 오류 발생: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 로비를 통한 클라이언트 연결 시작
    /// </summary>
    public void StartClientLobby(string playerName)
    {
        m_DebugClassFacade?.LogInfo(GetType().Name, $"[ConnectionManager] 로비 클라이언트 시작: ");
        m_CurrentState.StartClientLobby(playerName);
    }



    public void StartHostLobby(string playerName)
    {
        m_CurrentState.StartHostLobby(playerName);
    }


    public void RequestShutdown()
        {
            m_CurrentState.OnUserRequestedShutdown();
        }


    }
}

