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
