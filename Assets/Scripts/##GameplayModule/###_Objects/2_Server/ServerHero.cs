using System;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Unity.Assets.Scripts.Objects;
using VContainer;
using Unity.Assets.Scripts.Infrastructure;
using UnityEngine.Events;
using Unity.Assets.Scripts.Data;
using static Define;
using System.Collections.Generic;
using Unity.Assets.Scripts.Objects;
using UnityEditor.PackageManager;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// 네트워크 멀티플레이어 게임에서 몬스터의 서버 측 로직을 담당하는 클래스입니다.
    /// Creature를 상속받아 네트워크 기능을 활용하며, MonsterAvatarSO의 데이터를 사용합니다.
    /// </summary>
    public class ServerHero : Creature
    {	
        // [Inject] private DataLoader _dataLoader;
        [Inject] private DebugClassFacade _debugClassFacade;

        [Inject] public ObjectManager _objectManager;


        protected HeroData heroData;

        protected ClientHero clientHero;

        #region Singleton
        private static ServerHero instance;
        public static ServerHero Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ServerHero>();
                    if (instance == null)
                    {
                        Debug.LogError("[ServerHero] 인스턴스를 찾을 수 없습니다!");
                    }
                }
                return instance;
            }
            set => instance = value;
        }
        #endregion

        #region Fields
        [Header("===== 영웅 설정 =====")]
        [Space(10)]
        [SerializeField] 
        private HeroAvatarSO heroAvatarSO;
        [SerializeField]
        private string rarity;
        [SerializeField]
        public CreatureStat gachaSpawnWeight = new CreatureStat(0);
        [SerializeField]
        public CreatureStat gachaWeight = new CreatureStat(0);
        [SerializeField]
        public CreatureStat gachaExpCount = new CreatureStat(0);
        [SerializeField]
        public CreatureStat atkSpeed = new CreatureStat(0);
        [SerializeField]
        public CreatureStat atkTime = new CreatureStat(0);



        public bool NeedArrange { get; set; }

        // 네트워크 변수
        public NetworkVariable<bool> IsAttacking = new NetworkVariable<bool>();
        public NetworkVariable<int> HeroId = new NetworkVariable<int>();
  
        public event Action<ServerHero, bool> OnDataLoadComplete;
  
	    public Data.CreatureData CreatureData { get; private set; }
        private UI_Spawn_Holder parent_holder;

        #endregion

        // Fields 리전 내에 추가
#if UNITY_EDITOR
        [Header("===== 영웅 디버그 정보 =====")]
        [Space(5)]
        [SerializeField] private float _dbgGachaSpawnWeight;
        [SerializeField] private float _dbgGachaWeight;
        [SerializeField] private float _dbgGachaExpCount;
        [SerializeField] private float _dbgAtkSpeed;
        [SerializeField] private float _dbgAtkTime;
#endif

        // Update 메서드 추가 또는 기존 메서드에 추가
        public override void Update()
        {
            base.Update(); // 부모 클래스의 Update 호출 (있다면)

#if UNITY_EDITOR
            // 런타임에만 디버그 정보 업데이트
            if (Application.isPlaying)
            {
                _dbgGachaSpawnWeight = gachaSpawnWeight.Value;
                _dbgGachaWeight = gachaWeight.Value;
                _dbgGachaExpCount = gachaExpCount.Value;
                _dbgAtkSpeed = atkSpeed.Value;
                _dbgAtkTime = atkTime.Value;
            }
#endif
        }


        #region Unity Lifecycle
            private void Start()
            {
  
            }

        private void Awake()
        {
            base.Awake();
            Instance = this;
            CreatureType = CharacterTypeEnum.Hero;
  
        }

        public void SetParentHolder(UI_Spawn_Holder holder)
        {
            parent_holder = holder;

        }
      

        EHeroMoveState _heroMoveState = EHeroMoveState.None;
        public EHeroMoveState HeroMoveState
        {
            get { return _heroMoveState; }
            private set
            {
                _heroMoveState = value;
                switch (value)
                {

                    case EHeroMoveState.TargetMonster:
                        NeedArrange = true;
                        break;
                    case EHeroMoveState.ForceMove:
                        NeedArrange = true;
                        break;
                }
            }
        }


        public override bool Init()
	    {
            if (base.Init() == false)
                return false;
            ObjectType = EObjectType.Hero;
            return true;
        }



        // FixedUpdate 메소드 수정

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // ObjectManagerFacade를 통해 ObjectManager 참조 가져오기
            if (_objectManager == null)
            {
                var facade = FindAnyObjectByType<ObjectManagerFacade>();
                if (facade != null)
                {
                        _objectManager = facade._objectManager; // 참고: Manager는 ObjectManagerFacade에 있는 속성이어야 합니다
                        Debug.Log("[ServerHero] ObjectManagerFacade를 통해 ObjectManager를 찾았습니다.");
                    }
                    else
                    {
                        Debug.LogError("[ServerHero] ObjectManagerFacade를 찾을 수 없습니다!");
                    }
            }
            StartCoroutine(CoUpdateAI());
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        public void SetClientHero(ClientHero _clientHero)
        {
           clientHero = _clientHero;
        }

        public override void SetInfo<T>(int templateID, Data.CreatureData creatureData, T clientCreature) 
        where T : class	    {
        base.SetInfo(templateID , creatureData, clientCreature);
        DataTemplateID = templateID;
        HeroId.Value = templateID;
        CreatureData = creatureData;


        if (CreatureData is HeroData heroData)
        {
            rarity = heroData.Rarity;
            gachaSpawnWeight = new CreatureStat(heroData.GachaSpawnWeight);
            gachaWeight = new CreatureStat(heroData.GachaWeight);
            gachaExpCount = new CreatureStat(heroData.GachaExpCount);
            atkSpeed = new CreatureStat(heroData.AtkSpeed);
            atkTime = new CreatureStat(heroData.AtkTime);
        }
        else
        {
         _debugClassFacade?.LogError(GetType().Name, $"[ServerMonster] CreatureData가 MonsterData 타입이 아닙니다! templateID: {templateID}");
        }
		gameObject.name = $"{CreatureData.DataId}_{CreatureData.CharacterType}";
        }


        #endregion
        public NetworkObject target;
        float atkSpeed_Coroutine_Value = 1.0f;
        private float attackTimer = 0f;

        protected override void UpdateIdle()
        {
        if (parent_holder != null)
        {
            // 공격 타이머 업데이트
            attackTimer += Time.deltaTime * (atkSpeed.Value * atkSpeed_Coroutine_Value);
            BaseObject creatureEnemy = FindNearestEnemy();
            
            if (creatureEnemy != null)
            {
                Debug.Log($"[ServerHero] 적 감지: {creatureEnemy.name}");
                
                target = creatureEnemy.GetComponent<NetworkObject>();
                Target = creatureEnemy;
                //일반 어택이 있고 시간이 되면 skill 이되야할거같은데데
                CreatureState = ECreatureState.Move;
//                 |Client-1|ServerHero(Clone)|_creatureState| Write permissions (Server) for this client instance is not allowed!
// UnityEngine.Debug:LogError (object)
// Unity.Netcode.NetworkVariableBase:LogWritePermissionError () (at ./Library/PackageCache/com.unity.netcode.gameobjects/Runtime/NetworkVariable/NetworkVariableBase.cs:45)
// Unity.Netcode.NetworkVariable`1<ECreatureState>:set_Value (ECreatureState) (at ./Library/PackageCache/com.unity.netcode.gameobjects/Runtime/NetworkVariable/NetworkVariable.cs:117)
// Unity.Assets.Scripts.Objects.Creature:set_CreatureState (ECreatureState) (at Assets/Scripts/##GameplayModule/###_Objects/2_Server/Creature.cs:103)
// Unity.Assets.Scripts.Objects.ServerHero:UpdateIdle () (at Assets/Scripts/##GameplayModule/###_Objects/2_Server/ServerHero.cs:252)
// Unity.Assets.Scripts.Objects.Creature/<CoUpdateAI>d__65:MoveNext () (at Assets/Scripts/##GameplayModule/###_Objects/2_Server/Creature.cs:270)
// UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)
                HeroMoveState = EHeroMoveState.TargetMonster;
                
                if (attackTimer >= atkTime.Value)
                {
                    attackTimer = 0.0f;
        //            GetBullet();
        //            //AttackMonsterServerRpc(target.NetworkObjectId);
                }
                return;
            }
            else
            {
                target = null;
            }
        }
    }

        //     // 1. 몬스터
        //     //Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
        //     //if (creature != null)
        //     //{
        //     //    Target = creature;
        //     //    CreatureState = ECreatureState.Move;
        //     //    HeroMoveState = EHeroMoveState.TargetMonster;
        //     //    return;
        //     //}



        //     //// 3. Camp 주변으로 모이기
        //     //if (NeedArrange)
        //     //{
        //     //    CreatureState = ECreatureState.Move;
        //     //    HeroMoveState = EHeroMoveState.ReturnToCamp;
        //     //    return;
        //     //}
        // }

	protected override void UpdateMove() 
	{


		// 1. 주변 몬스터 서치
		if (HeroMoveState == EHeroMoveState.TargetMonster)
		{
			// 몬스터 죽었으면 포기.
			if (Target.IsValid() == false)
			{
				HeroMoveState = EHeroMoveState.None;
				CreatureState = ECreatureState.Idle;
				return;
			}

			ChaseOrAttackTarget(8.0f, 1.0f);
			return;
		}
	}


    private BaseObject FindNearestEnemy()
    {
        if (parent_holder == null) return null;
        
        // 몬스터 레이어 마스크 생성 (레이어 번호 8 = "Monster" 가정)
        int monsterLayerMask = LayerNames.Monster;
        
        // 범위 내 모든 적 콜라이더 찾기
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(
            parent_holder.transform.position, 
            AtkRange.Value, 
            monsterLayerMask
        );
        
        Debug.Log($"[ServerHero] 범위 내 적 수: {enemiesInRange.Length}, 범위: {AtkRange.Value}, 위치: {parent_holder.transform.position}");
        
        // 가장 가까운 적 찾기
        BaseObject nearestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider2D enemyCollider in enemiesInRange)
        {
            float distance = Vector2.Distance(parent_holder.transform.position, enemyCollider.transform.position);
            Debug.Log($"[ServerHero] 적 발견: {enemyCollider.name}, 거리: {distance}");
            
            if (distance < closestDistance)
            {
                BaseObject enemy = enemyCollider.GetComponent<BaseObject>();
                if (enemy != null)
                {
                    closestDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }
        
        return nearestEnemy;
    }









        //void CheckForEnemies()
        //{
        //    Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(parent_holder.transform.position, attackRange, enemyLayer);

        //    attackTime += Time.deltaTime * (attackSpeed * atkSpeed_Coroutine_Value);

        //    if (enemiesInRange.Length > 0)
        //    {
        //        target = enemiesInRange[0].GetComponent<NetworkObject>();
        //        if (attackTime >= 1.0f)
        //        {
        //            attackTime = 0.0f;
        //            AnimatorChange("ATTACK", true);
        //            animator.speed = attackSpeed * atkSpeed_Coroutine_Value;
        //            GetBullet();
        //            //AttackMonsterServerRpc(target.NetworkObjectId);

        //        }

        //    }
        //    else
        //    {
        //        target = null;
        //    }
        //}

        public bool SetHeroAvatarSO(HeroAvatarSO avatarSO)
        {
            heroAvatarSO = avatarSO;
            Debug.Log($"[ServerHero] heroAvatarSO '{ heroAvatarSO.name}'가 성공적으로 설정되었습니다.");
            return true;
        }

    }
}