using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer;
using UnityEngine;
using Unity.Netcode;
// using Unity.Assets.Scripts.UnityServices.Lobbies;
// using Unity.Assets.Scripts.Gameplay.UI;
// using Unity.Assets.Scripts.Utils;
// using Unity.Assets.Scripts.ConnectionManagement;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 오프라인 상태를 나타내는 클래스
    /// 연결 상태 패턴의 일부로, 오프라인 상태에서의 동작을 정의
    /// 
    /// 이 상태에서는 네트워크 연결이 없는 상태를 관리하며,
    /// 클라이언트나 호스트 연결을 시작하기 위한 진입점 역할을 합니다.
    /// NetworkBehaviour 관련 기능은 비활성화된 상태입니다.
    /// </summary>
    class OfflineState : ConnectionState
    {
        [Inject]
        IObjectResolver m_Resolver;

        // [Inject]
        // LobbyServiceFacade m_LobbyServiceFacade;
        // [Inject]
        // ProfileManager m_ProfileManager;
        // [Inject]
        // LocalLobby m_LocalLobby;

        const string k_MainMenuSceneName = "MainMenu";
        
        // 씬 변경 이벤트 구독 여부를 추적
        private bool m_IsSceneEventSubscribed = false;
        
        // 온라인 연결 가능 여부 확인 주기 (초)
        private const float k_OnlineCheckInterval = 5.0f;
        
        // 마지막 온라인 연결 확인 시간
        private float m_LastOnlineCheckTime = 0f;
        
        // 온라인 연결 가능 여부
        private bool m_IsOnlineAvailable = false;
        
        // 온라인 연결 확인 코루틴
        private Coroutine m_OnlineCheckCoroutine;

        /// <summary>
        /// 상태 진입 시 호출되는 메서드
        /// 
        /// 오프라인 상태로 진입할 때 네트워크 상태를 확인하고,
        /// 필요한 경우 메인 메뉴 씬으로 전환합니다.
        /// 씬 변경 이벤트를 구독하여 MainMenu 씬 로드 시 LobbyConnectingState로 자동 전환합니다.
        /// </summary>
        public override void Enter()
        {
            Debug.Log("[OfflineState] 시작: Enter");
            
            // 네트워크 상태 확인
            if (m_ConnectionManager.NetworkManager.IsListening)
            {
                Debug.Log("[OfflineState] NetworkManager가 아직 활성 상태입니다. 이전 연결을 정리합니다.");
                m_ConnectionManager.NetworkManager.Shutdown();
            }
            else
            {
                Debug.Log("[OfflineState] NetworkManager가 이미 비활성 상태입니다.");
            }
            
            // 로비 서비스 추적 종료 (필요한 경우)
            // m_LobbyServiceFacade.EndTracking();
            

            

            
            Debug.Log("[OfflineState] 종료: Enter");
        }
        
        /// <summary>
        /// 온라인 연결 가능 여부를 주기적으로 확인하는 코루틴
        /// </summary>
        private System.Collections.IEnumerator CheckOnlineAvailability()
        {
            while (true)
            {
                // 온라인 연결 가능 여부 확인
                m_IsOnlineAvailable = CheckInternetConnection();
                Debug.Log($"[OfflineState] 온라인 연결 가능 여부: {m_IsOnlineAvailable}");
                
         
                
                // 다음 확인까지 대기
                yield return new WaitForSeconds(k_OnlineCheckInterval);
            }
        }
        
        /// <summary>
        /// 인터넷 연결 가능 여부를 확인하는 메서드
        /// </summary>
        private bool CheckInternetConnection()
        {
            // 실제 구현에서는 네트워크 연결 상태를 확인하는 로직 구현
            // 예: Unity의 NetworkReachability 사용 또는 핑 테스트 등
            
            // 임시 구현: 항상 연결 가능으로 가정 (실제 프로젝트에서는 적절히 구현 필요)
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        
        /// <summary>
        /// 온라인 연결 가능 여부와 메인 메뉴 씬 로드 여부를 확인하고 로비 연결 상태로 전환할지 결정하는 메서드
        /// </summary>
        private void CheckAndTransitionToLobby()
        {
            // 온라인 연결이 가능하고 메인 메뉴 씬에 있는 경우에만 로비 연결 상태로 전환
            if (m_IsOnlineAvailable && SceneManager.GetActiveScene().name == k_MainMenuSceneName)
            {
                Debug.Log("[OfflineState] 온라인 연결 가능 및 메인 메뉴 씬 확인됨, 로비 연결 상태로 전환 준비");
                
                // 로비 연결 상태로 전환 전 지연 추가
                MonoBehaviour monoBehaviour = m_ConnectionManager as MonoBehaviour;
                if (monoBehaviour != null)
                {
                    monoBehaviour.StartCoroutine(TransitionToLobbyAfterDelay());
                }
            }
            else
            {
                if (!m_IsOnlineAvailable)
                {
                    Debug.Log("[OfflineState] 온라인 연결 불가능, 로비 연결 상태로 전환하지 않음");
                }
                
                if (SceneManager.GetActiveScene().name != k_MainMenuSceneName)
                {
                    Debug.Log("[OfflineState] 메인 메뉴 씬이 아님, 로비 연결 상태로 전환하지 않음");
                }
            }
        }

        /// <summary>
        /// 씬 로드 완료 시 호출되는 이벤트 핸들러
        /// 
        /// MainMenu 씬이 로드되면 온라인 연결 가능 여부를 확인하고,
        /// 온라인 연결이 가능한 경우에만 LobbyConnectingState로 전환합니다.
        /// </summary>
        // private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        // {
        //     Debug.Log($"[OfflineState] 씬 로드됨: {scene.name}, 모드: {mode}");
            
        //     if (scene.name == k_MainMenuSceneName)
        //     {
        //         Debug.Log("[OfflineState] 메인 메뉴 씬 로드 완료, 온라인 연결 가능 여부 확인");
                
        //         // 온라인 연결 가능 여부 확인 및 로비 연결 상태로 전환 결정
        //         CheckAndTransitionToLobby();
        //     }
        // }
        
        /// <summary>
        /// 지연 후 로비 연결 상태로 전환하는 코루틴
        /// </summary>
        private System.Collections.IEnumerator TransitionToLobbyAfterDelay()
        {
            // 씬 로드 후 UI 초기화 등을 위한 짧은 지연
            yield return new WaitForSeconds(0.5f);
            
            // 전환 직전에 다시 한번 온라인 연결 가능 여부 확인
            if (!m_IsOnlineAvailable)
            {
                Debug.Log("[OfflineState] 전환 직전 온라인 연결 불가능 확인, 로비 연결 상태로 전환하지 않음");
                yield break;
            }
            
            Debug.Log("[OfflineState] 로비 연결 상태로 자동 전환");
            
            // 기본 플레이어 이름 설정 (실제로는 UI에서 입력받거나 저장된 값 사용)
            string defaultPlayerName = "Player_" + UnityEngine.Random.Range(1000, 9999);
            
            // LobbyConnectingState로 전환
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_LobbyConnecting.Configure(defaultPlayerName));
        }

        /// <summary>
        /// 상태 종료 시 호출되는 메서드
        /// 
        /// 오프라인 상태에서 다른 상태로 전환될 때 필요한 정리 작업을 수행합니다.
        /// 씬 변경 이벤트 구독을 해제합니다.
        /// </summary>
        public override void Exit()
        {
            Debug.Log("[OfflineState] Exit 호출");
            
            // 씬 변경 이벤트 구독 해제
            if (m_IsSceneEventSubscribed)
            {
                // SceneManager.sceneLoaded -= OnSceneLoaded;
                m_IsSceneEventSubscribed = false;
                Debug.Log("[OfflineState] 씬 로드 이벤트 구독 해제");
            }
            
            // 온라인 연결 확인 코루틴 중지
            if (m_OnlineCheckCoroutine != null)
            {
                MonoBehaviour monoBehaviour = m_ConnectionManager as MonoBehaviour;
                if (monoBehaviour != null)
                {
                    monoBehaviour.StopCoroutine(m_OnlineCheckCoroutine);
                    m_OnlineCheckCoroutine = null;
                    Debug.Log("[OfflineState] 온라인 연결 가능 여부 확인 중지");
                }
            }
        }

        /// <summary>
        /// IP 주소를 통한 클라이언트 연결 시작 메서드
        /// 
        /// 오프라인 상태에서 IP 주소를 통해 서버에 직접 연결을 시도합니다.
        /// 이 메서드는 ClientConnectingState로 전환하여 연결 프로세스를 시작해야 합니다.
        /// </summary>
        public override void StartClientIP(string playerName, string ipaddress, int port)
        {
            Debug.Log($"[OfflineState] StartClientIP 호출: PlayerName={playerName}, IP={ipaddress}, Port={port}");
            
            // 온라인 연결 가능 여부 확인
            if (!CheckInternetConnection())
            {
                Debug.LogError("[OfflineState] 온라인 연결 불가능, 클라이언트 연결을 시작할 수 없습니다.");
                return;
            }
            
            // 여기서 ClientConnectingState로 전환하는 코드 구현 필요
            // 예: m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnecting.Configure(playerName, ipaddress, port));
            base.StartClientIP(playerName, ipaddress, port);
        }

        /// <summary>
        /// 로비를 통한 클라이언트 연결 시작 메서드
        /// 
        /// 오프라인 상태에서 로비 서비스를 통해 서버에 연결을 시도합니다.
        /// LobbyConnectingState로 전환하여 로비 연결 프로세스를 시작합니다.
        /// </summary>
        public override void StartClientLobby(string playerName)
        {
            Debug.Log($"[OfflineState] 로비 연결 시작 - 플레이어: {playerName}");
            
            // 온라인 연결 가능 여부 확인
            if (!CheckInternetConnection())
            {
                Debug.LogError("[OfflineState] 온라인 연결 불가능, 로비 연결을 시작할 수 없습니다.");
                return;
            }
            
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_LobbyConnecting.Configure(playerName));
        }

        /// <summary>
        /// IP 주소를 통한 호스트 시작 메서드
        /// 
        /// 오프라인 상태에서 IP 주소를 통해 호스트를 시작합니다.
        /// 이 메서드는 StartingHostState로 전환하여 호스트 시작 프로세스를 시작해야 합니다.
        /// </summary>
        public override void StartHostIP(string playerName, string ipaddress, int port)
        {
            Debug.Log($"[OfflineState] StartHostIP 호출: PlayerName={playerName}, IP={ipaddress}, Port={port}");
            
            // 온라인 연결 가능 여부 확인
            if (!CheckInternetConnection())
            {
                Debug.LogError("[OfflineState] 온라인 연결 불가능, 호스트를 시작할 수 없습니다.");
                return;
            }
            
            // 여기서 StartingHostState로 전환하는 코드 구현 필요
            // 예: m_ConnectionManager.ChangeState(m_ConnectionManager.m_StartingHost.Configure(playerName, ipaddress, port));
            base.StartHostIP(playerName, ipaddress, port);
        }

        /// <summary>
        /// 로비를 통한 호스트 시작 메서드
        /// 
        /// 오프라인 상태에서 로비 서비스를 통해 호스트를 시작합니다.
        /// LobbyConnectingState로 전환하여 로비 연결 프로세스를 시작합니다.
        /// </summary>
        public override void StartHostLobby(string playerName)
        {
            Debug.Log($"[OfflineState] 호스트 로비 연결 시작 - 플레이어: {playerName}");
            
            // 온라인 연결 가능 여부 확인
            if (!CheckInternetConnection())
            {
                Debug.LogError("[OfflineState] 온라인 연결 불가능, 로비 호스트를 시작할 수 없습니다.");
                return;
            }
            
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_LobbyConnecting.Configure(playerName));
        }
    }
}

