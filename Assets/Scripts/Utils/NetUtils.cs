using System;
using Unity.Assets.Scripts.Infrastructure;
using Unity.Netcode;
using UnityEngine;
using VContainer;

public class NetUtils
{

    [Inject] private NetworkManager _networkManager;
    public ulong LocalID()
    {
        return _networkManager.LocalClientId;
    }


    public void HostAndClientMethod(Action clientAction, Action HostAction)
    {
        if (_networkManager == null)
        {
            Debug.LogError("[NetUtils] _networkManager가 null입니다!");
            return;
        }
        
        if (_networkManager.IsClient) clientAction?.Invoke();
        else if (_networkManager.IsServer) HostAction?.Invoke();
    }

    public  bool TryGetSpawnedObject(ulong networkObjectId, out NetworkObject spawnedObject)
    {
        spawnedObject = null;
        
        if (_networkManager == null)
        {
            Debug.LogError("[NetUtils] NetworkManager.Singleton이 null입니다!");
            return false;
        }
        
        if (_networkManager.SpawnManager == null)
        {
            Debug.LogError("[NetUtils] NetworkManager.Singleton.SpawnManager가 null입니다!");
            return false;
        }
        
        return _networkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out spawnedObject);
    }

    public bool IsClientCheck(ulong clientId)
    {
        if (LocalID() == clientId) return true;
        return false;
    }
// NetUtils 클래스에 추가
    public void InitializeNetworkObject(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogError("[NetUtils] 대상 GameObject가 null입니다!");
            return;
        }
        
        var netObj = targetObject.GetComponent<NetworkObject>() ?? targetObject.AddComponent<NetworkObject>();
        
        // 오브젝트 이름에 기반한 고유한 해시 코드 생성
        string uniqueString = $"{targetObject.name}_{DateTime.Now.Ticks}_{UnityEngine.Random.Range(0, 10000)}";
        int hashCode = uniqueString.GetHashCode();
        uint uintHash = (uint)(hashCode & 0x7FFFFFFF); // 음수 제거
        
        // NetworkPrefabHandler에 등록 (씬 간 ID 충돌 방지)
        if (_networkManager != null && _networkManager.PrefabHandler != null)
        {
            // 고유 ID를 가진 프리팹으로 등록
            var prefabHandler = _networkManager.PrefabHandler;
            Type handlerType = prefabHandler.GetType();
            
            // 리플렉션으로 RegisteredPrefabOverrideCount 속성 접근 시도
            var countProperty = handlerType.GetProperty("RegisteredPrefabsCount", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (countProperty != null)
            {
                int prefabCount = (int)countProperty.GetValue(prefabHandler);
                uintHash = (uint)(prefabCount + 1) * 100; // 단순히 100의 배수로 증가하는 ID 생성
            }
        }
        
        // NetworkObject 클래스가 GlobalObjectIdHash에 대한 setter를 제공하지 않으므로
        // 리플렉션 사용
        try
        {
            var field = typeof(NetworkObject).GetField("GlobalObjectIdHash", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                field.SetValue(netObj, uintHash);
                Debug.Log($"[NetUtils] {targetObject.name}에 ID 할당됨: {uintHash}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NetUtils] ID 할당 중 오류: {ex.Message}");
        }
        
        // 서버인 경우에만 스폰
        if (_networkManager != null && _networkManager.IsServer && !netObj.IsSpawned)
        {
            netObj.Spawn();
            Debug.Log($"[NetUtils] {targetObject.name}의 네트워크 오브젝트 스폰 완료 (서버)");
        }
        else
        {
            Debug.Log($"[NetUtils] {targetObject.name}의 네트워크 오브젝트 설정 완료 (클라이언트)");
        }
    }

    // public static string RarityColor(Rarity rarity)
    // {
    //     switch (rarity)
    //     {
    //         case Rarity.Common: return "<color=#A4A4A4>";
    //         case Rarity.UnCommon: return "<color=#79FF73>";
    //         case Rarity.Rare: return "<color=#6EE5FF>";
    //         case Rarity.Hero: return "<color=#FF9EF5>";
    //         case Rarity.Legendary: return "<color=#FFBA13>";

    //     }
    //     return "";
    // }
}
