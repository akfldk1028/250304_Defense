using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Define;
using VContainer;
using Unity.Assets.Scripts.Resource;
using Unity.Assets.Scripts.Objects;
using Unity.Netcode;
using System;
using Unity.Assets.Scripts.Data;

/*
 * ===== 몬스터 관리 시스템 설명 =====
 * 
 * 1. HashSet<ServerMonster> Monsters
 *    목적: 게임 내에서 현재 활성화된 몬스터들을 효율적으로 추적
 *    담당: ObjectManager 클래스
 *    사용 시점: 몬스터가 게임에 등장할 때 추가, 죽거나 사라질 때 제거
 *    성능 이점: FindObjectsOfType 등의 무거운 검색 없이 O(1) 접근 가능
 *    주요 사용 사례: 
 *    - 범위 공격 시 영향받는 몬스터 계산
 *    - 웨이브 종료 조건 확인 (Monsters.Count == 0)
 *    - 전체 몬스터 상태 변경
 * 
 * 2. 풀링 시스템 (PoolManager)
 *    목적: 오브젝트 생성/파괴의 성능 비용 절감을 위한 메모리 관리
 *    담당: PoolManager/ResourceManager 클래스
 *    사용 시점: 오브젝트 생성 요청 시 풀에서 가져오고, 필요 없을 때 풀로 반환
 *    성능 이점: 가비지 컬렉션 감소, 오브젝트 생성 오버헤드 감소
 *    주요 사용 사례:
 *    - 자주 생성/파괴되는 몬스터, 투사체, 이펙트 등의 재활용
 * 
 * 3. 두 시스템의 관계
 *    - 완전히 독립적인 시스템으로, 서로 다른 목적을 위해 존재
 *    - 풀링은 메모리/성능 최적화, HashSet은 게임 로직 최적화
 *    - 둘을 함께 사용하면 성능과 코드 가독성이 모두 향상
 * 
 * 4. 실제 흐름
 *    몬스터 생성 시:
 *    1) ResourceManager.Instantiate(pooling: true) -> 풀에서 오브젝트 가져옴
 *    2) ObjectManager.Monsters.Add(monster) -> 활성 몬스터 목록에 추가
 * 
 *    몬스터 제거 시:
 *    1) ObjectManager.Monsters.Remove(monster) -> 활성 목록에서 제거
 *    2) ResourceManager.Destroy(go, pooling: true) -> 풀로 반환
 */

public enum NetworkEventType
{
    Spawning,           // 몬스터 생성 시작
    NetworkSpawned,     // 네트워크 스폰 완료
    ClientSync,         // 클라이언트 동기화
    PathSet,            // 경로 설정 완료
    AnimInit,           // 애니메이션 초기화
    HealthInit,         // 체력 초기화
    AIInit,             // AI 초기화
    SpawnComplete       // 모든 초기화 완료
}

// 몬스터 스폰 이벤트 데이터 클래스
public class MonsterSpawnEventData
{
    public GameObject MonsterObject { get; set; }
    public ulong NetworkObjectId { get; set; }
    public ulong ClientId { get; set; }
    public string PrefabName { get; set; }
    public Vector3 Position { get; set; }
    public bool IsBoss { get; set; }
    public List<Vector2> MovePath { get; set; }
    
    public MonsterSpawnEventData(GameObject monsterObject, string prefabName, Vector3 position, ulong clientId, bool isBoss = false)
    {
        MonsterObject = monsterObject;
        PrefabName = prefabName;
        Position = position;
        ClientId = clientId;
        IsBoss = isBoss;
        MovePath = new List<Vector2>();
    }
}

// 몬스터 스폰 중재자 인터페이스
public interface INetworkMediator
{
    void Notify(NetworkEventType eventType, MonsterSpawnEventData data);
    void RegisterHandler(NetworkEventType eventType, Action<MonsterSpawnEventData> handler);
}

// 실제 몬스터 스폰 중재자 구현
public class NetworkMediator : INetworkMediator
{
    private Dictionary<NetworkEventType, List<Action<MonsterSpawnEventData>>> _handlers = 
        new Dictionary<NetworkEventType, List<Action<MonsterSpawnEventData>>>();
    
	public void Notify(NetworkEventType eventType, MonsterSpawnEventData data)
	{
		if (data == null)
		{
			Debug.LogError($"[NetworkMediator] 전달된 데이터가 null입니다: {eventType}");
			return;
		}
		
		if (_handlers.TryGetValue(eventType, out var handlers))
		{
			if (handlers == null || handlers.Count == 0)
			{
				Debug.LogWarning($"[NetworkMediator] {eventType}에 대한 핸들러 리스트가 비어 있습니다.");
				return;
			}
			
			foreach (var handler in handlers)
			{
				if (handler == null)
				{
					Debug.LogWarning($"[NetworkMediator] {eventType}에 대한 핸들러가 null입니다.");
					continue;
				}
				
				try
				{
					handler(data);
				}
				catch (Exception ex)
				{
					Debug.LogError($"[NetworkMediator] 이벤트 처리 중 오류 발생: {eventType}, {ex.Message}\n{ex.StackTrace}");
				}
			}
		}
	}
    
    public void RegisterHandler(NetworkEventType eventType, Action<MonsterSpawnEventData> handler)
    {
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Action<MonsterSpawnEventData>>();
        }
        
        _handlers[eventType].Add(handler);
    }
}
public class ObjectManager
{
    [Inject] private ResourceManager _resourceManager;
    [Inject] private INetworkMediator _spawnMediator;
	public CreatureData CreatureData { get; private set; }

	// public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();

	// 요소 추가
// fruits.Add("사과");
// fruits.Add("바나나");
// fruits.Add("오렌지");
// fruits.Add("사과");  // 중복된 요소는 추가되지 않음

	public HashSet<ServerMonster> Monsters { get; } = new HashSet<ServerMonster>();
	// public HashSet<Projectile> Projectiles { get; } = new HashSet<Projectile>();
	// public HashSet<Env> Envs { get; } = new HashSet<Env>();
	// public HashSet<EffectBase> Effects { get; } = new HashSet<EffectBase>();
	// public HeroCamp Camp { get; private set; }

	#region Roots
	public Transform GetRootTransform(string name)
	{
		GameObject root = GameObject.Find(name);
		if (root == null)
			root = new GameObject { name = name };

		return root.transform;
	}

	public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
	public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }

	public Transform ProjectileRoot { get { return GetRootTransform("@Projectiles"); } }
	public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
	public Transform EffectRoot { get { return GetRootTransform("@Effects"); } }
	#endregion


	// public void ShowDamageFont(Vector2 position, float damage, Transform parent, bool isCritical = false)
	// {
	// 	GameObject go = Managers.Resource.Instantiate("DamageFont", pooling: true);
	// 	DamageFont damageText = go.GetComponent<DamageFont>();
	// 	damageText.SetInfo(position, damage, parent, isCritical);
	// }

	public GameObject SpawnGameObject(Vector3 position, string prefabName)
	{
        GameObject go = _resourceManager.Instantiate(prefabName, pooling: true, position: position);
		return go;
	}

	public T Spawn<T>(ulong clientId, int templateID, string prefabName) where T : BaseObject
	{
		// 기본 위치 (0,0,0)으로 스폰
		Vector3Int cellPos = new Vector3Int(0, 0, 0);
		Debug.Log($"[ObjectManager] Spawn<T> 호출: {prefabName}");
		return Spawn<T>(cellPos, clientId, templateID, prefabName);
	}

	public T Spawn<T>(Vector3Int cellPos, ulong clientId = 0, int templateID = 0, string prefabName = "") where T : BaseObject
	{
		Vector3 spawnPos = new Vector3(cellPos.x, cellPos.y, 0);
		return Spawn<T>(spawnPos, clientId, templateID, prefabName);
	}
	public event Action<ulong, ulong> OnMonsterSpawned; // (networkObjectId, clientId)

	public T Spawn<T>(Vector3 position, ulong clientID = 0, int templateID = 0, string prefabName = "") where T : BaseObject
	{
		Debug.Log($"[ObjectManager] Spawn<T> 호출: {prefabName}");
		Debug.Log($"[ObjectManager] Spawn<T> 호출: {position}");

		prefabName = typeof(T).Name;

		// 풀링 시스템을 통해 오브젝트 생성
		GameObject go = _resourceManager.Instantiate(prefabName, pooling: true, position: position);
		if (go == null)
		{
			Debug.LogError($"[ObjectManager] '{prefabName}' 프리팹을 인스턴스화하지 못했습니다.");
			return null;
		}
		go.name = prefabName;
		
		// 타입에 따라 적절한 부모 오브젝트 설정

		// 다른 타입의 경우 여기에 추가
		// else if (typeof(T) == typeof(Hero))
		// {
		//     go.transform.SetParent(HeroRoot);
		// }
		
		BaseObject obj = go.GetComponent<BaseObject>();
		if (obj == null)
		{
			Debug.LogError($"[ObjectManager] '{prefabName}' 오브젝트에 BaseObject 컴포넌트가 없습니다.");
			_resourceManager.Destroy(go);
			return null;
		}
		

		// NetworkObject 처리
		NetworkObject networkObject = go.GetComponent<NetworkObject>();

		try 
		{
			if (!networkObject.IsSpawned && NetworkManager.Singleton.IsServer)
			{
				networkObject.Spawn();
				Debug.Log($"[ObjectManager] NetworkObject 스폰 완료: {networkObject.NetworkObjectId}");
			}

			var eventData = new MonsterSpawnEventData(go, prefabName, position, clientID);
			eventData.NetworkObjectId = networkObject.NetworkObjectId;
			
			// 네트워크 스폰 완료 이벤트 발생
			_spawnMediator.Notify(NetworkEventType.NetworkSpawned, eventData);
		}
		catch (Exception e)
		{
			Debug.LogError($"[ObjectManager] NetworkObject 스폰 중 오류 발생: {e.Message}");
			_resourceManager.Destroy(go);
			return null;
		}




		if (obj.ObjectType == EObjectType.Creature)
		{
			Creature creature = obj.GetComponent<Creature>();

			if (creature.CreatureType == CharacterTypeEnum.Monster)
			{
    			MonsterRoot.SetParent(obj.transform, false);


				CreatureData = DataLoader.instance.MonsterDic[templateID];
				
				ClientMonster clientMonster = go.GetComponent<ClientMonster>();
				MonsterAvatarSO clientAvatar = _resourceManager.Load<MonsterAvatarSO>(CreatureData.ClientAvatar);
				clientMonster.SetAvatar(clientAvatar);

				ServerMonster monster = go.GetComponent<ServerMonster>();
				monster.SetInfo(templateID, CreatureData);
				Monsters.Add(monster);

			}


			// Creature creature = go.GetComponent<Creature>();
			// switch (creature.CreatureType)
			// {
			// 	case CharacterTypeEnum.Hero:
			// 		obj.transform.parent = HeroRoot;
			// 		// Hero hero = creature as Hero;
			// 		// Heroes.Add(hero);
			// 		break;
			// 	case CharacterTypeEnum.Monster:
			// 		obj.transform.parent = MonsterRoot;
			// 		ServerMonster monster = creature as ServerMonster;
			// 		Monsters.Add(monster);
			// 		Debug.Log($"[ObjectManager] Monsters 추가: {monster.MonsterId}");
			// 		break;
			// }
		}
		else if (obj.ObjectType == EObjectType.Projectile)
		{
			obj.transform.parent = ProjectileRoot;

			// Projectile projectile = go.GetComponent<Projectile>();
			// Projectiles.Add(projectile);

		// 	// projectile.SetInfo(templateID);
		}
		else if (obj.ObjectType == EObjectType.Env)
		{
			obj.transform.parent = EnvRoot;

			// Env env = go.GetComponent<Env>();
			// Envs.Add(env);

			// env.SetInfo(templateID);
		}
		else if (obj.ObjectType == EObjectType.HeroCamp)
		{
			// Camp = go.GetComponent<HeroCamp>();
		}
		return obj as T;
	}

	public void Despawn<T>(T obj) where T : BaseObject
	{

		EObjectType objectType = obj.ObjectType;

		if (obj.ObjectType == EObjectType.Creature)
		{
			Creature creature = obj.GetComponent<Creature>();
			switch (creature.CreatureType)
			{
				case CharacterTypeEnum.Hero:
					// Hero hero = creature as Hero;
					// Heroes.Remove(hero);
					break;
				case CharacterTypeEnum.Monster:
					ServerMonster monster = creature as ServerMonster;
					Monsters.Remove(monster);
					break;
			}
		}
		else if (obj.ObjectType == EObjectType.Projectile)
		{
			// Projectile projectile = obj as Projectile;
			// Projectiles.Remove(projectile);
		}
		else if (obj.ObjectType == EObjectType.Env)
		{
			// Env env = obj as Env;
			// Envs.Remove(env);
		}
		else if (obj.ObjectType == EObjectType.Effect)
		{
			// EffectBase effect = obj as EffectBase;
			// Effects.Remove(effect);
		}


		// Managers.Resource.Destroy(obj.gameObject);
	}

	#region Skill 판정
	// public List<Creature> FindConeRangeTargets(Creature owner, Vector3 dir, float range, int angleRange, bool isAllies = false)
	// {
	// 	HashSet<Creature> targets = new HashSet<Creature>();
	// 	HashSet<Creature> ret = new HashSet<Creature>();

	// 	ECreatureType targetType = Util.DetermineTargetType(owner.CreatureType, isAllies);

	// 	if (targetType == ECreatureType.Monster)
	// 	{
	// 		var objs = Managers.Map.GatherObjects<Monster>(owner.transform.position, range, range);
	// 		targets.AddRange(objs);
	// 	}
	// 	else if (targetType == ECreatureType.Hero)
	// 	{
	// 		var objs = Managers.Map.GatherObjects<Hero>(owner.transform.position, range, range);
	// 		targets.AddRange(objs);
	// 	}

	// 	foreach (var target in targets)
	// 	{
	// 		// 1. 거리안에 있는지 확인
	// 		var targetPos = target.transform.position;
	// 		float distance = Vector3.Distance(targetPos, owner.transform.position);

	// 		if (distance > range)
	// 			continue;

	// 		// 2. 각도 확인
	// 		if (angleRange != 360)
	// 		{
	// 			BaseObject ownerTarget = (owner as Creature).Target;

	// 			// 2. 부채꼴 모양 각도 계산
	// 			float dot = Vector3.Dot((targetPos - owner.transform.position).normalized, dir.normalized);
	// 			float degree = Mathf.Rad2Deg * Mathf.Acos(dot);

	// 			if (degree > angleRange / 2f)
	// 				continue;
	// 		}

	// 		ret.Add(target);
	// 	}

	// 	return ret.ToList();
	// }

	// public List<Creature> FindCircleRangeTargets(Creature owner, Vector3 startPos, float range, bool isAllies = false)
	// {
	// 	HashSet<Creature> targets = new HashSet<Creature>();
	// 	HashSet<Creature> ret = new HashSet<Creature>();

	// 	ECreatureType targetType = Util.DetermineTargetType(owner.CreatureType, isAllies);

	// 	if (targetType == ECreatureType.Monster)
	// 	{
	// 		var objs = Managers.Map.GatherObjects<Monster>(owner.transform.position, range, range);
	// 		targets.AddRange(objs);
	// 	}
	// 	else if (targetType == ECreatureType.Hero)
	// 	{
	// 		var objs = Managers.Map.GatherObjects<Hero>(owner.transform.position, range, range);
	// 		targets.AddRange(objs);
	// 	}

	// 	foreach (var target in targets)
	// 	{
	// 		// 1. 거리안에 있는지 확인
	// 		var targetPos = target.transform.position;
	// 		float distSqr = (targetPos - startPos).sqrMagnitude;

	// 		if (distSqr < range * range)
	// 			ret.Add(target);
	// 	}

	// 	return ret.ToList();
	// }
	#endregion
}
