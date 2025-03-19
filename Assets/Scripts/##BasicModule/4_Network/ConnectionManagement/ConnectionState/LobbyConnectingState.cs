using System;
using UnityEngine;
using VContainer;
// using Unity.Assets.Scripts.UnityServices.Lobbies;
// using Unity.Assets.Scripts.Utils;
using UnityEngine.SceneManagement;
// using Unity.Assets.Scripts.Gameplay.UI;
using Unity.Netcode;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 로비 연결 상태를 나타내는 클래스
    /// 
    /// 이 상태는 멀티플레이 연결 전 로비에서 대기하는 상태를 관리합니다.
    /// 로비 서비스를 통해 다른 플레이어와 연결하기 전의 준비 단계입니다.
    /// NetworkBehaviour 기능은 아직 활성화되지 않았지만, 
    /// 로비 연결 후 네트워크 기능을 활성화하기 위한 준비를 수행합니다.
    /// </summary>
    internal class LobbyConnectingState : ConnectionState
    {
        // [Inject]
        // LobbyServiceFacade m_LobbyServiceFacade;
        
        // [Inject]
        // ProfileManager m_ProfileManager;
        
        // [Inject]
        // LocalLobby m_LocalLobby;

        private string m_PlayerName;
        private Action m_LoadingCompleteCallback;
        const string k_MainMenuSceneName = "MainMenu";

        public LobbyConnectingState Configure(string playerName)
        {
            m_PlayerName = playerName;
            return this;
        }

        public override void Enter()
        {
            Debug.Log("[LobbyConnectingState] 로비화면 및 상태 연결 준비 중...");
            
            // 로딩 완료 콜백 설정
            m_LoadingCompleteCallback = OnLoadingComplete;
        }

        private void OnLoadingComplete()
        {
            Debug.Log("[LobbyConnectingState] 로딩 완료, 리소스 정리 및 메인 메뉴 전환 시작");
        }



        private void LoadMainMenu()
        {
            if (SceneManager.GetActiveScene().name != k_MainMenuSceneName)
            {
                Debug.Log("[LobbyConnectingState] 메인 메뉴로 전환");
                SceneManager.LoadSceneAsync(k_MainMenuSceneName, LoadSceneMode.Single);
            }
        }

        public override void Exit()
        {
            Debug.Log("[LobbyConnectingState] 로비 연결 상태 종료");
        }

        public override void StartRelayConnection()
        {
            Debug.Log("[LobbyConnectingState] 릴레이 연결 시작");
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
    }
}