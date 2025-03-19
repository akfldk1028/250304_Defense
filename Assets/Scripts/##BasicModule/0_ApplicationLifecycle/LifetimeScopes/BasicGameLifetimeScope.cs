using VContainer;
using VContainer.Unity;
using Unity.Assets.Scripts.Scene;
using Unity.Assets.Scripts.UI;
using UnityEngine;
using Unity.Assets.Scripts.Resource;
using System;
using Unity.Assets.Scripts.Objects;
using Unity.Netcode;
using Unity.Assets.Scripts.Data;
// ServerMonsterFactory는 Unity.Assets.Scripts.Objects 네임스페이스에 있습니다.
// using Unity.Assets.Scripts.Factories;

public class BasicGameLifetimeScope : LifetimeScope
{
    [Inject] private DebugClassFacade _debugClassFacade;

    protected override void Configure(IContainerBuilder builder)
    {
       _debugClassFacade?.LogInfo(GetType().Name, "BasicGameLifetimeScope Configure 시작");
       base.Configure(builder);
       
       // DataLoader 명시적으로 등록 확인
       if (Parent != null && Parent.Container.Resolve<DataLoader>() != null)
       {
           var dataLoader = Parent.Container.Resolve<DataLoader>();
           builder.RegisterInstance(dataLoader);
           Debug.Log("[BasicGameLifetimeScope] 부모 스코프에서 DataLoader 참조 성공");
       }
       else
       {
           // 부모 스코프에서 찾을 수 없는 경우에만 새로 생성
           builder.Register<DataLoader>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
           Debug.Log("[BasicGameLifetimeScope] 새 DataLoader 인스턴스 생성");
       }
       
       // ObjectManager를 먼저 등록
       builder.Register<ObjectManager>(Lifetime.Singleton);

       // ServerMonster 컴포넌트 등록 (Prefab 인스턴스에 주입하기 위해)
       builder.Register<ServerMonster>(Lifetime.Transient);

       // ObjectManagerFacade를 등록 (싱글톤으로)
    //    builder.Register<ObjectManagerFacade>(Lifetime.Singleton);
        var spawnerRegistration =  builder.RegisterComponentOnNewGameObject<ObjectManagerFacade>(
           Lifetime.Singleton, 
           "MonsterSpawner");

            //    var spawnerRegistration = builder.RegisterComponentOnNewGameObject<MapSpawnerFacade>(
    //        Lifetime.Singleton, 
    //        "Spawner");
           
       // 오브젝트 생성 후 콜백을 등록하여 추가 처리
       builder.RegisterBuildCallback(container => {
           try {
               // MapSpawnerFacade 컴포넌트 찾기
               var mapSpawnerFacade = container.Resolve<ObjectManagerFacade>();
               
               // Spawner 게임 오브젝트 가져오기
               GameObject spawnerObject = mapSpawnerFacade.gameObject; 
               
               // NetworkObject 컴포넌트 확인 및 추가
               if (spawnerObject.GetComponent<NetworkObject>() == null)
               {
                   spawnerObject.AddComponent<NetworkObject>();
                   Debug.Log("Spawner에 NetworkObject 컴포넌트 추가 완료");
               }

           }
           catch (Exception e)
           {
               Debug.LogError($"Spawner 오브젝트 설정 중 오류 발생: {e.Message}\n{e.StackTrace}");
           }
       });

       builder.Register<MapManager>(Lifetime.Singleton);
       
       builder.Register<MapSpawnerFacade>(Lifetime.Singleton);
          // MapSpawnerFacade 등록 및 Spawner 오브젝트 생성
    //    var spawnerRegistration = builder.RegisterComponentOnNewGameObject<MapSpawnerFacade>(
    //        Lifetime.Singleton, 
    //        "Spawner");
           
       // 오브젝트 생성 후 콜백을 등록하여 추가 처리
    //    builder.RegisterBuildCallback(container => {
    //        try {
    //            // MapSpawnerFacade 컴포넌트 찾기
    //            var mapSpawnerFacade = container.Resolve<MapSpawnerFacade>();
               
    //            // Spawner 게임 오브젝트 가져오기
    //            GameObject spawnerObject = mapSpawnerFacade.gameObject;
               
    //            // NetworkObject 컴포넌트 확인 및 추가
    //            if (spawnerObject.GetComponent<NetworkObject>() == null)
    //            {
    //                spawnerObject.AddComponent<NetworkObject>();
    //                Debug.Log("Spawner에 NetworkObject 컴포넌트 추가 완료");
    //            }
               
    //            // ObjectManagerFacade 컴포넌트 추가 (이미 등록된 인스턴스 사용)
    //            var objectManagerFacade = container.Resolve<ObjectManagerFacade>();
    //            if (!spawnerObject.GetComponent<ObjectManagerFacade>())
    //            {
    //                // 기존 등록된 인스턴스를 컴포넌트로 추가하는 대신, 컴포넌트를 추가하고 참조 설정
    //                var objectManagerComponent = spawnerObject.AddComponent<ObjectManagerFacade>();
    //                // 의존성 수동 주입
    //                container.Inject(objectManagerComponent);
    //                Debug.Log("Spawner에 ObjectManagerFacade 컴포넌트 추가 및 의존성 주입 완료");
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            Debug.LogError($"Spawner 오브젝트 설정 중 오류 발생: {e.Message}\n{e.StackTrace}");
    //        }
    //    });


       // ResourceManager 명시적 등록 (싱글톤)
       Debug.Log("[BasicGameLifetimeScope] ResourceManager 참조 시도");
       
       // 부모 스코프에서 ResourceManager 참조 (새 인스턴스 생성 방지)
       if (Parent != null && Parent.Container.Resolve<ResourceManager>() != null)
       {
           var resourceManager = Parent.Container.Resolve<ResourceManager>();
           builder.RegisterInstance(resourceManager);
           Debug.Log("[BasicGameLifetimeScope] 부모 스코프에서 ResourceManager 참조 성공");
       }
       else
       {
           // 부모 스코프에서 찾을 수 없는 경우에만 새로 생성
           builder.Register<ResourceManager>(Lifetime.Singleton);
           Debug.Log("[BasicGameLifetimeScope] 새 ResourceManager 인스턴스 생성");
       }
       


       Debug.Log("[BasicGameLifetimeScope] ResourceManager 등록 완료");
              // MainMenu 씬에서만 사용할 컴포넌트 등록
       _debugClassFacade?.LogInfo(GetType().Name, "BasicGameScene  등록 시도");
       builder.RegisterComponentInHierarchy<BasicGameScene>();
       
       _debugClassFacade?.LogInfo(GetType().Name, "UI_BasicGame 등록 시도");
       builder.RegisterComponentInHierarchy<UI_BasicGame>();


       _debugClassFacade?.LogInfo(GetType().Name, "BasicGameLifetimeScope Configure 완료");
    }


    
}
