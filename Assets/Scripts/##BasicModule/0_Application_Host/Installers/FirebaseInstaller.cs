// FirebaseInstaller.cs 또는 적절한 위치의 Installer 클래스
using VContainer;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers
{
    public class FirebaseInstaller : IModuleInstaller
    {
        public ModuleType ModuleType => ModuleType.ThirdParty; // 적절한 ModuleType 사용

        public void Install(IContainerBuilder builder)
        {
            // FirebaseManager 싱글톤 인스턴스 등록
            builder.Register<FirebaseManager>(Lifetime.Singleton);

        }


    }
}