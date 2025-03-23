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

        [Inject] private ConnectionManager _connectionManager; // ConnectionManager 주입


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
            Debug.Log($"[SceneManagerEx] LoadScene 호출: {sceneName}, 네트워크={useNetworkSceneManager}");
            
            if (useNetworkSceneManager)
            {
                if (_networkManager == null)
                {
                    Debug.LogError("[SceneManagerEx] NetworkManager가 null입니다.");
                    return;
                }

                if (_networkManager.IsServer)
                {
                    Debug.Log($"[SceneManagerEx] 서버: 씬 전환 시작: {sceneName}");
                    if (_networkManager.SceneManager != null)
                    {
                        _networkManager.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                    }
                    else
                    {
                        Debug.LogError("[SceneManagerEx] NetworkManager.SceneManager가 null입니다.");
                    }
                }
                else
                {
                    Debug.Log($"[SceneManagerEx] 클라이언트: 서버의 씬 전환 명령 대기 중");
                    if (_connectionManager != null)
                    {
                        _connectionManager.LoadSceneClientRpc(sceneName);
                    }
                    else
                    {
                        Debug.LogError("[SceneManagerEx] ConnectionManager가 null입니다.");
                    }
                }
            }
            else
            {
                // 일반 씬 로드
                SceneManager.LoadScene(sceneName);
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