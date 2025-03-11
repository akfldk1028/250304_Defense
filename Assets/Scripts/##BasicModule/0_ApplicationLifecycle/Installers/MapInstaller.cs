using VContainer;
// using Unity.Assets.Scripts.UnityServices.Lobbies;
// using Unity.Assets.Scripts.Utils;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers
{
    public class MapInstaller : IModuleInstaller
    {
        public ModuleType ModuleType => ModuleType.Map;
        public void Install(IContainerBuilder builder)
        {
            UnityEngine.Debug.Log("[MapInstaller] 로비 모듈 설치 시작");


            UnityEngine.Debug.Log("[MapInstaller] 로비 모듈 설치 완료");
        }
    }
} 