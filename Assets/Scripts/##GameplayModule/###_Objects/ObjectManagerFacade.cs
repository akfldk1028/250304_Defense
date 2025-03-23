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
        _netUtils.InitializeNetworkObject(gameObject);
        //스포된거 알리고 완료되면 client에서 처리
        _networkMediator.RegisterHandler(NetworkEventType.NetworkSpawned, OnNetworkObjectSpawned);
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
            
            if (monster != null && monster.NetworkObject != null)
            {
                // 몬스터의 NetworkObject ID를 전달
                MonsterSetClientRpc(monster.NetworkObject.NetworkObjectId, clientId);
                
                Debug.Log($"[ObjectManagerFacade] 몬스터 스폰 성공: ID={templateID}, NetworkID={monster.NetworkObject.NetworkObjectId}, Position={cellPos}");
            }
            else
            {
                Debug.LogError("[ObjectManagerFacade] 몬스터 스폰 실패 또는 NetworkObject 없음");
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
        try 
        {
            Debug.Log($"[ObjectManagerFacade] MonsterSetClientRpc 호출됨 - NetworkID: {networkObjectId}, ClientID: {clientId}");
            
            if (_netUtils == null)
            {
                Debug.LogError("[ObjectManagerFacade] _netUtils가 null입니다");
                return;
            }
            
            if (_mapSpawnerFacade == null)
            {
                Debug.LogError("[ObjectManagerFacade] _mapSpawnerFacade가 null입니다");
                return;
            }
            
            if (_netUtils.TryGetSpawnedObject(networkObjectId, out NetworkObject monsterNetworkObject)) 
            {
                var moveList = clientId == _netUtils.LocalID() ? _mapSpawnerFacade.Player_move_list : _mapSpawnerFacade.Other_move_list;
                
                if (moveList == null)
                {
                    Debug.LogError("[ObjectManagerFacade] moveList가 null입니다");
                    return;
                }
                
                if (moveList.Count > 0)
                {
                    monsterNetworkObject.transform.position = moveList[0];
                    
                    var serverMonster = monsterNetworkObject.GetComponent<ServerMonster>();
                    if (serverMonster != null)
                    {
                        serverMonster.SetMoveList(moveList);
                        Debug.Log($"[ObjectManagerFacade] 몬스터 이동 경로 설정 완료: {networkObjectId}");
                    }
                    else
                    {
                        Debug.LogError($"[ObjectManagerFacade] ServerMonster 컴포넌트를 찾을 수 없습니다: {networkObjectId}");
                    }
                }
                else
                {
                    Debug.LogError($"[ObjectManagerFacade] 이동 경로 리스트가 비어 있습니다. ClientID: {clientId}, LocalID: {_netUtils.LocalID()}");
                }
            }
            else
            {
                Debug.LogError($"[ObjectManagerFacade] NetworkObjectId {networkObjectId}에 해당하는 객체를 찾을 수 없습니다");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ObjectManagerFacade] MonsterSetClientRpc 예외 발생: {ex.Message}\n{ex.StackTrace}");
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