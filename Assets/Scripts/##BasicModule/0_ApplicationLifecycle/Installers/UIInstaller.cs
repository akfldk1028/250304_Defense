using VContainer;
// using Unity.Assets.Scripts.Gameplay.UI;
using VContainer.Unity;
using Unity.Assets.Scripts.UI;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers{
    public class UIInstaller : IModuleInstaller
    {
        ModuleType IModuleInstaller.ModuleType => ModuleType.UI;

        public void Install(IContainerBuilder builder)
        {
            UnityEngine.Debug.Log("[UIInstaller] UI 모듈 설치 시작");

            // UIManager 등록
            builder.Register<UIManager>(Lifetime.Singleton);

            // UI_StartUpScene 등록
            builder.RegisterComponentInHierarchy<UI_StartUpScene>();
            
            // UI_MainMenu_Matching은 다음 씬에서 사용되므로 타입만 등록 (Singleton으로 변경)
            // builder.Register<UI_MainMenu_Matching>(Lifetime.Singleton);
            UnityEngine.Debug.Log("[UIInstaller] UI_MainMenu_Matching 타입 등록 완료 (Singleton, 다음 씬에서 사용)");

            UnityEngine.Debug.Log("[UIInstaller] UI 모듈 설치 완료");
        }
    }
} 