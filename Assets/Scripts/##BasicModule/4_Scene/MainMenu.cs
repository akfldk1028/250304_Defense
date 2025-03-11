using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Assets.Scripts.Scene;
using VContainer;
using Unity.Assets.Scripts.Resource;
using Object = UnityEngine.Object;

namespace Unity.Assets.Scripts.Scene
{
public class MainMenuScene : BaseScene
{


	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		SceneType = EScene.MainMenu;


		return true;
	}


	public override void Clear()
	{
	}
}

}
