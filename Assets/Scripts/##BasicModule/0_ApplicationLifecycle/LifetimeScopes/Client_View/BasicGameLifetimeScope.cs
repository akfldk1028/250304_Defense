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

       builder.Register<ObjectManager>(Lifetime.Singleton);
    //    builder.Register<ServerMonster>(Lifetime.Transient);

       // 기존 스포너 찾기
       GameObject existingMapSpawner = GameObject.Find("MapSpawner");
       GameObject existingMonsterSpawner = GameObject.Find("MonsterSpawner");
    //    GameObject existingBasicGameManager = GameObject.Find("BasicGameManager");

    //    if (existingBasicGameManager != null)
    //    {
    //        builder.RegisterInstance(existingBasicGameManager.GetComponent<BasicGameManager>());
    //        Debug.Log("[BasicGameLifetimeScope] 기존 BasicGameManager 재사용");
    //    }
    //    else
    //    {
    //     builder.RegisterComponentOnNewGameObject<BasicGameManager>(
    //         Lifetime.Singleton,
    //         "BasicGameManager"
    //     );
    //     Debug.Log("[BasicGameLifetimeScope] 새로운 BasicGameManager 생성");
    //    }
       

       // MapSpawnerFacade 등록
       if (existingMapSpawner != null)
       {
           var mapSpawnerFacade = existingMapSpawner.GetComponent<MapSpawnerFacade>();
           if (mapSpawnerFacade != null)
           {
               builder.RegisterInstance(mapSpawnerFacade);
               Debug.Log("[BasicGameLifetimeScope] 기존 MapSpawnerFacade 재사용");
           }
       }
       else
       {
               builder.RegisterComponentOnNewGameObject<MapSpawnerFacade>(
               Lifetime.Singleton, 
               "MapSpawner");
           Debug.Log("[BasicGameLifetimeScope] 새로운 MapSpawnerFacade 생성");
       }

       // ObjectManagerFacade 등록
       if (existingMonsterSpawner != null)
       {
           var objectManagerFacade = existingMonsterSpawner.GetComponent<ObjectManagerFacade>();
           if (objectManagerFacade != null)
           {
               builder.RegisterInstance(objectManagerFacade);
               Debug.Log("[BasicGameLifetimeScope] 기존 ObjectManagerFacade 재사용");
           }
       }
       else
       {
              builder.RegisterComponentOnNewGameObject<ObjectManagerFacade>(
               Lifetime.Singleton, 
               "MonsterSpawner");
           Debug.Log("[BasicGameLifetimeScope] 새로운 ObjectManagerFacade 생성");
       }

       // 오브젝트 생성 후 콜백을 등록하여 추가 처리
        builder.RegisterBuildCallback(container => {
            try {
                // NetUtils 가져오기
                var netUtils = container.Resolve<NetUtils>();
                
                // MapSpawnerFacade 초기화
                var mapSpawnerFacade = container.Resolve<MapSpawnerFacade>();
                GameObject mapSpawnerObject = mapSpawnerFacade.gameObject;
                DontDestroyOnLoad(mapSpawnerObject);
                
                // NetUtils를 사용하여 NetworkObject 초기화
                netUtils.InitializeNetworkObject(mapSpawnerObject);
                
                // ObjectManagerFacade 초기화
                var objectManagerFacade = container.Resolve<ObjectManagerFacade>();
                GameObject objectManagerObject = objectManagerFacade.gameObject;
                DontDestroyOnLoad(objectManagerObject);
                
                // NetUtils를 사용하여 NetworkObject 초기화
                netUtils.InitializeNetworkObject(objectManagerObject);
                
                Debug.Log("[BasicGameLifetimeScope] MapSpawnerFacade와 ObjectManagerFacade 초기화 완료");
            }
            catch (Exception e) {
                Debug.LogError($"오브젝트 설정 중 오류 발생: {e.Message}\n{e.StackTrace}");
            }
        });
        builder.Register<MapManager>(Lifetime.Singleton);
       
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

       _debugClassFacade?.LogInfo(GetType().Name, "BasicGameState 등록 시도");
       builder.RegisterComponentInHierarchy<BasicGameState>();

       _debugClassFacade?.LogInfo(GetType().Name, "BasicGameLifetimeScope Configure 완료");
    }


    
}
