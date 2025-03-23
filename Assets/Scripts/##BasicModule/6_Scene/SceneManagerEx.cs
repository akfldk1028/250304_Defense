// using System.Collections;
// using System.Collections.Generic;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using VContainer;
// using Unity.Assets.Scripts.Resource;
// using System;
// using Unity.Assets.Scripts.Network;

// namespace Unity.Assets.Scripts.Scene
// {
//     public enum EScene
// 	{
// 		Unknown,
// 		TitleScene,
// 		GameScene,
//         MainMenu,
//         StartUp,
//         BasicGame
// 	}


    
//     public class SceneManagerEx
//     {
//         [Inject] private ResourceManager _resourceManager;
//         [Inject] private NetworkManager _networkManager;

//         [Inject] private ConnectionManager _connectionManager; // ConnectionManager 주입


//         public BaseScene CurrentScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }

//         public void LoadScene(EScene type)
//         {
//             // 씬 전환 전에 현재 씬과 리소스 정리
//             Debug.Log($"[SceneManagerEx] 씬 전환: {CurrentScene?.SceneType} -> {type}");
            
//             // 씬 로드
//             SceneManager.LoadScene(GetSceneName(type));
//         }

//         //네트워크용

//         public virtual void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
//         {
//             Debug.Log($"[SceneManagerEx] LoadScene 호출: {sceneName}, 네트워크={useNetworkSceneManager}");
            
//             if (useNetworkSceneManager)
//             {
//                 if (_networkManager == null)
//                 {
//                     Debug.LogError("[SceneManagerEx] NetworkManager가 null입니다.");
//                     return;
//                 }

//                 if (_networkManager.IsServer)
//                 {
//                     Debug.Log($"[SceneManagerEx] 서버: 씬 전환 시작: {sceneName}");
//                     if (_networkManager.SceneManager != null)
//                     {
//                         _networkManager.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
//                     }
//                     else
//                     {
//                         Debug.LogError("[SceneManagerEx] NetworkManager.SceneManager가 null입니다.");
//                     }
//                 }
//                 else
//                 {
//                     Debug.Log($"[SceneManagerEx] 클라이언트: 서버의 씬 전환 명령 대기 중");
//                     if (_connectionManager != null)
//                     {
//                         _connectionManager.LoadSceneClientRpc(sceneName);
//                     }
//                     else
//                     {
//                         Debug.LogError("[SceneManagerEx] ConnectionManager가 null입니다.");
//                     }
//                 }
//             }
//             else
//             {
//                 // 일반 씬 로드
//                 SceneManager.LoadScene(sceneName);
//             }
//         }

//         public void LoadSceneForAllPlayers(EScene type)
//         {
//             // 씬 전환 전에 현재 씬과 리소스 정리
//             Debug.Log($"[SceneManagerEx] 네트워크 씬 전환: {CurrentScene?.SceneType} -> {type}");
//             // Clear();
            
//             string sceneName = type.ToString();
//             _networkManager.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
//         }
//         private string GetSceneName(EScene type)
//         {
//             string name = System.Enum.GetName(typeof(EScene), type);
//             return name;
//         }

     
//         public void Clear()
//         {
//             // 현재 씬의 Clear 메서드 호출
//             CurrentScene?.Clear();
            
//             // // ResourceManager의 Clear 메서드 호출하여 모든 리소스 초기화
//             // if (_resourceManager != null)
//             // {
//             //     Debug.Log("[SceneManagerEx] ResourceManager.Clear() 호출 - 모든 리소스 초기화");
//             //     _resourceManager.Clear();
//             // }
//             // else
//             // {
//             //     Debug.LogWarning("[SceneManagerEx] ResourceManager가 null입니다. 리소스를 초기화할 수 없습니다.");
//             // }
//         }
//     }
// }
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Unity.Assets.Scripts.Resource;
using System;
using Unity.Assets.Scripts.Network;

namespace Unity.Assets.Scripts.Scene
{
    public enum EScene
	{
		Unknown,
		TitleScene,
		GameScene,
        MainMenu,
        StartUp,
        BasicGame
	}


    
    public class SceneManagerEx
    {
        [Inject] private ResourceManager _resourceManager;
        [Inject] private NetworkManager _networkManager;
        [Inject] private ConnectionManager _connectionManager;
        [Inject] DebugClassFacade _debugClassFacade;

        // 씬 전환 요청 추적을 위한 변수
        private bool _isSceneTransitionInProgress = false;
        private float _lastSceneTransitionTime = 0f;
        private string _pendingSceneName = null;

        public BaseScene CurrentScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }

        public void LoadScene(EScene type)
        {
            // 씬 전환 전에 현재 씬과 리소스 정리
            Debug.Log($"[SceneManagerEx] 씬 전환: {CurrentScene?.SceneType} -> {type}");
            
            // 씬 로드
            SceneManager.LoadScene(GetSceneName(type));
        }

        // 네트워크용 씬 로드 메서드 - 개선됨
        public virtual void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            _debugClassFacade?.LogInfo(GetType().Name, $"LoadScene 호출: {sceneName}, 네트워크={useNetworkSceneManager}");
            
            // 씬 전환 요청 중복 방지 (3초 안에 같은 씬 전환 요청 무시)
            if (_isSceneTransitionInProgress || (Time.time - _lastSceneTransitionTime < 3f && _pendingSceneName == sceneName))
            {
                _debugClassFacade?.LogWarning(GetType().Name, $"씬 전환 요청 무시 - 이미 진행 중이거나 최근에 같은 요청이 있었음: {sceneName}");
                return;
            }

            _isSceneTransitionInProgress = true;
            _lastSceneTransitionTime = Time.time;
            _pendingSceneName = sceneName;

            if (useNetworkSceneManager)
            {
                if (_networkManager == null)
                {
                    _debugClassFacade?.LogError(GetType().Name, "NetworkManager가 null입니다.");
                    _isSceneTransitionInProgress = false;
                    return;
                }

                // 네트워크 상태 확인 및 출력
                bool isServer = _networkManager.IsServer;
                bool isClient = _networkManager.IsClient;
                bool isConnected = _networkManager.IsConnectedClient;
                _debugClassFacade?.LogInfo(GetType().Name, $"네트워크 상태: Server={isServer}, Client={isClient}, Connected={isConnected}");

                if (isServer)
                {
                    _debugClassFacade?.LogInfo(GetType().Name, $"서버: 씬 전환 시작: {sceneName}");
                    
                    // 서버에서 직접 씬 전환 (모든 클라이언트에게 자동 전파)
                    if (_networkManager.SceneManager != null)
                    {
                        try
                        {
                            _networkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                            _debugClassFacade?.LogInfo(GetType().Name, $"서버: {sceneName} 씬 로드 요청 성공");

                            // 추가: RPC를 통해 클라이언트에게 직접 알림 (중복 보장)
                            if (_connectionManager != null)
                            {
                                _connectionManager.LoadSceneClientRpc(sceneName);
                                _debugClassFacade?.LogInfo(GetType().Name, "서버: 추가적으로 RPC를 통해 클라이언트에게 씬 전환 명령 전송");
                            }
                        }
                        catch (Exception e)
                        {
                            _debugClassFacade?.LogError(GetType().Name, $"서버: 씬 로드 중 오류 발생: {e.Message}");
                        }
                    }
                    else
                    {
                        _debugClassFacade?.LogError(GetType().Name, "NetworkManager.SceneManager가 null입니다.");
                    }
                }
                else if (isClient && isConnected)
                {
                    _debugClassFacade?.LogInfo(GetType().Name, $"클라이언트: 씬 전환 요청 (서버에게 전달): {sceneName}");
                    
                    // 클라이언트에서는 로컬과 RPC 모두 시도
                    if (_connectionManager != null)
                    {
                        _debugClassFacade?.LogInfo(GetType().Name, "클라이언트: ConnectionManager를 통해 씬 전환 요청");

                        // 직접 로드 시도 (호스트에 의해 이미 씬 변경 이벤트가 오고 있을 수 있음)
                        try
                        {
                            _debugClassFacade?.LogInfo(GetType().Name, $"클라이언트: 로컬에서 {sceneName} 씬 직접 로드 시도");
                            SceneManager.LoadScene(sceneName);
                        }
                        catch (Exception e)
                        {
                            _debugClassFacade?.LogWarning(GetType().Name, $"클라이언트: 로컬 씬 로드 시도 중 오류 (무시 가능): {e.Message}");
                        }
                    }
                    else
                    {
                        _debugClassFacade?.LogError(GetType().Name, "ConnectionManager가 null입니다.");
                    }
                }
                else
                {
                    _debugClassFacade?.LogWarning(GetType().Name, $"네트워크 연결 상태가 유효하지 않음: Server={isServer}, Client={isClient}, Connected={isConnected}");
                    
                    // 네트워크 상태가 유효하지 않으면 로컬 씬 전환 시도
                    try
                    {
                        _debugClassFacade?.LogInfo(GetType().Name, $"네트워크 없이 로컬에서 {sceneName} 씬 로드 시도");
                        SceneManager.LoadScene(sceneName);
                    }
                    catch (Exception e)
                    {
                        _debugClassFacade?.LogError(GetType().Name, $"로컬 씬 로드 중 오류: {e.Message}");
                    }
                }
            }
            else
            {
                // 일반 씬 로드 (비 네트워크)
                _debugClassFacade?.LogInfo(GetType().Name, $"일반 씬 로드: {sceneName}");
                try
                {
                    SceneManager.LoadScene(sceneName);
                }
                catch (Exception e)
                {
                    _debugClassFacade?.LogError(GetType().Name, $"일반 씬 로드 중 오류: {e.Message}");
                }
            }

            // 씬 전환 요청 상태 리셋 (약간의 지연을 두고)
            ResetSceneTransitionState();
        }

        private void ResetSceneTransitionState()
        {
            // 씬 전환 상태 리셋 (코루틴이나 지연 처리로 구현해야 하지만, 여기서는 타이머 기반으로 작성)
            _isSceneTransitionInProgress = false;
        }

        public void LoadSceneForAllPlayers(EScene type)
        {
            // 씬 전환 전에 현재 씬과 리소스 정리
            _debugClassFacade?.LogInfo(GetType().Name, $"네트워크 씬 전환: {CurrentScene?.SceneType} -> {type}");
            
            string sceneName = type.ToString();
            
            if (_networkManager != null && _networkManager.SceneManager != null)
            {
                _networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                _debugClassFacade?.LogInfo(GetType().Name, $"NetworkSceneManager를 통해 {sceneName} 씬 로드 요청 성공");
                
                // 추가: RPC를 통해서도 알림 (중복 보장)
                if (_connectionManager != null && _networkManager.IsServer)
                {
                    _connectionManager.LoadSceneClientRpc(sceneName);
                    _debugClassFacade?.LogInfo(GetType().Name, "RPC를 통해 클라이언트에게 씬 전환 명령 전송");
                }
            }
            else
            {
                _debugClassFacade?.LogError(GetType().Name, "NetworkManager 또는 SceneManager가 null입니다.");
            }
        }

        private string GetSceneName(EScene type)
        {
            string name = System.Enum.GetName(typeof(EScene), type);
            return name;
        }

        public void Clear()
        {
            // 현재 씬의 Clear 메서드 호출
            CurrentScene?.Clear();
        }
    }
}