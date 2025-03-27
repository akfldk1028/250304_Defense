// using Unity.Assets.Scripts.UnityServices.Auth;
// using Unity.Assets.Scripts.Gameplay.Configuration;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Unity.Assets.Scripts.Auth;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers
{
    public class AuthenticationInstaller : IModuleInstaller
    {
        [Inject] private DebugClassFacade _debugClassFacade;
        public ModuleType ModuleType => ModuleType.Authentication;
        public void Install(IContainerBuilder builder)
        {
            _debugClassFacade?.LogInfo(GetType().Name, "인증 모듈 설치 시작");

            // AuthManager 등록
            builder.Register<AuthManager>(Lifetime.Singleton);
            
            // AuthInitializer 등록 (MonoBehaviour)
            // builder.RegisterComponentInHierarchy<AuthInitializer>();
            
            // builder.Register<AuthenticationServiceWrapper>(Lifetime.Singleton);
            // builder.Register<NameGenerationData>(Lifetime.Singleton);
         
// #if UNITY_EDITOR
//             var nameGenerationData = AssetDatabase.LoadAssetAtPath<NameGenerationData>("Assets/GameData/UI/NameGenerationData.asset");
// #else
//             var nameGenerationData = Resources.Load<NameGenerationData>("GameData/UI/NameGenerationData");
// #endif
//             if (nameGenerationData == null)
//             {
//                 Debug.LogError("[AuthInstaller] NameGenerationData 에셋을 찾을 수 없습니다. 기본 인스턴스를 생성합니다.");
//                 nameGenerationData = ScriptableObject.CreateInstance<NameGenerationData>();
//                 nameGenerationData.FirstWordList = new string[] { "Happy", "Brave", "Swift" };
//                 nameGenerationData.SecondWordList = new string[] { "Player", "Warrior", "Runsner" };
//             }
//             builder.Register<NameGenerationData>(resolver => nameGenerationData, Lifetime.Singleton);

            _debugClassFacade?.LogInfo(GetType().Name, "인증 모듈 설치 완료");
        }
    }
} 