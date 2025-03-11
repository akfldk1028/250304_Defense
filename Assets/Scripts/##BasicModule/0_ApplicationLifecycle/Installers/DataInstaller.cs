using VContainer;
using VContainer.Unity;
// using Unity.Assets.Scripts.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using Unity.Assets.Scripts.Data;
using Mono.Cecil;
using Unity.Assets.Scripts.Objects;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers
{



    public class DataInstaller : IModuleInstaller
    {
        public ModuleType ModuleType => ModuleType.GameData;


        public void Install(IContainerBuilder builder)
        {
            builder.Register<DataLoader>(Lifetime.Singleton).AsImplementedInterfaces();
            
            // SOManager 등록 (VContainer를 통해 생성)
            builder.Register<SOManager>(Lifetime.Singleton);
            
            // SOManager 인스턴스 설정을 위한 초기화 액션 등록
            builder.RegisterBuildCallback(container => 
            {
                // 생성된 인스턴스를 싱글톤 인스턴스로 설정
                var soManager = container.Resolve<SOManager>();
                SOManager.Instance = soManager;
                
                // SOManager 초기화 호출
                soManager.Initialize();
                Debug.Log("[DataInstaller] SOManager 초기화 완료");
            });
            
            builder.Register<RemoteDataRepository>(Lifetime.Singleton).As<IDataRepository>();
            builder.Register<UserDataManager>(Lifetime.Singleton);
            builder.Register<CurrencyManager>(Lifetime.Singleton); 
            builder.Register<GameDataManager>(Lifetime.Singleton);
            
            // 빌드 콜백은 필요 없음 - MonsterDataSOManager가 Awake에서 자동으로 JSON 데이터 적용
        }
    

    }
}
