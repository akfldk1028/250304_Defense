using VContainer;
using VContainer.Unity;
// using Unity.Assets.Scripts.UnityServices.Lobbies;
// using Unity.Assets.Scripts.Utils;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers
{
    public class LobbyInstaller : IModuleInstaller
    {
        [Inject] private DebugClassFacade _debugClassFacade;
        
        public ModuleType ModuleType => ModuleType.Lobby;
        public void Install(IContainerBuilder builder)
        {
            _debugClassFacade?.LogInfo(GetType().Name, "로비 모듈 설치 시작");
            // builder.RegisterEntryPoint<LobbyServiceFacade>(Lifetime.Singleton).AsSelf();

            // 로비 관련 서비스 등록
            // builder.Register<LocalLobbyUser>(Lifetime.Singleton);
            // builder.Register<LocalLobby>(Lifetime.Singleton);
            // builder.Register<LobbyServiceFacade>(Lifetime.Singleton).AsSelf();
            
            // // 프로필 매니저 등록
            // builder.Register<ProfileManager>(Lifetime.Singleton).AsSelf();

            _debugClassFacade?.LogInfo(GetType().Name, "로비 모듈 설치 완료");
        }
    }
} 