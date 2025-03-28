using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;
using VContainer;
using Unity.Assets.Scripts.Data;
using Unity.Netcode;

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

    public CreatureStat LifeStealRate;
    public CreatureStat ThornsDamageRate; // 쏜즈
    public CreatureStat AttackSpeedRate;



    public NetworkLifeState NetLifeState { get; private set; }

 	//ECreatureState 와 LifeState 통합해야함
	public LifeState LifeState
        {
            get => NetLifeState.LifeState.Value;
            private set => NetLifeState.LifeState.Value = value;
        }

	protected NetworkVariable<ECreatureState> _creatureState = new NetworkVariable<ECreatureState>(ECreatureState.None);

	public NetworkVariable<ECreatureState> NetworkCreatureState => _creatureState;

	public virtual ECreatureState CreatureState
	{
		get { return _creatureState.Value; }
		set
		{
			if (_creatureState.Value != value)
			{
				_creatureState.Value = value;
				if (IsServer)
				{
					// 서버에서는 상태 변경만 처리
					OnCreatureStateChanged(_creatureState.Value);
				}
			}
		}
	}

        #endregion


#if UNITY_EDITOR
        [Header("===== 디버그 정보 =====")]
        [Space(5)]
        [SerializeField] private float _dbgMaxHp;
        [SerializeField] private float _dbgAtk;
        [SerializeField] private float _dbgAtkRange;
        [SerializeField] private float _dbgAtkBonus;
        [SerializeField] private float _dbgMoveSpeed;
        [SerializeField] private float _dbgCriRate;
        [SerializeField] private float _dbgCriDamage;
#endif
        public virtual void Update()
        {
#if UNITY_EDITOR
            // 런타임에만 디버그 정보 업데이트
            if (Application.isPlaying)
            {
                _dbgMaxHp = MaxHp.Value;
                _dbgAtk = Atk.Value;
                _dbgAtkRange = AtkRange.Value;
                _dbgAtkBonus = AtkBonus.Value;
                _dbgMoveSpeed = MoveSpeed.Value;
                _dbgCriRate = CriRate.Value;
                _dbgCriDamage = CriDamage.Value;
            }
#endif

            // 기존 Update 코드 (있다면)
        }

        protected void Awake()
        {
            ObjectType = EObjectType.Creature;
			NetLifeState = GetComponent<NetworkLifeState>();
	
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
     


	public override bool Init()
	{
		ObjectType = EObjectType.Creature;

		return true;
	}

    public virtual void SetInfo<T>(int templateID, Data.CreatureData creatureData, T clientCreature) 
    where T : class    {
		DataTemplateID = templateID;
        Hp = creatureData.MaxHp;
        MaxHp = new CreatureStat(creatureData.MaxHp);
        Atk = new CreatureStat(creatureData.Atk);
        CriRate = new CreatureStat(creatureData.CriRate);
        CriDamage = new CreatureStat(creatureData.CriDamage);
        ReduceDamageRate = new CreatureStat(0);
        LifeStealRate = new CreatureStat(0);
        ThornsDamageRate = new CreatureStat(0);
        MoveSpeed = new CreatureStat(creatureData.MoveSpeed);
        AttackSpeedRate = new CreatureStat(1);
        AtkRange = new CreatureStat(creatureData.AtkRange);
        AtkBonus = new CreatureStat(creatureData.AtkBonus);
        CriRate = new CreatureStat(creatureData.CriRate);


        
        CreatureState = ECreatureState.Idle;


            // Collider 추가
            // Collider.offset = new Vector2(CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
            // Collider.radius = CreatureData.ColliderRadius;

            // // RigidBody 추가	
            // RigidBody.mass = 0;


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


            //// Effect
            //Effects = gameObject.AddComponent<EffectComponent>();
            //Effects.SetInfo(this);

            //// Map
            //StartCoroutine(CoLerpToCellPos());
        }

    protected virtual void OnCreatureStateChanged(ECreatureState newState)
	{
		// Client로 대충 이동함함
		// 서버에서 상태 변경 시 필요한 로직
	}
    protected virtual void UpdateAnimation(){}


	float DistToTargetSqr
	{
		get
		{
			Vector3 dir = (Target.transform.position - transform.position);
			float distToTarget = Math.Max(0, dir.magnitude - Target.ExtraCells * 1f - ExtraCells * 1f); // TEMP
			return distToTarget * distToTarget;
		}
	}


    protected void ChaseOrAttackTarget(float chaseRange, float attackRange)
	{
		float distToTargetSqr = DistToTargetSqr;
		float attackDistanceSqr = attackRange * attackRange;

		if (distToTargetSqr <= attackDistanceSqr)
		{
			// 공격 범위 이내로 들어왔다면 공격.
			CreatureState = ECreatureState.Skill;
			//skill.DoSkill();
			return;
		}
		else
		{
			Target = null;
			CreatureState = ECreatureState.Move;
		}
	}

   public float UpdateAITick { get; protected set; } = 0.0f;
   protected IEnumerator CoUpdateAI()
        {
            while (true)
            {
                switch (CreatureState)
                {
                    case ECreatureState.Idle:
                        UpdateIdle();
                        break;
                    case ECreatureState.Move:
                        UpdateMove();
                        break;
                    case ECreatureState.Skill:
                        UpdateSkill();
                        break;
                    case ECreatureState.OnDamaged:
                        //UpdateOnDamaged();
                        break;
                    case ECreatureState.Dead:
                        //UpdateDead();
                        break;
                }

                if (UpdateAITick > 0)
                    yield return new WaitForSeconds(UpdateAITick);
                else
                    yield return null;
            }
        }



        // protected BaseObject FindClosestInRange(Vector3 centerPosition, float range, IEnumerable<BaseObject> objs, Func<BaseObject, bool> func = null)
        // {
        //     BaseObject target = null;
        //     float bestDistanceSqr = float.MaxValue;
        //     float searchDistanceSqr = range * range;

        //     foreach (BaseObject obj in objs)
        //     {
        //         Vector3 dir = obj.transform.position - centerPosition;
        //         float distToTargetSqr = dir.sqrMagnitude;

        //         if (distToTargetSqr > searchDistanceSqr)
        //             continue;

        //         if (distToTargetSqr > bestDistanceSqr)
        //             continue;

        //         if (func != null && func.Invoke(obj) == false)
        //             continue;

        //         target = obj;
        //         bestDistanceSqr = distToTargetSqr;
        //     }

        //     return target;
        // }

        protected virtual void UpdateIdle() { }
   		protected virtual void UpdateMove() { }

        protected virtual void UpdateSkill()
        {
            //if (_coWait != null)
            //    return;

            //if (Target.IsValid() == false || Target.ObjectType == EObjectType.HeroCamp)
            //{
            //    CreatureState = ECreatureState.Idle;
            //    return;
            //}

            //float distToTargetSqr = DistToTargetSqr;
            //float attackDistanceSqr = AttackDistance * AttackDistance;
            //if (distToTargetSqr > attackDistanceSqr)
            //{
            //    CreatureState = ECreatureState.Idle;
            //    return;
            //}

            //// DoSkill
            //Skills.CurrentSkill.DoSkill();

            //LookAtTarget(Target);

            //var trackEntry = SkeletonAnim.state.GetCurrent(0);
            //float delay = trackEntry.Animation.Duration;

            //StartWait(delay);
        }



    }
}