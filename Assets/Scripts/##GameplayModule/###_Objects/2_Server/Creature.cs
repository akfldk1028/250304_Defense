using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;
using VContainer;
using Unity.Assets.Scripts.Data;

namespace Unity.Assets.Scripts.Objects
{
public class Creature : BaseObject, ITargetable
{

    [SerializeField]
    protected Guid creatureGuid; // 캐릭터의 GUID


	public BaseObject Target { get; protected set; }
	// public SkillComponent Skills { get; protected set; } //이게 과연 어떤파일로 오는가 M1파일확인

	public CharacterTypeEnum CreatureType { get; protected set; } = CharacterTypeEnum.None;
	
	[SerializeField]
	[Tooltip("이 생물체가 NPC인지 여부를 나타냅니다.")]
	private bool _isNpc = false; // NPC 여부를 나타내는 필드 (private으로 변경)
	private bool _isValidTarget = false; // NPC 여부를 나타내는 필드 (private으로 변경)

	// ITargetable 인터페이스 구현
	public bool IsNpc 
	{ 
		get { return _isNpc; } 
		set { _isNpc = value; }
	}

    public bool IsValidTarget => LifeState != LifeState.Dead;

	// public EffectComponent Effects { get; set; }

	float DistToTargetSqr
	{
		get
		{
			Vector3 dir = (Target.transform.position - transform.position);
			float distToTarget = Math.Max(0, dir.magnitude - Target.ExtraCells * 1f - ExtraCells * 1f); // TEMP
			return distToTarget * distToTarget;
		}
	}

	#region Stats
	public float Hp { get; set; }
	
	[Header("===== 기본 스탯 =====")]
	[Space(5)]
	[SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	public CreatureStat MaxHp = new CreatureStat(0);
	
	[SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	public CreatureStat Atk = new CreatureStat(0);
	
	[SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	public CreatureStat AtkRange = new CreatureStat(0);

	[SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	public CreatureStat AtkBonus = new CreatureStat(0);

	[SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	public CreatureStat MoveSpeed = new CreatureStat(0);


	[SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	public CreatureStat CriRate = new CreatureStat(0);
	
	[SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	public CreatureStat CriDamage = new CreatureStat(0);
	
	[Header("===== 추가 스탯  나중에 데이터에 넣어야함함=====")]
	[Space(5)]
	[SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	public CreatureStat ReduceDamageRate = new CreatureStat(0);

	public NetworkLifeState NetLifeState { get; private set; }

	public LifeState LifeState
        {
            get => NetLifeState.LifeState.Value;
            private set => NetLifeState.LifeState.Value = value;
        }
	// [SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	// public CreatureStat LifeStealRate = new CreatureStat(0);
	
	// [SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	// public CreatureStat ThornsDamageRate = new CreatureStat(0); // 쏜즈
	

	// [SerializeField] // 인스펙터에서 표시되도록 SerializeField 추가
	// public CreatureStat AttackSpeedRate = new CreatureStat(0);
	#endregion

	// protected float AttackDistance
	// {
	// 	get
	// 	{
	// 		float env = 2.2f;
	// 		if (Target != null && Target.ObjectType == EObjectType.Env)
	// 			return Mathf.Max(env, Collider.radius + Target.Collider.radius + 0.1f);

	// 		float baseValue = CreatureData.AtkRange;
	// 		return baseValue;
	// 	}
	// }
	
        protected void Awake()
        {
            ObjectType = EObjectType.Creature;
			NetLifeState = GetComponent<NetworkLifeState>();
		// SetSpineAnimation(CreatureData.SkeletonDataID, SortingLayers.CREATURE);

            // m_ServerActionPlayer = new ServerActionPlayer(this);
            // NetLifeState = GetComponent<NetworkLifeState>();
            // NetHealthState = GetComponent<NetworkHealthState>();
            // m_State = GetComponent<NetworkAvatarGuidState>();

            // CreatureStatsSO는 SetCreatureGuid에서 찾기 때문에 여기서는 호출하지 않음
            // FindCreatureStatsSO();
            // Init();
        }

	        public override void OnNetworkDespawn()
        {
            // if (IsServer)
            // {
            //     NetLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
            //     m_DamageReceiver.DamageReceived -= ReceiveHP;
            //     m_DamageReceiver.CollisionEntered -= CollisionEntered;
            // }
        }
        public void AddAnimation(int trackIndex, string AnimName, bool loop, float delay)
        {

        }

	protected ECreatureState _creatureState = ECreatureState.None;
	public virtual ECreatureState CreatureState
	{
		get { return _creatureState; }
		set
		{
			if (_creatureState != value)
			{
				_creatureState = value;
				UpdateAnimation();
			}
		}
	}

	public override bool Init()
	{
		ObjectType = EObjectType.Creature;

		return true;
	}

	public virtual void SetInfo(int templateID, Data.CreatureData creatureData)
	{

		DataTemplateID = templateID;

		// if (ObjectType == EObjectType.Hero)
		// 	CreatureData = Managers.Data.HeroDic[templateID];
		// else
		// 	CreatureData = Managers.Data.MonsterDic[templateID];


		MoveSpeed = new CreatureStat(creatureData.MoveSpeed);

		// CreatureData = _dataLoader.MonsterDic[templateID];
		
		// gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

		// Collider 추가
		// Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
		// Collider.radius = CreatureData.ColliderRadius;

		// // RigidBody 추가	
		// RigidBody.mass = 0;

		// Stat - 기존 CreatureStat 객체를 재사용하고 BaseValue만 업데이트
		// Hp = CreatureData.MaxHp;
		// MaxHp = new CreatureStat(CreatureData.MaxHp);
		// MaxHp.SetBaseValue(CreatureData.MaxHp);
		// Atk.SetBaseValue(CreatureData.Atk);
		// AtkRange.SetBaseValue(CreatureData.AtkRange);
		// AtkBonus.SetBaseValue(CreatureData.AtkBonus);
		// MoveSpeed.SetBaseValue(CreatureData.MoveSpeed);
		// CriRate.SetBaseValue(CreatureData.CriRate);
		// CriDamage.SetBaseValue(CreatureData.CriDamage);
		// CreatureData에 IsNpc 속성이 있다면 설정, 없으면 기본값 유지
		// if (CreatureData.GetType().GetProperty("IsNpc") != null)
		// {
		// 	IsNpc = (bool)CreatureData.GetType().GetProperty("IsNpc").GetValue(CreatureData);
		// }
		// // if (CreatureData.GetType().GetProperty("IsValidTarget") != null)
		// // {
		// // 	IsValidTarget = (bool)CreatureData.GetType().GetProperty("IsValidTarget").GetValue(CreatureData);
		// // }

		// // IsValidTarget = LifeState != LifeState.Dead;
		
		// // 스탯 값 확인을 위한 디버그 로그 추가
		// Debug.Log($"[Creature] SetInfo: 스탯 값 설정 완료. " +
		// 		  $"MaxHp.BaseValue={MaxHp.BaseValue}, MaxHp.Value={MaxHp.Value}, " +
		// 		  $"Atk.BaseValue={Atk.BaseValue}, Atk.Value={Atk.Value}, " +
		// 		  $"MoveSpeed.BaseValue={MoveSpeed.BaseValue}, MoveSpeed.Value={MoveSpeed.Value}");
		
		// // 이미 초기화된 스탯은 그대로 유지
		// ReduceDamageRate, LifeStealRate, ThornsDamageRate, AttackSpeedRate는 
		// 이미 생성자에서 초기화되었으므로 여기서 다시 생성할 필요 없음

		// State
		CreatureState = ECreatureState.Idle;
	}

	protected override void UpdateAnimation()
	{
		switch (CreatureState)
		{
			case ECreatureState.Idle:
				// PlayAnimation(0, AnimName.IDLE, true);
				break;
			case ECreatureState.Skill:
				//PlayAnimation(0, AnimName.ATTACK_A, true);
				break;
			case ECreatureState.Move:
				// PlayAnimation(0, AnimName.MOVE, true);
				break;
			case ECreatureState.OnDamaged:
				// PlayAnimation(0, AnimName.IDLE, true);
				// Skills.CurrentSkill.CancelSkill();
				break;
			case ECreatureState.Dead:
				// PlayAnimation(0, AnimName.DEAD, true);
				RigidBody.simulated = false;
				break;
			default:
				break;
		}
	}

	// #region AI
	// public float UpdateAITick { get; protected set; } = 0.0f;

	// protected IEnumerator CoUpdateAI()
	// {
	// 	while (true)
	// 	{
	// 		switch (CreatureState)
	// 		{
	// 			case ECreatureState.Idle:
	// 				UpdateIdle();
	// 				break;
	// 			case ECreatureState.Move:
	// 				UpdateMove();
	// 				break;
	// 			case ECreatureState.Skill:
	// 				UpdateSkill();
	// 				break;
	// 			case ECreatureState.OnDamaged:
	// 				UpdateOnDamaged();
	// 				break;
	// 			case ECreatureState.Dead:
	// 				UpdateDead();
	// 				break;
	// 		}

	// 		if (UpdateAITick > 0)
	// 			yield return new WaitForSeconds(UpdateAITick);
	// 		else
	// 			yield return null;
	// 	}
	// }

	// protected virtual void UpdateIdle() { }
	// protected virtual void UpdateMove() { }
	
	// protected virtual void UpdateSkill() 
	// {
	// 	if (_coWait != null)
	// 		return;

	// 	if (Target.IsValid() == false || Target.ObjectType == EObjectType.HeroCamp)
	// 	{
	// 		CreatureState = ECreatureState.Idle;
	// 		return;
	// 	}

	// 	float distToTargetSqr = DistToTargetSqr;
	// 	float attackDistanceSqr = AttackDistance * AttackDistance;
	// 	if (distToTargetSqr > attackDistanceSqr)
	// 	{
	// 		CreatureState = ECreatureState.Idle;
	// 		return;
	// 	}

	// 	// DoSkill
	// 	Skills.CurrentSkill.DoSkill();

	// 	LookAtTarget(Target);

	// 	var trackEntry = SkeletonAnim.state.GetCurrent(0);
	// 	float delay = trackEntry.Animation.Duration;

	// 	StartWait(delay);
	// }

	// protected virtual void UpdateOnDamaged() { }

	// protected virtual void UpdateDead() { }
	// #endregion

	#region Wait
	protected Coroutine _coWait;

	protected void StartWait(float seconds)
	{
		CancelWait();
		_coWait = StartCoroutine(CoWait(seconds));
	}

	IEnumerator CoWait(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		_coWait = null;
	}

	protected void CancelWait()
	{
		if (_coWait != null)
			StopCoroutine(_coWait);
		_coWait = null;
	}
	#endregion

	// #region Battle
	// public void HandleDotDamage(EffectBase effect)
	// {
	// 	if (effect == null)
	// 		return;
	// 	if (effect.Owner.IsValid() == false)
	// 		return;

	// 	// TEMP
	// 	float damage = (Hp * effect.EffectData.PercentAdd) + effect.EffectData.Amount;
	// 	if (effect.EffectData.ClassName.Contains("Heal"))
	// 		damage *= -1f;

	// 	float finalDamage = Mathf.Round(damage);
	// 	Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp.Value);

	// 	Managers.Object.ShowDamageFont(CenterPosition, finalDamage, transform, false);

	// 	// TODO : OnDamaged 통합
	// 	if (Hp <= 0)
	// 	{
	// 		OnDead(effect.Owner, effect.Skill);
	// 		CreatureState = ECreatureState.Dead;
	// 		return;
	// 	}
	// }

	// public override void OnDamaged(BaseObject attacker, SkillBase skill)
	// {
	// 	base.OnDamaged(attacker, skill);

	// 	if (attacker.IsValid() == false)
	// 		return;

	// 	Creature creature = attacker as Creature;
	// 	if (creature == null)
	// 		return;

	// 	float finalDamage = creature.Atk.Value;
	// 	Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp.Value);

	// 	Managers.Object.ShowDamageFont(CenterPosition, finalDamage, transform, false);

	// 	if (Hp <= 0)
	// 	{
	// 		OnDead(attacker, skill);
	// 		CreatureState = ECreatureState.Dead;
	// 		return;
	// 	}

	// 	// 스킬에 따른 Effect 적용
	// 	if (skill.SkillData.EffectIds != null)
	// 		Effects.GenerateEffects(skill.SkillData.EffectIds.ToArray(), EEffectSpawnType.Skill, skill);

	// 	// AOE
	// 	if (skill != null && skill.SkillData.AoEId != 0)
	// 		skill.GenerateAoE(transform.position);
	// }

	// public override void OnDead(BaseObject attacker, SkillBase skill)
	// {
	// 	base.OnDead(attacker, skill);
	// }

	// protected BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs, Func<BaseObject, bool> func = null)
	// {
	// 	BaseObject target = null;
	// 	float bestDistanceSqr = float.MaxValue;
	// 	float searchDistanceSqr = range * range;

	// 	foreach (BaseObject obj in objs)
	// 	{
	// 		Vector3 dir = obj.transform.position - transform.position;
	// 		float distToTargetSqr = dir.sqrMagnitude;

	// 		// 서치 범위보다 멀리 있으면 스킵.
	// 		if (distToTargetSqr > searchDistanceSqr)
	// 			continue;

	// 		// 이미 더 좋은 후보를 찾았으면 스킵.
	// 		if (distToTargetSqr > bestDistanceSqr)
	// 			continue;

	// 		// 추가 조건
	// 		if (func != null && func.Invoke(obj) == false)
	// 			continue;

	// 		target = obj;
	// 		bestDistanceSqr = distToTargetSqr;
	// 	}

	// 	return target;
	// }

	// protected void ChaseOrAttackTarget(float chaseRange, float attackRange)
	// {
	// 	float distToTargetSqr = DistToTargetSqr;
	// 	float attackDistanceSqr = attackRange * attackRange;

	// 	if (distToTargetSqr <= attackDistanceSqr)
	// 	{
	// 		// 공격 범위 이내로 들어왔다면 공격.
	// 		CreatureState = ECreatureState.Skill;
	// 		//skill.DoSkill();
	// 		return;
	// 	}
	// 	else
	// 	{
	// 		// 공격 범위 밖이라면 추적.
	// 		FindPathAndMoveToCellPos(Target.transform.position, HERO_DEFAULT_MOVE_DEPTH);

	// 		// 너무 멀어지면 포기.
	// 		float searchDistanceSqr = chaseRange * chaseRange;
	// 		if (distToTargetSqr > searchDistanceSqr)
	// 		{
	// 			Target = null;
	// 			CreatureState = ECreatureState.Move;
	// 		}
	// 		return;
	// 	}
	// }
	// #endregion

	// #region Misc
	// protected bool IsValid(BaseObject bo)
	// {
	// 	return bo.IsValid();
	// }
	// #endregion

	// #region Map
	// public EFindPathResult FindPathAndMoveToCellPos(Vector3 destWorldPos, int maxDepth, bool forceMoveCloser = false)
	// {
	// 	Vector3Int destCellPos = Managers.Map.World2Cell(destWorldPos);
	// 	return FindPathAndMoveToCellPos(destCellPos, maxDepth, forceMoveCloser);
	// }

	// public EFindPathResult FindPathAndMoveToCellPos(Vector3Int destCellPos, int maxDepth, bool forceMoveCloser = false)
	// {
	// 	if (LerpCellPosCompleted == false)
	// 		return EFindPathResult.Fail_LerpCell;

	// 	// A*
	// 	List<Vector3Int> path = Managers.Map.FindPath(this, CellPos, destCellPos, maxDepth);
	// 	if (path.Count < 2)
	// 		return EFindPathResult.Fail_NoPath;

	// 	if (forceMoveCloser)
	// 	{
	// 		Vector3Int diff1 = CellPos - destCellPos;
	// 		Vector3Int diff2 = path[1] - destCellPos;
	// 		if (diff1.sqrMagnitude <= diff2.sqrMagnitude)
	// 			return EFindPathResult.Fail_NoPath;
	// 	}

	// 	Vector3Int dirCellPos = path[1] - CellPos;
	// 	//Vector3Int dirCellPos = destCellPos - CellPos;
	// 	Vector3Int nextPos = CellPos + dirCellPos;

	// 	if (Managers.Map.MoveTo(this, nextPos) == false)
	// 		return EFindPathResult.Fail_MoveTo;

	// 	return EFindPathResult.Success;
	// }

	// public bool MoveToCellPos(Vector3Int destCellPos, int maxDepth, bool forceMoveCloser = false)
	// {
	// 	if (LerpCellPosCompleted == false)
	// 		return false;

	// 	return Managers.Map.MoveTo(this, destCellPos);
	// }

	// protected IEnumerator CoLerpToCellPos()
	// {
	// 	while (true)
	// 	{
	// 		Hero hero = this as Hero;
	// 		if (hero != null)
	// 		{
	// 			float div = 5;
	// 			Vector3 campPos = Managers.Object.Camp.Destination.transform.position;
	// 			Vector3Int campCellPos = Managers.Map.World2Cell(campPos);
	// 			float ratio = Math.Max(1, (CellPos - campCellPos).magnitude / div);

	// 			LerpToCellPos(CreatureData.MoveSpeed * ratio);
	// 		}
	// 		else
	// 			LerpToCellPos(CreatureData.MoveSpeed);

	// 		yield return null;
	// 	}
	// }
	// #endregion
}
}