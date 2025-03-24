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
    public class ServerMonster : Creature
    {	
        // [Inject] private DataLoader _dataLoader;


        #region Singleton
        private static ServerMonster instance;
        public static ServerMonster Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ServerMonster>();
                    if (instance == null)
                    {
                        Debug.LogError("[ServerMonster] 인스턴스를 찾을 수 없습니다!");
                    }
                }
                return instance;
            }
            set => instance = value;
        }
        #endregion

        #region Fields
        [Header("===== 몬스터 설정 =====")]
        [Space(10)]
        [SerializeField] 
        private MonsterAvatarSO monsterAvatarSO;

        [Header("===== 몬스터 특화 속성 (읽기 전용) =====")]
        [Space(5)]
        [SerializeField]
        private int dropItemId;

        // 네트워크 변수
        public NetworkVariable<float> CurrentHp = new NetworkVariable<float>();
        public NetworkVariable<bool> IsAttacking = new NetworkVariable<bool>();
        public NetworkVariable<int> MonsterId = new NetworkVariable<int>();
        
        // 이벤트
        public event Action<ServerMonster> OnMonsterDeath;
        public event Action<ServerMonster, float> OnMonsterDamaged;
        public event Action<ServerMonster, bool> OnDataLoadComplete;
        
        // 몬스터 데이터
        protected MonsterData monsterData;
        public int DropItemId => dropItemId;
	    public Data.CreatureData CreatureData { get; private set; }

        // 이동 관련
        private int target_Value = 0;
        private List<Vector2> _moveList = new List<Vector2>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            base.Awake();
            Instance = this;
            CreatureType = CharacterTypeEnum.Monster;
        }

        // private void Update()
        // {
        //     if (IsServer)
        //     {
        //         HandleMovement();
        //     }
        // }
        public NetworkVariable<Vector3> NetworkPosition = new NetworkVariable<Vector3>();
        private bool positionInitialized = false;

        // FixedUpdate 메소드 수정
        private void FixedUpdate()
        {
            if (IsServer)
            {
                HandleServerMovement();
            }
            else if (IsClient)
            {
                HandleClientMovement();
            }
        }

        private void HandleServerMovement()
        {
            if (_moveList.Count == 0 || target_Value >= _moveList.Count) return;

            transform.position = Vector2.MoveTowards(transform.position, _moveList[target_Value], Time.deltaTime * MoveSpeed.Value);
            NetworkPosition.Value = transform.position; // 위치 업데이트

            if (Vector2.Distance(transform.position, _moveList[target_Value]) <= 0.0f)
            {
                target_Value++;
                if (target_Value >= 4) // 하드코딩된 4 사용
                {
                    target_Value = 0;
                }
            }
        }

        private void HandleClientMovement()
        {
            if (!positionInitialized && NetworkPosition.Value != Vector3.zero)
            {
                positionInitialized = true;
            }

            if (positionInitialized)
            {
                // 위치 강제 설정 (보간 없이)
                transform.position = NetworkPosition.Value;
            }
}
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                CurrentHp.OnValueChanged += OnHpChanged;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            CurrentHp.OnValueChanged -= OnHpChanged;
        }

        public override void SetInfo(int templateID, Data.CreatureData creatureData)
	    {
        base.SetInfo(templateID , creatureData);

		DataTemplateID = templateID;
        MonsterId.Value = templateID;
        CreatureData = creatureData;
        // CreatureData = DataLoader.instance.MonsterDic[templateID];


		
		gameObject.name = $"{CreatureData.DataId}_{CreatureData.CharacterType}";
        }

        
        #endregion

        #region Initialization
    
        #endregion

        #region Movement
        private void HandleMovement()
        {
            // ClientMonster처럼 간단한 체크
            if (_moveList.Count == 0 || target_Value >= _moveList.Count) return;

            // ClientMonster와 유사한 이동 방식 사용
            transform.position = Vector2.MoveTowards(transform.position, _moveList[target_Value], Time.deltaTime * MoveSpeed.Value);
            
            if (Vector2.Distance(transform.position, _moveList[target_Value]) <= 0.0f)
            {
                target_Value++;
                
                // ClientMonster와 유사한 스프라이트 flip 처리
                // SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                // if (spriteRenderer != null)
                // {
                //     spriteRenderer.flipX = target_Value >= 3;
                // }
                
                // ClientMonster와 유사한 경로 순환 처리 (하드코딩된 4 사용)
                if (target_Value >= 4)
                {
                    target_Value = 0;
                }
            }
        }

        public void SetMoveList(List<Vector2> moveList)
        {
            if (moveList == null || moveList.Count == 0)
            {
                Debug.LogError($"[ServerMonster:{MonsterId.Value}] SetMoveList: 유효하지 않은 경로 데이터");
                return;
            }
            
            // ClientMonster처럼 직접 할당 (깊은 복사 없이)
            _moveList = moveList;
            
            // 항상 첫 번째 포인트부터 시작
            target_Value = 0;
            
            Debug.Log($"[ServerMonster:{MonsterId.Value}] 새 경로 설정 완료: 포인트 수={_moveList.Count}, 시작 인덱스={target_Value}");
        }
                #endregion

     

        // private void SetupMonsterData()
        // {
        //     dropItemId = monsterStatsSO.DropItemId;
        //     MonsterId.Value = monsterStatsSO.DataId;

        //     if (Application.isPlaying)
        //     {
        //         SetInfo(monsterStatsSO.DataId);
        //         CurrentHp.Value = monsterStatsSO.MaxHp;
        //     }

        //     Debug.Log($"[ServerMonster] 몬스터 데이터 로드 완료: ID={MonsterId.Value}, MaxHP={monsterStatsSO.MaxHp}, ATK={monsterStatsSO.Atk}");
        //     OnDataLoadComplete?.Invoke(this, true);
        // }

 

        #region Combat
        public void TakeDamage(float damage)
        {
            if (!IsServer)
            {
                Debug.LogWarning("TakeDamage는 서버에서만 호출해야 합니다!");
                return;
            }
            
            if (CurrentHp.Value <= 0) return;
            
            CurrentHp.Value -= damage;
            Hp = CurrentHp.Value;
        }

        private void OnHpChanged(float oldValue, float newValue)
        {
            Hp = newValue;
            
            if (newValue <= 0 && oldValue > 0)
            {
                OnMonsterDeath?.Invoke(this);
            }
            else if (newValue < oldValue)
            {
                OnMonsterDamaged?.Invoke(this, oldValue - newValue);
            }
        }
        #endregion

        #region Setup Methods
        public bool SetMonsterAvatarSO(MonsterAvatarSO avatarSO)
        {
            if (avatarSO == null)
            {
                Debug.LogError("[ServerMonster] 설정하려는 MonsterAvatarSO가 null입니다!");
                OnDataLoadComplete?.Invoke(this, false);
                return false;
            }

            monsterAvatarSO = avatarSO;

     
            Debug.Log($"[ServerMonster] MonsterAvatarSO '{monsterAvatarSO.name}'가 성공적으로 설정되었습니다.");
            return true;
        }

     

        // public static bool SetupServerMonsterInPrefab(GameObject prefab, int monsterId, MonsterAvatarList monsterAvatarList)
        // {
        //     if (!ValidateSetupParameters(prefab, monsterId, monsterAvatarList)) return false;

        //     ServerMonster serverMonster = GetServerMonsterComponent(prefab);
        //     if (serverMonster == null) return false;

        //     MonsterAvatarSO avatarSO = monsterAvatarList.GetAvatarById(monsterId);
        //     if (avatarSO == null)
        //     {
        //         Debug.LogError($"[ServerMonster] ID {monsterId}에 해당하는 몬스터 아바타를 찾을 수 없습니다!");
        //         return false;
        //     }

        //     serverMonster.monsterAvatarSO = avatarSO;
        //     serverMonster.LoadMonsterDataFromAvatarSO();
            
        //     Debug.Log($"[ServerMonster] 프리팹 '{prefab.name}'의 ServerMonster 컴포넌트에 몬스터 ID {monsterId}와 아바타 '{avatarSO.name}'을(를) 성공적으로 할당했습니다.");
        //     return true;
        // }

        // private static bool ValidateSetupParameters(GameObject prefab, int monsterId, MonsterAvatarList monsterAvatarList)
        // {
        //     if (prefab == null)
        //     {
        //         Debug.LogError("[ServerMonster] 설정할 프리팹이 null입니다!");
        //         return false;
        //     }
            
        //     if (monsterId <= 0)
        //     {
        //         Debug.LogError($"[ServerMonster] 유효하지 않은 몬스터 ID: {monsterId}");
        //         return false;
        //     }
            
        //     if (monsterAvatarList == null)
        //     {
        //         Debug.LogError("[ServerMonster] monsterAvatarList가 null입니다!");
        //         return false;
        //     }

        //     return true;
        // }

   
        #endregion

        #region Editor Methods
        private void OnValidate()
        {
            if (monsterAvatarSO == null) return;

            if (Application.isPlaying)
            {
            }
            else
            {
                // UpdateEditorValues();
            }
        }

        // private void UpdateEditorValues()
        // {
        //     monsterStatsSO = monsterAvatarSO.MonsterData;
        //     if (monsterStatsSO == null) return;

        //     dropItemId = monsterStatsSO.DropItemId;
            
        //     MaxHp.SetBaseValue(monsterStatsSO.MaxHp);
        //     Atk.SetBaseValue(monsterStatsSO.Atk);
        //     AtkRange.SetBaseValue(monsterStatsSO.AtkRange);
        //     AtkBonus.SetBaseValue(monsterStatsSO.AtkBonus);
        //     CriRate.SetBaseValue(monsterStatsSO.CriRate);
        //     CriDamage.SetBaseValue(monsterStatsSO.CriDamage);
        //     MoveSpeed.SetBaseValue(monsterStatsSO.MoveSpeed);
            
        //     Hp = monsterStatsSO.MaxHp;
            
        //     Debug.Log($"[ServerMonster] 에디터 모드: monsterStatsSO가 업데이트되었습니다. ID={monsterStatsSO.DataId}");
        // }

        [ContextMenu("디버그 상태 출력")]
        public void DebugPrintState()
        {
            if (!ValidateDebugState()) return;
            
            LogCreatureInfo();
            LogMonsterSpecificInfo();
            LogNetworkInfo();
        }

        private bool ValidateDebugState()
        {
            if (monsterAvatarSO == null)
            {
                Debug.Log("[ServerMonster] 디버그: monsterAvatarSO가 설정되지 않았습니다.");
                return false;
            }

            return true;
        }

        private void LogCreatureInfo()
        {
            // string creatureInfo = $"[ServerMonster] 기본 속성 (Creature 클래스):\n" +
            //                     $"- ID: {DataTemplateID}\n" +
            //                     $"- HP: {Hp}\n" +
            //                     $"- 최대 HP: {monsterStatsSO.MaxHp}\n" +
            //                     $"- 공격력: {monsterStatsSO.Atk}\n" +
            //                     $"- 공격 범위: {monsterStatsSO.AtkRange}\n" +
            //                     $"- 공격 보너스: {monsterStatsSO.AtkBonus}\n" +
            //                     $"- 이동 속도: {monsterStatsSO.MoveSpeed}\n" +
            //                     $"- 치명타 확률: {monsterStatsSO.CriRate}\n" +
            //                     $"- 치명타 데미지: {monsterStatsSO.CriDamage}\n" +
            //                     $"- 생물체 타입: {CreatureType}";
            
            // Debug.Log(creatureInfo);
        }

        private void LogMonsterSpecificInfo()
        {
            Debug.Log($"[ServerMonster] 몬스터 특화 속성:\n" +
                     $"- 드롭 아이템 ID: {dropItemId}\n" +
                     $"- 현재 공격 중: {(Application.isPlaying ? IsAttacking.Value : false)}");
        }

        private void LogNetworkInfo()
        {
            if (!Application.isPlaying) return;
            
            Debug.Log($"[ServerMonster] 네트워크 변수:\n" +
                     $"- 네트워크 HP: {CurrentHp.Value}\n" +
                     $"- 네트워크 몬스터 ID: {MonsterId.Value}\n" +
                     $"- 네트워크 공격 중: {IsAttacking.Value}");
        }
        #endregion
    }
}