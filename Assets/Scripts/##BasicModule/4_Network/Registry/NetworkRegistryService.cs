using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using VContainer;
using VContainer.Unity;

// // NetworkRegistryService 인터페이스 정의
// public interface INetworkRegistryService
// {
//     void InitializeNetworkObject(GameObject gameObject, bool spawnImmediately = false);
//     void RegisterNetworkPrefabRuntime(GameObject gameObject);
//     void PreRegisterPrefab(string prefabName, GameObject prefab);
//     void RegisterSpawnedObject(GameObject gameObject, ulong networkId);
//     GameObject GetObjectByNetworkId(ulong networkId);
//     void UnregisterObject(ulong networkId);
    
//     event Action<GameObject, ulong> OnObjectSpawned;
// }

// NetworkRegistryService 구현
public class NetworkRegistryService : IStartable
{
    // NetworkManager 참조
    private NetworkManager _networkManager;
    
    // 이미 등록된 프리팹 추적
    private readonly Dictionary<string, GameObject> _registeredPrefabs = new Dictionary<string, GameObject>();
    
    // 네트워크 오브젝트 ID로 오브젝트 추적
    private readonly Dictionary<ulong, GameObject> _spawnedObjects = new Dictionary<ulong, GameObject>();
    
    // 스폰 이벤트 콜백
    public event Action<GameObject, ulong> OnObjectSpawned; // (게임오브젝트, 네트워크ID)
    
    [Inject]
    public NetworkRegistryService(NetworkManager networkManager = null)
    {
        _networkManager = networkManager ?? GameObject.FindObjectOfType<NetworkManager>();
        if (_networkManager == null)
        {
            Debug.LogError("[NetworkRegistryService] NetworkManager를 찾을 수 없습니다.");
        }
    }
    
    // IStartable 인터페이스 구현 - VContainer가 Start() 메서드처럼 호출함
    public void Start()
    {
        if (_networkManager != null)
        {
            // 스폰 감지를 위한 구독 로직
            // Unity Netcode에서는 OnObjectSpawned 이벤트가 없으므로, 대안 방법을 사용
            // 예: MonoBehavior를 만들어서 Update에서 감지하거나, 다른 메커니즘 사용
            Debug.Log("[NetworkRegistryService] 초기화 완료");
            
            // NetworkManager 이벤트 구독은 여기서 하면 됨
            // 예: _networkManager.OnServerStarted += ...
        }
    }
    
    // VContainer/DI를 통해 생성된 오브젝트 초기화 및 등록
    public void InitializeNetworkObject(GameObject gameObject, bool spawnImmediately = false)
    {
        if (gameObject == null)
        {
            Debug.LogError("[NetworkRegistryService] 초기화할 게임 오브젝트가 null입니다.");
            return;
        }

        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            networkObject = gameObject.AddComponent<NetworkObject>();
            Debug.Log($"[NetworkRegistryService] {gameObject.name}에 NetworkObject 컴포넌트를 추가했습니다.");
        }

        // NetworkManager에 등록
        RegisterNetworkPrefabRuntime(gameObject);
        
        // 즉시 스폰 옵션이 활성화된 경우 스폰
        if (spawnImmediately && _networkManager != null && _networkManager.IsServer && !networkObject.IsSpawned)
        {
            try
            {
                networkObject.Spawn();
                
                // 스폰 후 딕셔너리에 추가
                _spawnedObjects[networkObject.NetworkObjectId] = gameObject;
                
                // 스폰 이벤트 발생
                OnObjectSpawned?.Invoke(gameObject, networkObject.NetworkObjectId);
                
                Debug.Log($"[NetworkRegistryService] {gameObject.name}을 네트워크에 스폰했습니다. ID={networkObject.NetworkObjectId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkRegistryService] {gameObject.name} 스폰 중 오류 발생: {ex.Message}");
            }
        }
    }
    
    // 런타임에 NetworkManager에 프리팹 등록
    public void RegisterNetworkPrefabRuntime(GameObject gameObject)
    {
        if (_networkManager == null || gameObject == null) return;

        string prefabName = gameObject.name;
        
        // 이미 등록된 프리팹인지 확인
        if (_registeredPrefabs.ContainsKey(prefabName))
        {
            Debug.Log($"[NetworkRegistryService] {prefabName}은 이미 NetworkManager에 등록되어 있습니다.");
            return;
        }

        try
        {
            // NetworkPrefab 생성 및 등록
            NetworkObject netObj = gameObject.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                // 이미 시작된 NetworkManager인 경우
                if (_networkManager.IsListening)
                {
                    // 이미 시작된 후에는 PrefabHandler를 통해 추가
                    _networkManager.PrefabHandler.AddHandler(gameObject, new DynamicPrefabInstanceHandler(gameObject));
                    Debug.Log($"[NetworkRegistryService] {prefabName}을 PrefabHandler를 통해 등록했습니다.");
                }
                else
                {
                    // 시작 전이라면 NetworkConfig에 직접 추가
                    NetworkPrefab networkPrefab = new NetworkPrefab { Prefab = gameObject };
                    _networkManager.NetworkConfig.Prefabs.Add(networkPrefab);
                    Debug.Log($"[NetworkRegistryService] {prefabName}을 NetworkConfig.Prefabs에 등록했습니다.");
                }

                // 등록된 프리팹 추적
                _registeredPrefabs.Add(prefabName, gameObject);
            }
            else
            {
                Debug.LogWarning($"[NetworkRegistryService] {prefabName}에는 NetworkObject 컴포넌트가 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkRegistryService] 프리팹 등록 중 오류: {e.Message}\n{e.StackTrace}");
        }
    }
    
    // ResourceManager로 생성된 오브젝트의 사전 등록 (ObjectManager.Spawn 메서드를 위한)
    public void PreRegisterPrefab(string prefabName, GameObject prefab)
    {
        if (string.IsNullOrEmpty(prefabName) || prefab == null || _registeredPrefabs.ContainsKey(prefabName))
            return;
        
        try
        {
            NetworkObject netObj = prefab.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                netObj = prefab.AddComponent<NetworkObject>();
            }
            
            // NetworkManager에 등록
            if (_networkManager.IsListening)
            {
                _networkManager.PrefabHandler.AddHandler(prefab, new DynamicPrefabInstanceHandler(prefab));
            }
            else
            {
                NetworkPrefab networkPrefab = new NetworkPrefab { Prefab = prefab };
                _networkManager.NetworkConfig.Prefabs.Add(networkPrefab);
            }
            
            _registeredPrefabs.Add(prefabName, prefab);
            Debug.Log($"[NetworkRegistryService] {prefabName} 프리팹이 사전 등록되었습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkRegistryService] 프리팹 사전 등록 중 오류: {e.Message}");
        }
    }
    
    // 수동으로 스폰된 오브젝트 등록 (ObjectManager에서 호출)
    public void RegisterSpawnedObject(GameObject gameObject, ulong networkId)
    {
        if (gameObject == null || _spawnedObjects.ContainsKey(networkId)) 
            return;
        
        _spawnedObjects[networkId] = gameObject;
        OnObjectSpawned?.Invoke(gameObject, networkId);
        Debug.Log($"[NetworkRegistryService] 외부에서 스폰된 오브젝트 등록: ID={networkId}, Name={gameObject.name}");
    }
    
    // 네트워크 ID로 게임오브젝트 검색
    public GameObject GetObjectByNetworkId(ulong networkId)
    {
        if (_spawnedObjects.TryGetValue(networkId, out GameObject gameObject))
            return gameObject;
        
        return null;
    }
    
    // 오브젝트가 파괴될 때 목록에서 제거
    public void UnregisterObject(ulong networkId)
    {
        if (_spawnedObjects.ContainsKey(networkId))
        {
            _spawnedObjects.Remove(networkId);
            Debug.Log($"[NetworkRegistryService] NetworkObject ID {networkId}의 추적이 중지되었습니다.");
        }
    }

public void RegisterPrefabAndSpawn(GameObject gameObject, bool spawnImmediately = false)
{
    if (gameObject == null) return;
    
    // 먼저 NetworkObject 컴포넌트 확인
    NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
    if (networkObject == null)
    {
        networkObject = gameObject.AddComponent<NetworkObject>();
        Debug.Log($"[NetworkRegistryService] {gameObject.name}에 NetworkObject 컴포넌트 추가함");
    }
    
    // 등록 및 스폰
    if (_networkManager.IsServer)
    {
        // 현재 등록된 프리팹 중에 있는지 확인
        bool foundInPrefabs = false;
        foreach (var prefab in _networkManager.NetworkConfig.Prefabs.Prefabs)
        {
            if (prefab.Prefab.name == gameObject.name)
            {
                foundInPrefabs = true;
                break;
            }
        }
        
        // 등록되어 있지 않다면 추가
        if (!foundInPrefabs)
        {
            NetworkPrefab networkPrefab = new NetworkPrefab
            {
                Prefab = gameObject
            };
            _networkManager.NetworkConfig.Prefabs.Add(networkPrefab);
            Debug.Log($"[NetworkRegistryService] {gameObject.name}을 NetworkConfig.Prefabs에 직접 등록함");
        }
        
        // 스폰
        if (spawnImmediately && !networkObject.IsSpawned)
        {
            networkObject.Spawn(true);
            Debug.Log($"[NetworkRegistryService] {gameObject.name} 스폰 완료 (Owner Transfer 포함)");
        }
    }
}


}

// 동적으로 생성된 오브젝트를 위한 커스텀 PrefabInstanceHandler
public class DynamicPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private readonly GameObject _prefab;

    public DynamicPrefabInstanceHandler(GameObject prefab)
    {
        _prefab = prefab;
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        GameObject instantiated = GameObject.Instantiate(_prefab, position, rotation);
        return instantiated.GetComponent<NetworkObject>();
    }

    public void Destroy(NetworkObject networkObject)
    {
        GameObject.Destroy(networkObject.gameObject);
    }



}

