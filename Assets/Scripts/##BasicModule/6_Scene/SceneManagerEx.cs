using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Unity.Assets.Scripts.Resource;
using System;

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



        public BaseScene CurrentScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }

        public void LoadScene(EScene type)
        {
            // 씬 전환 전에 현재 씬과 리소스 정리
            Debug.Log($"[SceneManagerEx] 씬 전환: {CurrentScene?.SceneType} -> {type}");
            
            // 씬 로드
            SceneManager.LoadScene(GetSceneName(type));
        }

        //네트워크용

        public virtual void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (useNetworkSceneManager)
            {
                if (!_networkManager.ShutdownInProgress)
                {
                    if (_networkManager.IsServer)
                    {
                        // If is active server and NetworkManager uses scene management, load scene using NetworkManager's SceneManager
                        _networkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                    }
                }
            }
            else
            {
                _networkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                // Load using SceneManager
                var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                if (loadSceneMode == LoadSceneMode.Single)
                {
                }
            }
        }


        public void LoadSceneForAllPlayers(EScene type)
        {
            // 씬 전환 전에 현재 씬과 리소스 정리
            Debug.Log($"[SceneManagerEx] 네트워크 씬 전환: {CurrentScene?.SceneType} -> {type}");
            // Clear();
            
            string sceneName = type.ToString();
            _networkManager.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        private string GetSceneName(EScene type)
        {
            string name = System.Enum.GetName(typeof(EScene), type);
            return name;
        }

        public void ChangeSceneForAllPlayers(EScene type)
        {
            try
            {
                Debug.Log($"[SceneManagerEx] 네트워크를 통해 씬 변경 시도: {type}");
                
                // NetworkManager.Singleton null 체크
                if (_networkManager == null)
                {
                    Debug.LogError("[SceneManagerEx] NetworkManager.Singleton이 null입니다. 로컬 씬 로드로 대체합니다.");
                    LoadScene(type);
                    return;
                }

                // SceneManager null 체크
                if (_networkManager.SceneManager == null)
                {
                    Debug.LogError("[SceneManagerEx] NetworkManager.Singleton.SceneManager가 null입니다. 로컬 씬 로드로 대체합니다.");
                    LoadScene(type);
                    return;
                }

                // 호스트/서버인지 확인
                if (!_networkManager.IsServer && !_networkManager.IsHost)
                {
                    Debug.LogError("[SceneManagerEx] 서버나 호스트가 아닙니다. 클라이언트는 씬 로드를 요청할 수 없습니다.");
                    // 클라이언트가 호출하면 아무것도 하지 않습니다. 호스트의 요청을 기다려야 합니다.
                    return;
                }

                // 실제 씬 변경 실행
                string sceneName = type.ToString();
                Debug.Log($"[SceneManagerEx] 네트워크를 통해 씬 '{sceneName}' 로드 시작");
                _networkManager.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneManagerEx] 씬 변경 중 예외 발생: {ex.Message}\n{ex.StackTrace}");
                
                // 예외 발생 시 로컬 씬 로드로 폴백
                try
                {
                    Debug.Log($"[SceneManagerEx] 로컬 씬 로드로 대체: {type}");
                    LoadScene(type);
                }
                catch (Exception fallbackEx)
                {
                    Debug.LogError($"[SceneManagerEx] 로컬 씬 로드 중에도 예외 발생: {fallbackEx.Message}");
                }
            }
        }
        public void Clear()
        {
            // 현재 씬의 Clear 메서드 호출
            CurrentScene?.Clear();
            
            // // ResourceManager의 Clear 메서드 호출하여 모든 리소스 초기화
            // if (_resourceManager != null)
            // {
            //     Debug.Log("[SceneManagerEx] ResourceManager.Clear() 호출 - 모든 리소스 초기화");
            //     _resourceManager.Clear();
            // }
            // else
            // {
            //     Debug.LogWarning("[SceneManagerEx] ResourceManager가 null입니다. 리소스를 초기화할 수 없습니다.");
            // }
        }
    }
}