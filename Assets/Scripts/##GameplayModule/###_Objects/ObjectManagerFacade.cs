using System.Collections;
using UnityEngine;
using static Define;
using VContainer;
using Unity.Assets.Scripts.Objects;
using Unity.Netcode;
using System;

/// <summary>
/// 스폰 및 그리드 관리 기능을 제공하는 클래스입니다.
/// </summary>
public class ObjectManagerFacade : NetworkBehaviour
{   
    [Inject] private ObjectManager _objectManager;
    [Inject] private MapSpawnerFacade _mapSpawnerFacade;
    [Inject] private NetworkManager _networkManager;
    [Inject] private NetUtils _netUtils;
    [Inject] private INetworkMediator _networkMediator;

    private Coroutine _spawnMonsterCoroutine;

    private void Awake()
    {
        InitializeNetworkObject();
        SubscribeToEvents();
    }

    private void InitializeNetworkObject()
    {
        var netObj = GetComponent<NetworkObject>() ?? gameObject.AddComponent<NetworkObject>();
        if (!netObj.IsSpawned)
        {
            netObj.Spawn();
            Debug.Log("[ObjectManagerFacade] 네트워크 오브젝트 스폰 완료");
        }
    }

    private void SubscribeToEvents()
    {
        MapSpawnerFacade.GridSpawned += OnGridSpawned;
        _networkMediator.RegisterHandler(NetworkEventType.NetworkSpawned, OnNetworkObjectSpawned);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        StopSpawnCoroutine();
    }

    private void UnsubscribeFromEvents()
    {
        MapSpawnerFacade.GridSpawned -= OnGridSpawned;
        Debug.Log("[ObjectManagerFacade] OnDestroy: 이벤트 구독 해제");
    }

    private void StopSpawnCoroutine()
    {
        if (_spawnMonsterCoroutine != null)
        {
            StopCoroutine(_spawnMonsterCoroutine);
            _spawnMonsterCoroutine = null;
        }
    }

    private void OnGridSpawned()
    {
        Debug.Log("[ObjectManagerFacade] OnGridSpawned: 이동 경로 설정 완료 후 GridSpawned 이벤트 직접 호출");
        // monster 를 servermonster 로 변경 TODO
        Spawn_Monster(false, "Monster", 202001);
    }

    private void OnNetworkObjectSpawned(MonsterSpawnEventData data)
    {
        if (!_networkManager.IsServer) return;

        if (_netUtils.TryGetSpawnedObject(data.NetworkObjectId, out NetworkObject monsterNetworkObject))
        {
            SetupMonsterPosition(monsterNetworkObject);
        }
    }

    private void SetupMonsterPosition(NetworkObject monsterNetworkObject)
    {
        var moveList = _mapSpawnerFacade.Player_move_list;
        if (moveList.Count > 0)
        {
            monsterNetworkObject.transform.position = moveList[0];
            monsterNetworkObject.GetComponent<ServerMonster>()?.SetMoveList(moveList);
            Debug.Log($"[ObjectManagerFacade] 서버에서 몬스터 위치 및 이동 경로 설정 완료: {monsterNetworkObject.NetworkObjectId}");
        }
    }

    public void Spawn_Monster(bool getBoss, string prefabName, int templateID)
    {
        if (this == null)
        {
            Debug.LogError("[ObjectManagerFacade] MonoBehaviour 인스턴스가 유효하지 않습니다.");
            return;
        }

        Vector3 spawnPos = _mapSpawnerFacade.Player_move_list[0];
        Debug.Log($"<color=yellow>[ObjectManagerFacade] 위치: {spawnPos}</color>");
        
        StopSpawnCoroutine();
        _spawnMonsterCoroutine = StartCoroutine(SpawnMonsterRoutine(getBoss, prefabName, spawnPos, templateID));
    }

    private IEnumerator SpawnMonsterRoutine(bool getBoss, string prefabName, Vector3 spawnPos, int templateID)
    {
        while (!getBoss)
        {
            yield return new WaitForSeconds(0.1f);
            SpawnSingleMonster(prefabName, spawnPos,  templateID);
        }
    }

    private void SpawnSingleMonster(string prefabName, Vector3 spawnPos, int templateID)
    {
        try
        {
            ulong ClientId = 1;
            Vector3 cellPos = new Vector3(spawnPos.x, spawnPos.y, 0);
            ServerMonster monster = _objectManager.Spawn<ServerMonster>(cellPos, ClientId, templateID, prefabName);
            
            // 성공적으로 스폰된 몬스터 인스턴스에 대해 DataLoader 의존성이 주입되었는지 검사
            if (monster != null) {
                Debug.Log($"[ObjectManagerFacade] 몬스터 스폰 성공: ID={templateID}, Position={cellPos}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ObjectManagerFacade] 몬스터 스폰 중 예외 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }

    [ClientRpc]
    public void ClientMonsterSetClientRpc(ulong networkObjectId, ulong clientId) 
    {
        if (_netUtils.TryGetSpawnedObject(networkObjectId, out NetworkObject monsterNetworkObject)) 
        {
            var moveList = _mapSpawnerFacade.Player_move_list;
            
            if (moveList.Count > 0)
            {
                monsterNetworkObject.transform.position = moveList[0];
                monsterNetworkObject.GetComponent<ServerMonster>().SetMoveList(moveList);
            }
            else
            {
                Debug.LogError($"[ObjectManagerFacade] 이동 경로 리스트가 비어 있습니다. ClientID: {clientId}, LocalID: {_netUtils.LocalID()}");
            }
        }
    }
}