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
using System.Linq;
using Unity.Assets.Scripts.Data;


public class ObjectManagerFacade : NetworkBehaviour
{   
    // protected로 되어 있지만 외부에서 직접 설정할 수 있도록 유지
    [Inject] public ObjectManager _objectManager;
    [Inject] MapSpawnerFacade _mapSpawnerFacade;
    [Inject] private NetworkManager _networkManager;
    [Inject] public NetUtils _netUtils;
    [Inject] private INetworkMediator _networkMediator;
    [Inject] public ResourceManager _resourceManager;

    [Inject] private DebugClassFacade _debugClassFacade;

    private Coroutine _spawnMonsterCoroutine;
    private GameObject ObjectSpawner_O;
    RateLimitCooldown m_RateLimitQuery;
    private bool _isDestroyed = false;

    // 기본 생성자 추가
  
    public void Awake()
    {
        _debugClassFacade?.LogInfo(GetType().Name, "[ObjectManagerFacade] Awake 호출됨");
    }
  
    public void Initialize()
    {
        _isDestroyed = false;
    }
    
    public void Load()
    {
        if (_isDestroyed) return;
        
        // _networkMediator.RegisterHandler(NetworkEventType.NetworkSpawned, OnNetworkObjectSpawned);
        ObjectSpawner_O = this.gameObject;
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

        _debugClassFacade?.LogInfo(GetType().Name, "[ObjectManagerFacade] SpawnMonsterRoutine 시작");
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
                 _debugClassFacade?.LogError(GetType().Name, $"[ObjectManagerFacade] 이동 경로 리스트가 비어 있습니다. ClientID: {clientId}");

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
                _debugClassFacade?.LogInfo(GetType().Name, $"[ObjectManagerFacade] 몬스터 스폰 성공: ID={templateID}, NetworkID={monster.NetworkObject.NetworkObjectId}");
            }
      
        }
        catch (Exception ex)
        {
            _debugClassFacade?.LogError(GetType().Name, $"[ObjectManagerFacade] 몬스터 스폰 중 예외 발생: {ex.Message}\n{ex.StackTrace}");
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
                        monster.SetMoveList(moveList);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _debugClassFacade?.LogError(GetType().Name, $"[ObjectManagerFacade] MonsterSetClientRpc 예외 발생: {ex.Message}");
        }
    }

    public void Summon()
    {
 
        NetUtils.HostAndClientMethod(
            () => ServerSpawnHeroServerRpc(NetUtils.LocalID()),
            () => HeroSpawn(NetUtils.LocalID()));
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSpawnHeroServerRpc(ulong clientId)
    {
        HeroSpawn(clientId);
    }

    private void HeroSpawn(ulong clientId)
    {
    // 비어 있는 홀더 찾기
    string organizer = clientId == 0 ?  EOrganizer.HOST.ToString() :  EOrganizer.CLIENT.ToString();
    int value = clientId == 0 ? 0 : 1;
    _debugClassFacade?.LogInfo(GetType().Name, $"<color=yellow>[ObjectManagerFacade] 비어있는 홀더 검색 시작: prefix={organizer}</color>");
    bool foundEmptyHolder = false;
    for (int i = 0; i < _mapSpawnerFacade.Hero_Holders.Count / 2; i++) 
    {
        string OrganizersTemp = organizer + i.ToString();
        _debugClassFacade?.LogInfo(GetType().Name, $"<color=yellow>[ObjectManagerFacade] 홀더 검사: {OrganizersTemp}</color>");
        if (_mapSpawnerFacade.Hero_Holders.TryGetValue(OrganizersTemp, out var holder) && 
            holder.m_Heroes.Count == 0)
        {
            _debugClassFacade?.LogInfo(GetType().Name, $"<color=green>[ObjectManagerFacade] 비어있는 홀더 발견: {OrganizersTemp}</color>");
            _mapSpawnerFacade.Host_Client_Value_Index[value] = i;
            foundEmptyHolder = true;
            break;
        }
    }
    
    if (!foundEmptyHolder)
    {
        _debugClassFacade?.LogError(GetType().Name, $"<color=red>[ObjectManagerFacade] 비어있는 홀더를 찾지 못했습니다.</color>");
        return;
    }

    string Organizers = organizer + _mapSpawnerFacade.Host_Client_Value_Index[value].ToString();
   

   
    _debugClassFacade?.LogInfo(GetType().Name, $"<color=yellow>[ObjectManagerFacade] 선택된 홀더: {Organizers}</color>");


    // 홀더의 NetworkObject 가져오기
    if (!_mapSpawnerFacade.Hero_Holders.TryGetValue(Organizers, out var targetHolder))
    {
        _debugClassFacade?.LogError(GetType().Name, $"<color=red>[ObjectManagerFacade] 홀더를 찾을 수 없음: {Organizers}</color>");
        return;
    }
    
    var networkObject = targetHolder.GetComponent<NetworkObject>();
    
    var heroDataList = DataLoader.instance.HeroDic.Values.ToList();
    float totalWeight = heroDataList.Sum(hero => hero.GachaSpawnWeight);
    float randomValue = UnityEngine.Random.Range(0f, totalWeight);

    _debugClassFacade?.LogInfo(GetType().Name, $"<color=yellow>[ObjectManagerFacade] 전체 영웅 수: {heroDataList.Count}</color>");

    _debugClassFacade?.LogInfo(GetType().Name, $"<color=yellow>[ObjectManagerFacade] 전체 가중치 합: {totalWeight}</color>");

    _debugClassFacade?.LogInfo(GetType().Name, $"<color=yellow>[ObjectManagerFacade] 랜덤 값: {randomValue:F2}</color>");
    
    HeroData selectedHero = null;
    float currentWeight = 0f;
    
    _debugClassFacade?.LogInfo(GetType().Name, "<color=yellow>[ObjectManagerFacade] 영웅 가중치 목록:</color>");
    foreach (var hero in heroDataList)
    {
        currentWeight += hero.GachaSpawnWeight;
        _debugClassFacade?.LogInfo(GetType().Name, $"<color=yellow>  - {hero.DescriptionTextID}: {hero.GachaSpawnWeight} (누적: {currentWeight:F2})</color>");
        
        if (randomValue <= currentWeight)
        {
            selectedHero = hero;
            _debugClassFacade?.LogInfo(GetType().Name, $"<color=green>  >>> 선택됨!</color>");
            break;
        }
    }

    if (selectedHero != null)
    {
        _debugClassFacade?.LogInfo(GetType().Name, $"<color=green>[ObjectManagerFacade] 최종 선택된 영웅: {selectedHero.DescriptionTextID} ({selectedHero.Rarity})</color>");
        _debugClassFacade?.LogInfo(GetType().Name, $"<color=green>  - DataId: {selectedHero.DataId}</color>");
        
        try
        {
            // existingHolder가 있는지 확인
            var existingHolder = GetExistingHolder(Organizers, selectedHero.DataId);
            
            if (existingHolder != null)
            {
                // 기존 홀더가 있는 경우, 그 위치에 영웅 생성
                Vector3 spawnPosition = existingHolder.transform.position;
                var hero = _objectManager.Spawn<ServerHero>(spawnPosition, clientId, selectedHero.DataId);
                
                // 홀더에게 영웅 스폰 알림
                HeroSpawnClientRpc(hero.NetworkObject.NetworkObjectId, existingHolder.GetComponent<NetworkObject>().NetworkObjectId, clientId, selectedHero.DataId);
            }
            else
            {
                // 새 홀더에 영웅 생성
                Vector3 spawnPosition = targetHolder.transform.position;
                targetHolder.Holder_Name = selectedHero.DataId;
                var hero = _objectManager.Spawn<ServerHero>(spawnPosition, clientId, selectedHero.DataId);
                
                // 클라이언트에 알림
                HeroSpawnClientRpc(hero.NetworkObject.NetworkObjectId, networkObject.NetworkObjectId, clientId, selectedHero.DataId);
            }
        }
        catch (Exception ex)
        {
            _debugClassFacade?.LogError(GetType().Name, $"<color=red>[ObjectManagerFacade] 영웅 스폰 중 예외 발생: {ex.Message}\n{ex.StackTrace}</color>");
        }
    }
    else
    {
        _debugClassFacade?.LogError(GetType().Name, "<color=red>[ObjectManagerFacade] 영웅 선택 실패</color>");
    }
}

private UI_Spawn_Holder GetExistingHolder(string clientKey, int heroId)
{
    _debugClassFacade?.LogInfo(GetType().Name, $"<color=cyan>[ObjectManagerFacade] 기존 홀더 검색: clientKey='{clientKey}', heroId={heroId}</color>");
    
    string basePrefix = clientKey.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9'); // "HOST" 또는 "CLIENT"
    
    // 1. heroId가 이미 할당된 홀더 중에서 검색 (영웅 타입을 그룹화하기 위함)
    foreach (var holder in _mapSpawnerFacade.Hero_Holders)
    {
        bool hasCorrectPrefix = holder.Key.StartsWith(basePrefix); // HOST 또는 CLIENT로 시작하는지
        bool hasSameHeroId = holder.Value.Holder_Name == heroId; // 동일한 영웅 타입
        bool hasSpace = holder.Value.m_Heroes.Count < 3; // 최대 3개 제한
        
        _debugClassFacade?.LogInfo(GetType().Name, 
            $"<color=cyan>[ObjectManagerFacade] 동일 타입 홀더 검사: key='{holder.Key}', " +
            $"Holder_Name={holder.Value.Holder_Name}, " +
            $"Hero_Count={holder.Value.m_Heroes.Count}, " +
            $"HasCorrectPrefix={hasCorrectPrefix}, " +
            $"HasSameHeroId={hasSameHeroId}, " +
            $"HasSpace={hasSpace}</color>");
        
        if (hasCorrectPrefix && hasSameHeroId && hasSpace)
        {
            _debugClassFacade?.LogInfo(GetType().Name, $"<color=green>[ObjectManagerFacade] 동일 타입 홀더 발견: {holder.Key}</color>");
            return holder.Value;
        }
    }
    
    _debugClassFacade?.LogInfo(GetType().Name, $"<color=yellow>[ObjectManagerFacade] 기존 홀더를 찾지 못함: {clientKey}, {heroId}</color>");
    return null;
}

[ClientRpc]
private void HeroSpawnClientRpc(ulong heroNetworkId, ulong holderNetworkId, ulong clientId, int heroDataId)
{
    try
    {
        // 영웅 NetworkObject 찾기
        if (!NetUtils.TryGetSpawnedObject(heroNetworkId, out NetworkObject heroNetworkObject))
        {
            _debugClassFacade?.LogError(GetType().Name, $"<color=red>[ObjectManagerFacade] 영웅 NetworkObject를 찾을 수 없음: {heroNetworkId}</color>");
            return;
        }
        
        // 홀더 NetworkObject 찾기
        if (!NetUtils.TryGetSpawnedObject(holderNetworkId, out NetworkObject holderNetworkObject))
        {
            _debugClassFacade?.LogError(GetType().Name, $"<color=red>[ObjectManagerFacade] 홀더 NetworkObject를 찾을 수 없음: {holderNetworkId}</color>");
            return;
        }
        
        // 홀더 컴포넌트 가져오기
        UI_Spawn_Holder holder = holderNetworkObject.GetComponent<UI_Spawn_Holder>();
        
        // 영웅 컴포넌트 가져오기
        ServerHero hero = heroNetworkObject.GetComponent<ServerHero>();
        
        // 영웅을 홀더의 자식으로 설정 (중요!)
        heroNetworkObject.transform.SetParent(holderNetworkObject.transform);
        
        // 영웅 위치를 홀더 위치로 초기화
        heroNetworkObject.transform.position = holderNetworkObject.transform.position;
        
        // 홀더의 Heroes 리스트에 영웅 추가
        holder.m_Heroes.Add(hero);
        _debugClassFacade?.LogInfo(GetType().Name, $"<color=green>[ObjectManagerFacade] 홀더({holder.Holder_Part_Name})에 영웅 추가 성공</color>");
        
        // HeroData 가져오기
        if (DataLoader.instance.HeroDic.TryGetValue(heroDataId, out HeroData heroData))
        {
            // 홀더 이름 설정
            holder.Holder_Name = heroData.DataId;
            _debugClassFacade?.LogInfo(GetType().Name, $"<color=green>[ObjectManagerFacade] 홀더 이름 설정: {holder.Holder_Name}</color>");
        }
        
        // 홀더 내 영웅 위치 조정
        holder.CheckGetPosition();
    }
    catch (Exception ex)
    {
        _debugClassFacade?.LogError(GetType().Name, $"<color=red>[ObjectManagerFacade] HeroSpawnClientRpc 예외 발생: {ex.Message}</color>");
    }
}

              
}