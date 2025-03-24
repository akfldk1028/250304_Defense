using System.Collections;
using UnityEngine;
using static Define;
using VContainer;
using Unity.Assets.Scripts.Objects;
using Unity.Netcode;
using System;
using Unity.Assets.Scripts.Resource;
using Unity.Assets.Scripts.Scene;

/// <summary>
/// 스폰 및 그리드 관리 기능을 제공하는 클래스입니다.
/// </summary>
public class ObjectManagerFacade : NetworkBehaviour
{   
    // protected로 되어 있지만 외부에서 직접 설정할 수 있도록 유지
    [Inject] public ObjectManager _objectManager;
    public MapSpawnerFacade _mapSpawnerFacade;
    [Inject] private NetworkManager _networkManager;
    [Inject] public NetUtils _netUtils;
    [Inject] private INetworkMediator _networkMediator;
    [Inject] public ResourceManager _resourceManager;
    [Inject] private VContainer.IObjectResolver _container;

    private Coroutine _spawnMonsterCoroutine;
    private GameObject _ObjectSpawner;
    RateLimitCooldown m_RateLimitQuery;

    // 기본 생성자 추가
    public ObjectManagerFacade() 
    {
        Debug.Log("[ObjectManagerFacade] 기본 생성자 호출");
    }

    // 매개변수가 있는 생성자 유지
    [Inject]
    public ObjectManagerFacade(MapSpawnerFacade mapSpawnerFacade = null, ObjectManager objectManager = null, NetUtils netUtils = null)
    {
        _mapSpawnerFacade = mapSpawnerFacade;
        _objectManager = objectManager;
        _netUtils = netUtils;
        Debug.Log($"[ObjectManagerFacade] 생성자 호출: MapSpawnerFacade {(_mapSpawnerFacade != null ? "주입됨" : "주입되지 않음")}, ObjectManager {(_objectManager != null ? "주입됨" : "주입되지 않음")}, NetUtils {(_netUtils != null ? "주입됨" : "주입되지 않음")}");
    }
    public void Awake()
    {
        Debug.Log("[ObjectManagerFacade] Awake 호출됨");
    }
  
    public void Initialize()
    {
        Debug.Log("[ObjectManagerFacade] Initialize 시작");
        
        // _mapSpawnerFacade가 null이고 컨테이너가 사용 가능한 경우에만 시도
        if (_mapSpawnerFacade == null && _container != null)
        {
            Debug.Log("[ObjectManagerFacade] Initialize에서 MapSpawnerFacade 해결 시도");
            try
            {
                _mapSpawnerFacade = _container.Resolve<MapSpawnerFacade>();
                if (_mapSpawnerFacade != null)
                {
                    Debug.Log("[ObjectManagerFacade] MapSpawnerFacade 해결 성공");
                }
                else
                {
                    Debug.LogError("[ObjectManagerFacade] MapSpawnerFacade 해결 실패: null 반환됨");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ObjectManagerFacade] MapSpawnerFacade 해결 중 예외 발생: {ex.Message}");
            }
        }
        else
        {
            Debug.Log($"[ObjectManagerFacade] MapSpawnerFacade 상태: {(_mapSpawnerFacade != null ? "유효함" : "null")}");
        }
        
        // RateLimitCooldown 초기화
        if (m_RateLimitQuery == null)
        {
            m_RateLimitQuery = new RateLimitCooldown(3f);
            Debug.Log("[ObjectManagerFacade] RateLimitCooldown 초기화 완료");
        }
        
        Debug.Log("[ObjectManagerFacade] Initialize 완료");
    }
    
    public void Load()
    {
        Debug.Log("[ObjectManagerFacade] Load 시작");
        
        // ResourceManager가 null인지 확인
        if (_resourceManager == null)
        {
            Debug.LogWarning("[ObjectManagerFacade] ResourceManager가 null입니다.");
        }
        
        // Network 이벤트 등록
        if (_networkMediator != null)
        {
            _networkMediator.RegisterHandler(NetworkEventType.NetworkSpawned, OnNetworkObjectSpawned);
            Debug.Log("[ObjectManagerFacade] NetworkSpawned 이벤트 핸들러 등록");
        }
        else
        {
            Debug.LogWarning("[ObjectManagerFacade] _networkMediator가 null입니다.");
        }
        
        // _ObjectSpawner 설정
        _ObjectSpawner = this.gameObject;
        
        // RateLimitCooldown 초기화 (아직 초기화되지 않은 경우)
        if (m_RateLimitQuery == null)
        {
            m_RateLimitQuery = new RateLimitCooldown(3f);
            Debug.Log("[ObjectManagerFacade] RateLimitCooldown 초기화 완료");
        }
        
        Debug.Log("[ObjectManagerFacade] Load 완료");
    }
 
     private void OnDestroy()
    {
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
        // _mapSpawnerFacade 초기화 재시도
        try
        {
            if (_mapSpawnerFacade == null && _container != null)
            {
                Debug.Log("[ObjectManagerFacade] _container를 통해 MapSpawnerFacade 재시도");
                _mapSpawnerFacade = _container.Resolve<MapSpawnerFacade>();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ObjectManagerFacade] MapSpawnerFacade 해결 중 오류: {ex.Message}");
        }
     
        if (_objectManager == null || _mapSpawnerFacade == null || _netUtils == null)
        {
            if (_objectManager == null){
                Debug.LogError("[ObjectManagerFacade] _objectManager가 null입니다.");
            }
            if (_mapSpawnerFacade == null)
            {
                Debug.LogError("[ObjectManagerFacade] _mapSpawnerFacade가 null입니다.");
                // 마지막 방법으로 씬에서 직접 찾기 시도
                _mapSpawnerFacade = GameObject.FindObjectOfType<MapSpawnerFacade>();
                
                if (_mapSpawnerFacade == null)
                {
                    Debug.LogError("[ObjectManagerFacade] MapSpawnerFacade를 찾을 수 없습니다. 몬스터 스폰을 중단합니다.");
                    return; // 여기서 메서드 종료
                }
            }
            else
            {
                Debug.Log("[ObjectManagerFacade] _mapSpawnerFacade가 이미 초기화되었습니다.");
            }
            if (_netUtils == null){
                Debug.LogError("[ObjectManagerFacade] _netUtils가 null입니다.");
            }
            Debug.Log("[ObjectManagerFacade] 일부 의존성 객체가 null이지만 계속 진행합니다.");
        }
                // 의존성 직접 찾기

        Debug.Log("[ObjectManagerFacade] Spawn_Monster 시작");
        Debug.Log($"[ObjectManagerFacade] _mapSpawnerFacade.Player_move_list: {_mapSpawnerFacade.Player_move_list}");
        
        // m_RateLimitQuery가 null이 아닌지 확인
        if (m_RateLimitQuery == null)
        {
            Debug.LogError("[ObjectManagerFacade] m_RateLimitQuery가 null입니다.");
            return;
        }
        
        m_RateLimitQuery.PutOnCooldown();
        Debug.Log("[ObjectManagerFacade] 스포너가 초기화되었습니다. 몬스터 스폰을 시작합니다.");
        
        // 이 객체가 활성화되었는지 확인
        if (!isActiveAndEnabled)
        {
            Debug.LogError("[ObjectManagerFacade] 컴포넌트가 비활성화되었습니다.");
            return;
        }
        
        spawn_Monster_Coroutine = StartCoroutine(SpawnMonsterRoutine(getBoss, templateID));
    }


    private IEnumerator SpawnMonsterRoutine(bool getBoss, int templateID)
    {

        Debug.Log("[ObjectManagerFacade] SpawnMonsterRoutine 시작");
        yield return new WaitForSeconds(getBoss == false ? 0.1f : 0.1f);

        NetUtils.HostAndClientMethod(
                () => ServerMonsterSpawnServerRpc(NetUtils.LocalID(), getBoss , templateID),
                () => SpawnSingleMonster(NetUtils.LocalID(), getBoss, templateID));

        // if (getBoss) yield break;   

        spawn_Monster_Coroutine = StartCoroutine(SpawnMonsterRoutine(getBoss, templateID));
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
            var moveList = clientId == NetUtils.LocalID()
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
                var moveList = clientId == NetUtils.LocalID() ? _mapSpawnerFacade.Player_move_list : _mapSpawnerFacade.Other_move_list;
                
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
                    Debug.LogError($"[ObjectManagerFacade] 이동 경로 리스트가 비어 있습니다. ClientID: {clientId}, LocalID: {NetUtils.LocalID()}");
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