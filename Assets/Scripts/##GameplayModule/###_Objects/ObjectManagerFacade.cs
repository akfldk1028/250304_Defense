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

    public void Awake()
    {
        InitializeNetworkObject();
        _networkMediator.RegisterHandler(NetworkEventType.NetworkSpawned, OnNetworkObjectSpawned);
    }

    private void InitializeNetworkObject()
    {
        var netObj = GetComponent<NetworkObject>() ?? gameObject.AddComponent<NetworkObject>();
        if (!netObj.IsSpawned)
        {
            if (_networkManager != null && _networkManager.IsServer)
            {
                netObj.Spawn();
                Debug.Log("[ObjectManagerFacade] 네트워크 오브젝트 스폰 완료 (서버)");
            }
            else
            {
                Debug.Log("[ObjectManagerFacade] 클라이언트에서는 네트워크 오브젝트를 스폰하지 않음");
            }
        }
}
    private void OnDestroy()
    {
        StopSpawnCoroutine();
    }



    private void StopSpawnCoroutine()
    {
        if (_spawnMonsterCoroutine != null)
        {
            StopCoroutine(_spawnMonsterCoroutine);
            _spawnMonsterCoroutine = null;
        }
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


    Coroutine spawn_Monster_Coroutine;

    public void Spawn_Monster(bool getBoss, int templateID)
    {
        spawn_Monster_Coroutine = StartCoroutine( SpawnMonsterRoutine(false, templateID ));

        // Vector3 spawnPos = _mapSpawnerFacade.Player_move_list[0];
        // Debug.Log($"<color=yellow>[ObjectManagerFacade] 위치: {spawnPos}</color>");
        
        // StopSpawnCoroutine();
        // _spawnMonsterCoroutine = StartCoroutine(SpawnMonsterRoutine(getBoss, spawnPos, templateID));
    }


    private IEnumerator SpawnMonsterRoutine(bool getBoss, int templateID)
    {
        yield return new WaitForSeconds(getBoss == false ? 0.1f : 0.1f);

        _netUtils.HostAndClientMethod(
                () => ServerMonsterSpawnServerRpc(_netUtils.LocalID(), getBoss , templateID),
                () => SpawnSingleMonster(_netUtils.LocalID(), getBoss, templateID ));

        if (getBoss) yield break;   

        spawn_Monster_Coroutine =  StartCoroutine(SpawnMonsterRoutine(getBoss, templateID));


        // while (!getBoss)
        // {
        //     yield return new WaitForSeconds(0.1f);
        //     SpawnSingleMonster(spawnPos,  templateID);
        // }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerMonsterSpawnServerRpc(ulong clientId, bool GetBoss , int templateID)
    {
         SpawnSingleMonster(clientId, GetBoss, templateID);
    }


    private void SpawnSingleMonster(ulong clientId, bool isBoss, int templateID)
    {
        try
        {
            // 클라이언트 ID에 따라 적절한 이동 경로 선택
            var moveList = clientId == _netUtils.LocalID() 
                ? _mapSpawnerFacade.Player_move_list 
                : _mapSpawnerFacade.Other_move_list;

            if (moveList.Count == 0)
            {
                Debug.LogError($"[ObjectManagerFacade] 이동 경로 리스트가 비어 있습니다. ClientID: {clientId}");
                return;
            }

            // 스폰 위치 설정
            Vector3 spawnPos = moveList[0];
            Vector3 cellPos = new Vector3(spawnPos.x, spawnPos.y, 0);
            
            // 몬스터 스폰
            ServerMonster monster = _objectManager.Spawn<ServerMonster>(cellPos, clientId, templateID);
            
            if (monster != null)
            {
                // 몬스터 스폰 성공 시 클라이언트에 알림
                NetworkObject netObj = GetComponent<NetworkObject>();
                MonsterSetClientRpc(netObj.NetworkObjectId, clientId);
                
                Debug.Log($"[ObjectManagerFacade] 몬스터 스폰 성공: ID={templateID}, Position={cellPos}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ObjectManagerFacade] 몬스터 스폰 중 예외 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }

    [ClientRpc]
    public void MonsterSetClientRpc(ulong networkObjectId, ulong clientId) 
    {
        if (_netUtils.TryGetSpawnedObject(networkObjectId, out NetworkObject monsterNetworkObject)) 
        {
            var moveList = clientId == _netUtils.LocalID() ? _mapSpawnerFacade.Player_move_list : _mapSpawnerFacade.Other_move_list;
            
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


    //    if (moveList.Count > 0)
    //         {
    //             monsterNetworkObject.transform.position = moveList[0];
    //             monsterNetworkObject.GetComponent<ServerMonster>().SetMoveList(moveList);
    //         }
    //         else
    //         {
              
}