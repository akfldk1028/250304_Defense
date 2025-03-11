using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Unity.Assets.Scripts.Scene
{
    public enum EScene
	{
		Unknown,
		TitleScene,
		GameScene,
        MainMenu,
        StartUp
	}

    public class SceneManagerEx
    {
        public BaseScene CurrentScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }

        public void LoadScene(EScene type)
        {
            //Managers.Clear();
            SceneManager.LoadScene(GetSceneName(type));
        }

        private string GetSceneName(EScene type)
        {
            string name = System.Enum.GetName(typeof(EScene), type);
            return name;
        }

        public void Clear()
        {
            //CurrentScene.Clear();
        }
    }
}