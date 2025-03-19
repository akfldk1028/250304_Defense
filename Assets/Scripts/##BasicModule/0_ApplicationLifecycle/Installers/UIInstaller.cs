using VContainer;
// using Unity.Assets.Scripts.Gameplay.UI;
using VContainer.Unity;
using Unity.Assets.Scripts.UI;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers{
    public class UIInstaller : IModuleInstaller
    {
        [Inject] private DebugClassFacade _debugClassFacade;

        public ModuleType ModuleType => ModuleType.UI;

        public void Install(IContainerBuilder builder)
        {
            _debugClassFacade?.LogInfo(GetType().Name, "UI 모듈 설치 시작");

            // UIManager 등록
            builder.Register<UIManager>(Lifetime.Singleton);

            // UI_StartUpScene 등록
            builder.RegisterComponentInHierarchy<UI_StartUpScene>();
            
            // UI_MainMenu_Matching은 다음 씬에서 사용되므로 타입만 등록 (Singleton으로 변경)
            // builder.Register<UI_MainMenu_Matching>(Lifetime.Singleton);

            _debugClassFacade?.LogInfo(GetType().Name, "UI 모듈 설치 완료");
        }
    }
} 