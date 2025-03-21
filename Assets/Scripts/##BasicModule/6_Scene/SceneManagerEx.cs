using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Unity.Assets.Scripts.Resource;

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
        public void LoadSceneForAllPlayers(EScene type)
        {
            // 씬 전환 전에 현재 씬과 리소스 정리
            Debug.Log($"[SceneManagerEx] 네트워크 씬 전환: {CurrentScene?.SceneType} -> {type}");
            // Clear();
            
            string sceneName = type.ToString();
            SceneManager.LoadScene(GetSceneName(type));

            // 주입받아야함
            // if (NetworkManager.Singleton.IsHost)
            // {
            //     NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            // }
        }
        private string GetSceneName(EScene type)
        {
            string name = System.Enum.GetName(typeof(EScene), type);
            return name;
        }

        public void ChangeSceneForAllPlayers(EScene type)
        {
            if (_networkManager.IsHost)
            {
                _networkManager.SceneManager.LoadScene(type.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Single);
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