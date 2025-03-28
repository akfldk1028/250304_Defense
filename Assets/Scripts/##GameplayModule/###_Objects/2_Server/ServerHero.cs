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
        protected HeroData heroData;

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
        private int gachaSpawnWeight;
        [SerializeField]
        private int gachaWeight;
        [SerializeField]
        private int gachaExpCount;
        [SerializeField]
        private int atkSpeed;
        [SerializeField]
        private int atkTime;
       

         public string Rarity => rarity;
         public int GachaSpawnWeight => gachaSpawnWeight;
         public int GachaWeight => gachaWeight;
         public int GachaExpCount => gachaExpCount;
         public int AtkSpeed => atkSpeed;
         public int AtkTime => atkTime;
        public bool NeedArrange { get; set; }

        // 네트워크 변수
        public NetworkVariable<bool> IsAttacking = new NetworkVariable<bool>();
        public NetworkVariable<int> HeroId = new NetworkVariable<int>();
        
        // 이벤트

        public event Action<ServerHero, bool> OnDataLoadComplete;
        
        // 몬스터 데이터
	    public Data.CreatureData CreatureData { get; private set; }

        // 이동 관련
        #endregion

        #region Unity Lifecycle
       private void Awake()
        {
            base.Awake();
            Instance = this;
            CreatureType = CharacterTypeEnum.Hero;
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



            // StartCoroutine(CoUpdateAI());

            return true;
        }

    

        // FixedUpdate 메소드 수정
    
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
       
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        public override void SetInfo(int templateID, Data.CreatureData creatureData)
	    {
        base.SetInfo(templateID , creatureData);
        DataTemplateID = templateID;
        HeroId.Value = templateID;
        CreatureData = creatureData;


        if (CreatureData is HeroData heroData)
        {
            rarity = heroData.Rarity;
            gachaSpawnWeight = heroData.GachaSpawnWeight;
            gachaWeight = heroData.GachaWeight;
            gachaExpCount = heroData.GachaExpCount;
            atkSpeed = heroData.AtkSpeed;
            atkTime = heroData.AtkTime;
        }
        else
        {
         _debugClassFacade?.LogError(GetType().Name, $"[ServerMonster] CreatureData가 MonsterData 타입이 아닙니다! templateID: {templateID}");
        }
		gameObject.name = $"{CreatureData.DataId}_{CreatureData.CharacterType}";
        }


        #endregion

        protected override void UpdateIdle()
        {
            if (_objectManager == null) 
            {
                Debug.LogError("[ServerHero] _objectManager가 null입니다!");

                return; 
            }
            // 0. 이동 상태라면 강제 변경
            if (HeroMoveState == EHeroMoveState.ForceMove)
            {
                CreatureState = ECreatureState.Move;
                return;
            }

            // 1. 몬스터
            //Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
            //if (creature != null)
            //{
            //    Target = creature;
            //    CreatureState = ECreatureState.Move;
            //    HeroMoveState = EHeroMoveState.TargetMonster;
            //    return;
            //}



            //// 3. Camp 주변으로 모이기
            //if (NeedArrange)
            //{
            //    CreatureState = ECreatureState.Move;
            //    HeroMoveState = EHeroMoveState.ReturnToCamp;
            //    return;
            //}
        }




        public bool SetHeroAvatarSO(HeroAvatarSO avatarSO)
        {
            heroAvatarSO = avatarSO;
            Debug.Log($"[ServerHero] heroAvatarSO '{ heroAvatarSO.name}'가 성공적으로 설정되었습니다.");
            return true;
        }

    }
}