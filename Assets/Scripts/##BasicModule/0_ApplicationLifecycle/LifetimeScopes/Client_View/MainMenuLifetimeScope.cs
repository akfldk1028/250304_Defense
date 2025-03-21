using VContainer;
using VContainer.Unity;
using Unity.Assets.Scripts.Scene;
using Unity.Assets.Scripts.UI;
using UnityEngine;

public class MainMenuLifetimeScope : LifetimeScope
{
    [Inject] private DebugClassFacade _debugClassFacade;

    protected override void Configure(IContainerBuilder builder)
    {
       _debugClassFacade?.LogInfo(GetType().Name, "MainMenuLifetimeScope Configure 시작");
       
       // 부모 스코프의 설정을 상속
       base.Configure(builder);
       
       // MainMenu 씬에서만 사용할 컴포넌트 등록
       _debugClassFacade?.LogInfo(GetType().Name, "MainMenuScene 등록 시도");
       builder.RegisterComponentInHierarchy<MainMenuScene>();
       
       _debugClassFacade?.LogInfo(GetType().Name, "UI_MainMenu_Matching 등록 시도");
       builder.RegisterComponentInHierarchy<UI_MainMenu>();
       
       _debugClassFacade?.LogInfo(GetType().Name, "Configure 완료");
    }
}
