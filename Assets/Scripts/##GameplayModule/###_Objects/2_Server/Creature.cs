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

	public virtual void SetInfo(int templateID, Data.CreatureData creatureData)
	{

		DataTemplateID = templateID;
		MoveSpeed = new CreatureStat(creatureData.MoveSpeed);

		

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

	protected virtual void OnCreatureStateChanged(ECreatureState newState)
	{
		// Client로 대충 이동함함
		// 서버에서 상태 변경 시 필요한 로직
	}

	protected virtual void UpdateAnimation(){}
	

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


        protected BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs, Func<BaseObject, bool> func = null)
        {
            BaseObject target = null;
            float bestDistanceSqr = float.MaxValue;
            float searchDistanceSqr = range * range;

            foreach (BaseObject obj in objs)
            {
                Vector3 dir = obj.transform.position - transform.position;
                float distToTargetSqr = dir.sqrMagnitude;

                // 서치 범위보다 멀리 있으면 스킵.
                if (distToTargetSqr > searchDistanceSqr)
                    continue;

                // 이미 더 좋은 후보를 찾았으면 스킵.
                if (distToTargetSqr > bestDistanceSqr)
                    continue;

                // 추가 조건
                if (func != null && func.Invoke(obj) == false)
                    continue;

                target = obj;
                bestDistanceSqr = distToTargetSqr;
            }

            return target;
        }


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