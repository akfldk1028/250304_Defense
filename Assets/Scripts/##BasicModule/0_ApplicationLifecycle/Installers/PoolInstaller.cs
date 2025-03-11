using VContainer;
// using Unity.Assets.Scripts.UnityServices.Lobbies;
// using Unity.Assets.Scripts.Utils;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers
{
    public class PoolInstaller : IModuleInstaller
    {
        public ModuleType ModuleType => ModuleType.Pool;
        public void Install(IContainerBuilder builder)
        {
            UnityEngine.Debug.Log("[PoolInstaller] 로비 모듈 설치 시작");


            UnityEngine.Debug.Log("[PoolInstaller] 로비 모듈 설치 완료");
        }
    }
} 