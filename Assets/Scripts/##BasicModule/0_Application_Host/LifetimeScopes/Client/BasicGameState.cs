
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;
using VContainer;
using Unity.Assets.Scripts.Resource;

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
    [Inject] public ResourceManager _resourceManager;
    private GameObject _BasicGameLifetimeScope_Server;

    [Inject] private ObjectManager _objectManager;
    [Inject] private MapManager _mapManager;


    public override GameState ActiveState { get { return GameState.BasicGame; } }
    public void Awake()
    {            
        base.Awake();

    }

    public void Initialize(){
    Debug.Log("[BasicGameState] Initialize 호출됨");
        
        // 의존성 주입 확인
        CheckDependencyInjection();
        
    }
       private void CheckDependencyInjection()
    {
        // ResourceManager 주입 확인
        if (_resourceManager != null)
        {
            Debug.Log("[BasicGameState] ResourceManager 의존성 주입 성공!");
        }
        else
        {
            Debug.LogError("[BasicGameState] ResourceManager 의존성 주입 실패!");
        }
        
        // ObjectManager 주입 확인
        if (_objectManager != null)
        {
            Debug.Log("[BasicGameState] ObjectManager 의존성 주입 성공!");
        }
        else
        {
            Debug.LogError("[BasicGameState] ObjectManager 의존성 주입 실패!");
        }
        
        // MapManager 주입 확인
        if (_mapManager != null)
        {
            Debug.Log("[BasicGameState] MapManager 의존성 주입 성공!");
        }
        else
        {
            Debug.LogError("[BasicGameState] MapManager 의존성 주입 실패!");
        }
        
        // 인스턴스 식별을 위한 정보 출력
        Debug.Log($"[BasicGameState] 인스턴스 ID: {GetInstanceID()}, GameObject 이름: {gameObject.name}");
    }
    
    public void Load()
    {
        // GameObject BasicGameLifetimeScope_Server = _resourceManager.Load<GameObject>("BasicGameLifetimeScope_Server".EndsWith(".prefab") ? "BasicGameLifetimeScope_Server".Replace(".prefab", "") : "BasicGameLifetimeScope_Server");

        // _BasicGameLifetimeScope_Server = Instantiate(BasicGameLifetimeScope_Server);  
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

