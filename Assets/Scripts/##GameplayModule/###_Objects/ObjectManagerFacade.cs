using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using VContainer;
using Unity.Assets.Scripts.Objects;
using Unity.Netcode;
using System;
using Unity.Assets.Scripts.Resource;
using Unity.Assets.Scripts.Scene;
using Unity.Netcode.Components;

/// <summary>
/// 스폰 및 그리드 관리 기능을 제공하는 클래스입니다.
/// </summary>
public class ObjectManagerFacade : NetworkBehaviour
{   
    // protected로 되어 있지만 외부에서 직접 설정할 수 있도록 유지
    [Inject] public ObjectManager _objectManager;
    [Inject] MapSpawnerFacade _mapSpawnerFacade;
    [Inject] private NetworkManager _networkManager;
    [Inject] public NetUtils _netUtils;
    [Inject] private INetworkMediator _networkMediator;
    [Inject] public ResourceManager _resourceManager;

    private Coroutine _spawnMonsterCoroutine;
    private GameObject _ObjectSpawner;
    RateLimitCooldown m_RateLimitQuery;
    private bool _isDestroyed = false;

    // 기본 생성자 추가
  
    public void Awake()
    {
        Debug.Log("[ObjectManagerFacade] Awake 호출됨");
    }
  
    public void Initialize()
    {
        _isDestroyed = false;
    }
    
    public void Load()
    {
        if (_isDestroyed) return;
        
        // _networkMediator.RegisterHandler(NetworkEventType.NetworkSpawned, OnNetworkObjectSpawned);
        _ObjectSpawner = this.gameObject;
        m_RateLimitQuery = new RateLimitCooldown(3f);
    }
 
    private void OnDestroy()
    {
        _isDestroyed = true;
        if (_spawnMonsterCoroutine != null)
        {
            StopCoroutine(_spawnMonsterCoroutine);
            _spawnMonsterCoroutine = null;
        }
    }

    Coroutine spawn_Monster_Coroutine;

    public void Spawn_Monster(bool getBoss, int templateID)
    {
        if (_isDestroyed) return;
        
        if (_spawnMonsterCoroutine != null)
        {
            StopCoroutine(_spawnMonsterCoroutine);
        }
        
        _spawnMonsterCoroutine = StartCoroutine(SpawnMonsterRoutine(getBoss, templateID));
    }


    private IEnumerator SpawnMonsterRoutine(bool getBoss, int templateID)
    {   
        if (_isDestroyed) yield break;

        Debug.Log("[ObjectManagerFacade] SpawnMonsterRoutine 시작");
        yield return new WaitForSeconds(getBoss == false ? 0.1f : 0.1f);

        if (_isDestroyed) yield break;

        _netUtils.HostAndClientMethod_P(
                () => ServerMonsterSpawnServerRpc(NetUtils.LocalID(), getBoss, templateID),
                () => SpawnSingleMonster(NetUtils.LocalID(), getBoss, templateID));

        if (!_isDestroyed)
        {
            _spawnMonsterCoroutine = StartCoroutine(SpawnMonsterRoutine(getBoss, templateID));
        }
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
            var moveList = clientId == _netUtils.LocalID_P()
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
                
                Debug.Log($"[ObjectManagerFacade] 몬스터 스폰 성공: ID={templateID}, NetworkID={monster.NetworkObject.NetworkObjectId}");
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
        // NetworkObject 찾기
        if (NetUtils.TryGetSpawnedObject(networkObjectId, out NetworkObject monsterNetworkObject)) 
        {
            var moveList = clientId == _netUtils.LocalID_P() ? 
                _mapSpawnerFacade.Player_move_list : 
                _mapSpawnerFacade.Other_move_list;

            if (moveList != null && moveList.Count > 0)
            {
                // 위치를 명시적으로 설정 - 이 부분이 중요합니다
                Debug.Log($"[ObjectManagerFacade] 몬스터 위치 설정: {moveList[0]}, NetworkID={networkObjectId}");
                monsterNetworkObject.transform.position = moveList[0];
                
                ServerMonster monster = monsterNetworkObject.GetComponent<ServerMonster>();
                if (monster != null)
                {
                    // 경로 설정 
                    monster.SetMoveList(moveList);
                }
            }
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"[ObjectManagerFacade] MonsterSetClientRpc 예외 발생: {ex.Message}");
    }
}


// [ClientRpc]
// public void MonsterSetClientRpc(ulong networkObjectId, ulong clientId) 
// {
//     try 
//     {
//         // MapSpawnerFacade 재확보
     

//         if (NetUtils.TryGetSpawnedObject(networkObjectId, out NetworkObject monsterNetworkObject)) 
//         {
//             var moveList = clientId == NetUtils.LocalID() ? 
//                 _mapSpawnerFacade.Player_move_list : 
//                 _mapSpawnerFacade.Other_move_list;

//             monsterNetworkObject.transform.position = moveList[0];
//             monsterNetworkObject.GetComponent<ServerMonster>().SetMoveList(moveList);
     
//         }
//         else
//         {
//             Debug.LogError($"[ObjectManagerFacade] NetworkObjectId {networkObjectId}에 해당하는 객체를 찾을 수 없습니다");
//         }
//     }
//     catch (Exception ex)
//     {
//         Debug.LogError($"[ObjectManagerFacade] MonsterSetClientRpc 예외 발생: {ex.Message}\n{ex.StackTrace}");
//     }
// }



    //    if (moveList.Count > 0)
    //         {
    //             monsterNetworkObject.transform.position = moveList[0];
    //             monsterNetworkObject.GetComponent<ServerMonster>().SetMoveList(moveList);
    //         }
    //         else
    //         {
              
}