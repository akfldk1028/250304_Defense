using VContainer;
using VContainer.Unity;
using Unity.Assets.Scripts.Scene;
using Unity.Assets.Scripts.UI;
using UnityEngine;

public class MainMenuLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
       Debug.Log("[MainMenuLifetimeScope] Configure 시작");
       
       // 부모 스코프의 설정을 상속
       base.Configure(builder);
       
       // MainMenu 씬에서만 사용할 컴포넌트 등록
       Debug.Log("[MainMenuLifetimeScope] MainMenuScene 등록 시도");
       builder.RegisterComponentInHierarchy<MainMenuScene>();
       
       Debug.Log("[MainMenuLifetimeScope] UI_MainMenu_Matching 등록 시도");
       builder.RegisterComponentInHierarchy<UI_MainMenu>();
       
       Debug.Log("[MainMenuLifetimeScope] Configure 완료");
    }
}
