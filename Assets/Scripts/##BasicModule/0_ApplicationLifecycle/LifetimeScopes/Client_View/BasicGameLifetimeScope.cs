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

public class BasicGameLifetimeScope : LifetimeScope
{
    [Inject] private DebugClassFacade _debugClassFacade;
    private GameObject _mapSpawnerPrefab;
    private GameObject _objectSpawnerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        _debugClassFacade?.LogInfo(GetType().Name, "BasicGameLifetimeScope Configure 시작");
        base.Configure(builder);
      
        // 부모 스코프에서 ResourceManager 참조 (새 인스턴스 생성 방지)
        ResourceManager resourceManager = null;
        if (Parent != null && Parent.Container.Resolve<ResourceManager>() != null)
        {
            resourceManager = Parent.Container.Resolve<ResourceManager>();
            builder.RegisterInstance(resourceManager);
            Debug.Log("[BasicGameLifetimeScope] 부모 스코프에서 ResourceManager 참조 성공");
        }

        NetUtils netUtils = null;
        if (Parent != null && Parent.Container.Resolve<NetUtils>() != null)
        {
            netUtils = Parent.Container.Resolve<NetUtils>();
            builder.RegisterInstance(netUtils);
            Debug.Log("[BasicGameLifetimeScope] 부모 스코프에서 NetUtils 참조 성공");
        }
  
        // 기본 매니저 등록
        builder.Register<ObjectManager>(Lifetime.Singleton);
        builder.Register<MapManager>(Lifetime.Singleton);



        builder.RegisterComponentInHierarchy<BasicGameState>();



        // 프리팹을 로드하고 인스턴스화하여 등록 (ResourceManager가 있는 경우)
        MapSpawnerFacade mapSpawnerFacade = null;
        ObjectManagerFacade objectManagerFacade = null;
       
        if (resourceManager != null)
        {
            try
            {
                // 맵 스포너 프리팹 로드하고 인스턴스화
                _mapSpawnerPrefab = resourceManager.Load<GameObject>("MapSpawner");
                if (_mapSpawnerPrefab != null)
                {
                    GameObject mapSpawnerInstance = Instantiate(_mapSpawnerPrefab);
                    mapSpawnerInstance.name = "MapSpawner_Instance";
                    mapSpawnerFacade = mapSpawnerInstance.GetComponent<MapSpawnerFacade>();
                    
                    if (mapSpawnerFacade != null)
                    {
                        // 게임 오브젝트는 등록하지 않고 컴포넌트만 등록
                        builder.RegisterInstance(mapSpawnerFacade).AsSelf();
                        Debug.Log("[BasicGameLifetimeScope] MapSpawnerFacade 컴포넌트 등록 성공");
                        
                        // DontDestroyOnLoad 설정
                        DontDestroyOnLoad(mapSpawnerInstance);
                        
                        // NetworkObject 스폰 로직 수정 - 서버일 때만 스폰
                        NetworkObject networkObject = mapSpawnerInstance.GetComponent<NetworkObject>();
                        if (networkObject != null && !networkObject.IsSpawned)
                        {
                            // 서버일 때만 스폰
                            if (NetworkManager.Singleton.IsServer)
                            {
                                networkObject.Spawn();
                                Debug.Log($"[BasicGameLifetimeScope] MapSpawnerFacade NetworkObject 스폰 완료 - NetworkObjectId: {networkObject.NetworkObjectId}");
                            }
                            else
                            {
                                Debug.Log("[BasicGameLifetimeScope] 클라이언트에서는 NetworkObject를 스폰하지 않습니다 (MapSpawnerFacade)");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("[BasicGameLifetimeScope] MapSpawner 프리팹에 MapSpawnerFacade 컴포넌트가 없습니다");
                    }
                }
               
                // ObjectSpawner 프리팹 로드 및 인스턴스화
                _objectSpawnerPrefab = resourceManager.Load<GameObject>("ObjectSpawner");
                if (_objectSpawnerPrefab != null)
                {
                    GameObject objectSpawnerInstance = Instantiate(_objectSpawnerPrefab);
                    objectSpawnerInstance.name = "ObjectSpawner_Instance";
                    objectManagerFacade = objectSpawnerInstance.GetComponent<ObjectManagerFacade>();
                    
                    if (objectManagerFacade != null)
                    {
                        // 게임 오브젝트는 등록하지 않고 컴포넌트만 등록
                        builder.RegisterInstance(objectManagerFacade).AsSelf();
                        Debug.Log("[BasicGameLifetimeScope] ObjectManagerFacade 컴포넌트 등록 성공");
                        
                        // DontDestroyOnLoad 설정
                        DontDestroyOnLoad(objectSpawnerInstance);
                        
                        // MapSpawnerFacade 참조 직접 설정
                        if (mapSpawnerFacade != null)
                        {
                            objectManagerFacade._mapSpawnerFacade = mapSpawnerFacade;
                            Debug.Log("[BasicGameLifetimeScope] ObjectManagerFacade에 MapSpawnerFacade 직접 설정");
                        }
                        
                        // NetworkObject 스폰 로직 수정 - 서버일 때만 스폰
                        NetworkObject networkObject = objectSpawnerInstance.GetComponent<NetworkObject>();
                        if (networkObject != null && !networkObject.IsSpawned)
                        {
                            // 서버일 때만 스폰
                            if (NetworkManager.Singleton.IsServer)
                            {
                                networkObject.Spawn();
                                Debug.Log($"[BasicGameLifetimeScope] ObjectManagerFacade NetworkObject 스폰 완료 - NetworkObjectId: {networkObject.NetworkObjectId}");
                            }
                            else
                            {
                                Debug.Log("[BasicGameLifetimeScope] 클라이언트에서는 NetworkObject를 스폰하지 않습니다 (ObjectManagerFacade)");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("[BasicGameLifetimeScope] ObjectSpawner 프리팹에 ObjectManagerFacade 컴포넌트가 없습니다");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BasicGameLifetimeScope] 프리팹 인스턴스화 및 등록 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
            }
        }
       
        // 씬 관련 컴포넌트 등록
        _debugClassFacade?.LogInfo(GetType().Name, "BasicGameScene 등록 시도");
        builder.RegisterComponentInHierarchy<BasicGameScene>();
       
        _debugClassFacade?.LogInfo(GetType().Name, "UI_BasicGame 등록 시도");
        builder.RegisterComponentInHierarchy<UI_BasicGame>();

        // 컨테이너 빌드 후 초기화를 수행하는 콜백 등록
        builder.RegisterBuildCallback(container => {
            try {
                Debug.Log("[BasicGameLifetimeScope] 의존성 객체 초기화 시작");
                
                // 필요한 매니저 클래스들 참조 및 로깅
                var objectManager = container.Resolve<ObjectManager>();                
                var mapManager = container.Resolve<MapManager>();
                
                // MapSpawnerFacade 참조 및 초기화
                try {
                    var mapSpawnerFacadeRef = container.Resolve<MapSpawnerFacade>();
                    mapSpawnerFacadeRef.Initialize();
                }
                catch (Exception e) {
                    Debug.LogError($"[BasicGameLifetimeScope] MapSpawnerFacade 참조 또는 초기화 중 오류: {e.Message}");
                }
                
                // ObjectManagerFacade 참조, ObjectManager 설정 및 초기화
                try {
                    var objectManagerFacadeRef = container.Resolve<ObjectManagerFacade>();
                    
                    objectManagerFacadeRef._objectManager = objectManager;
                    objectManagerFacadeRef._netUtils = netUtils;
                    objectManagerFacadeRef.Initialize();
                }
                catch (Exception e) {
                    Debug.LogError($"[BasicGameLifetimeScope] ObjectManagerFacade 참조 또는 초기화 중 오류: {e.Message}");
                }
                
                // BasicGameState 참조 및 초기화
                try {
                    var basicGameState = container.Resolve<BasicGameState>();
                    basicGameState.Initialize();
                }
                catch (Exception e) {
                    Debug.LogError($"[BasicGameLifetimeScope] BasicGameState 참조 또는 초기화 중 오류: {e.Message}");
                }
                
                Debug.Log("[BasicGameLifetimeScope] 모든 컴포넌트 초기화 완료");
            } 
            catch (Exception ex) {
                Debug.LogError($"[BasicGameLifetimeScope] 초기화 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
            }
        });

        _debugClassFacade?.LogInfo(GetType().Name, "BasicGameLifetimeScope Configure 완료");
    }
}