
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

//class 하나더만들어서 basicGameManager 에서 사용하는 것으로 변경
[RequireComponent(typeof(NetcodeHooks))]

public class BasicGameState : GameStateLifetimeScope
{    
    [SerializeField]
    NetcodeHooks m_NetcodeHooks;
    public int Wave = 1;
    public int Money = 50;
    public int SummonCount = 20;
    public int HeroCount;
    public int HeroMaximumCount = 25;
    public int MonsterLimitCount = 100;

    public bool GetBoss = false;
    public int UpgradeMoney = 100;
    public float Timer = 20.0f;

    public override GameState ActiveState { get { return GameState.BasicGame; } }
    public void Awake()
    {            
        base.Awake();
        InitializeNetworkObject();

    }

    private void InitializeNetworkObject()
    {
        // 서버인 경우에만 NetworkObject 스폰
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            var netObj = GetComponent<NetworkObject>() ?? gameObject.AddComponent<NetworkObject>();
            if (!netObj.IsSpawned)
            {
                netObj.Spawn();
                Debug.Log("[BasicGameState] 네트워크 오브젝트 스폰 완료");
            }
        }
        else
        {
            Debug.Log("[BasicGameState] 클라이언트에서는 네트워크 오브젝트를 스폰할 수 없음");
        }
    }

    void OnNetworkSpawn()
    {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                // SessionManager<SessionPlayerData>.Instance.OnSessionEnded();
                // networkPostGame.WinState.Value = m_PersistentGameState.WinState;
            }
    }

    protected override void OnDestroy()
    {

            base.OnDestroy();

            m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
    }
    // private void InitializeNetworkObject()
    // {
    //     var netObj = GetComponent<NetworkObject>() ?? gameObject.AddComponent<NetworkObject>();
    //     if (!netObj.IsSpawned)
    //     {
    //         netObj.Spawn();
    //         Debug.Log("[BasicGameManager] 네트워크 오브젝트 스폰 완료");
    //     }
    // }
}

