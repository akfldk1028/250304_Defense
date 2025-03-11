using System;
using System.Collections;
// using Unity.Assets.Scripts.ConnectionManagement;
// using Unity.Assets.Scripts.Gameplay.Actions;
// using Unity.Assets.Scripts.Gameplay.Configuration;
using Unity.Netcode;
using UnityEngine;
using Unity.Assets.Scripts.Objects;
// using Action = Unity.Assets.Scripts.Gameplay.Actions.Action;

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    ///  중요!!! 객체의 상태 및 그리고 이제 동작Action 으로 넘어가야함
    /// 네트워크 멀티플레이어 게임에서 몬스터의 서버 측 로직을 담당하는 클래스입니다.
    ///  
    /// ServerCharacter를 상속받아 네트워크 기능을 활용하며, MonsterAvatarSO의 데이터를 사용합니다.
    /// </summary>
    public class ServerMonster : ServerCharacter
    {
        // 몬스터 데이터 참조
        [SerializeField] 
        private MonsterAvatarSO monsterAvatarSO;
        
        private MonsterStatsSO monsterStatsSO;
        
        // 네트워크 변수들
        public NetworkVariable<float> CurrentHp = new NetworkVariable<float>();
        public NetworkVariable<bool> IsAttacking = new NetworkVariable<bool>();
        public NetworkVariable<int> MonsterId = new NetworkVariable<int>();
        
        // 이벤트
        public event Action<ServerMonster> OnMonsterDeath;
        public event Action<ServerMonster, float> OnMonsterDamaged;
        
        private void Awake()
        {
            // 부모 클래스의 Awake() 호출
            base.Awake();
            
            // 추가적인 초기화 작업이 필요하다면 여기에 작성
        }
        
        public override void OnNetworkSpawn()
        {
            // 부모 클래스의 OnNetworkSpawn() 호출
            base.OnNetworkSpawn();
            
            // 네트워크 변수 이벤트 등록
            CurrentHp.OnValueChanged += OnHpChanged;
            
            // 추가적인 네트워크 초기화 작업이 필요하다면 여기에 작성
        }
        
        public override void OnNetworkDespawn()
        {
            // 부모 클래스의 OnNetworkDespawn() 호출
            base.OnNetworkDespawn();
            
            // 네트워크 변수 이벤트 해제
            CurrentHp.OnValueChanged -= OnHpChanged;
            
            // 추가적인 네트워크 정리 작업이 필요하다면 여기에 작성
        }
        
        // 이펙트 생성 요청을 위한 RPC
        [ServerRpc(RequireOwnership = false)]
        public void SpawnEffectServerRpc(Vector3 position, int effectType)
        {
            // 모든 클라이언트에게 이펙트 생성 요청
            SpawnEffectClientRpc(position, effectType);
        }
        
        [ClientRpc]
        private void SpawnEffectClientRpc(Vector3 position, int effectType)
        {
            // 클라이언트에서 이펙트 생성
            GameObject effectPrefab = null;
            
            switch (effectType)
            {
                case 0: // 공격 이펙트
                    effectPrefab = monsterAvatarSO.AttackEffectPrefab;
                    break;
                case 1: // 피격 이펙트
                    effectPrefab = monsterAvatarSO.HitEffectPrefab;
                    break;
                case 2: // 사망 이펙트
                    effectPrefab = monsterAvatarSO.DeathEffectPrefab;
                    break;
                case 3: // 스폰 이펙트
                    effectPrefab = monsterAvatarSO.SpawnEffectPrefab;
                    break;
            }
            
            if (effectPrefab != null)
            {
                Instantiate(effectPrefab, position, Quaternion.identity);
            }
        }
        
        /// <summary>
        /// 몬스터를 초기화합니다.
        /// </summary>
        /// <param name="avatarSO">몬스터 아바타 데이터</param>
        public void Initialize(MonsterAvatarSO avatarSO)
        {
            if (!IsServer)
            {
                Debug.LogError("Initialize는 서버에서만 호출해야 합니다!");
                return;
            }
            
            monsterAvatarSO = avatarSO;
            monsterStatsSO = avatarSO.MonsterData;
            
            if (monsterStatsSO == null)
            {
                Debug.LogError($"Monster {name}의 MonsterData가 null입니다!");
                return;
            }
            
            // 부모 클래스의 creatureGuid 설정
            SetCreatureGuid(monsterStatsSO.Guid);
            
            // 디버그 로그 추가
            Debug.Log($"[ServerMonster] Initialize: 몬스터 {monsterStatsSO.DataId}의 GUID {monsterStatsSO.Guid}로 설정됨");
            
            // 네트워크 변수 초기화
            MonsterId.Value = monsterStatsSO.DataId;
            CurrentHp.Value = monsterStatsSO.MaxHp;
            IsAttacking.Value = false;
            
            // 기본 ServerCharacter 속성 설정
            // CharacterClass = ScriptableObject.CreateInstance<CharacterClass>();
            // CharacterClass.BaseHP.Value = (int)monsterStatsSO.MaxHp;
            // CharacterClass.CharacterType = CharacterTypeEnum.Monster;
            // CharacterClass.IsNpc = true;
            
            // 이름 설정
            gameObject.name = $"Monster_{monsterStatsSO.DataId}";
            

            
            // 몬스터 AI 초기화 (ServerCharacter의 AIBrain 사용)
            // 주: ServerCharacter에 AIBrain이 구현되어 있다고 가정
        }
        
        private void OnHpChanged(float oldValue, float newValue)
        {
            if (newValue <= 0 && oldValue > 0)
            {
                Die();
            }
            else if (newValue < oldValue)
            {
                // 피격 이벤트 발생
                OnMonsterDamaged?.Invoke(this, oldValue - newValue);
            }
        }
        
        /// <summary>
        /// 몬스터에게 데미지를 입힙니다.
        /// </summary>
        /// <param name="damage">입힐 데미지</param>
        public void TakeDamage(float damage)
        {
            if (!IsServer)
            {
                Debug.LogWarning("TakeDamage는 서버에서만 호출해야 합니다!");
                return;
            }
            
            if (CurrentHp.Value <= 0)
                return;
            
            // 데미지 적용
            CurrentHp.Value -= damage;
            
            // 피격 이펙트 생성
            if (monsterAvatarSO != null && monsterAvatarSO.HitEffectPrefab != null)
            {
                SpawnEffectServerRpc(transform.position, 1);
            }
        }
        
        /// <summary>
        /// 몬스터를 사망 처리합니다.
        /// </summary>
        private void Die()
        {
            if (!IsServer)
                return;
            
            // 사망 이펙트 생성
            if (monsterAvatarSO != null && monsterAvatarSO.DeathEffectPrefab != null)
            {
                SpawnEffectServerRpc(transform.position, 2);
            }
            
            // 사망 이벤트 발생
            OnMonsterDeath?.Invoke(this);
            
            // 일정 시간 후 풀로 반환 또는 파괴
            StartCoroutine(DestroyAfterDelay(2.0f));
        }
        
        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // 네트워크 오브젝트 디스폰
            NetworkObject.Despawn();
        }
        

        private IEnumerator ResetAttackState(float delay)
        {
            yield return new WaitForSeconds(delay);
            IsAttacking.Value = false;
        }
        
        /// <summary>
        /// 몬스터가 이동합니다.
        /// </summary>
        /// <param name="direction">이동 방향</param>
        public void Move(Vector2 direction)
        {
            if (!IsServer)
                return;
            
            if (CurrentHp.Value <= 0)
                return;
            
            // 이동 로직
            // ServerCharacter의 Movement 시스템을 사용한다고 가정
            // Movement.SetMovementDirection(direction);
            
            // 방향에 따른 스프라이트 뒤집기 등은 ClientMonster에서 처리
        }
        
        /// <summary>
        /// 몬스터의 최대 체력을 반환합니다.
        /// </summary>
        public float MaxHp => monsterStatsSO != null ? monsterStatsSO.MaxHp : 0;
        
        /// <summary>
        /// 몬스터의 공격력을 반환합니다.
        /// </summary>
        public float Attack => monsterStatsSO != null ? monsterStatsSO.Atk : 0;
        
        /// <summary>
        /// 몬스터의 이동 속도를 반환합니다.
        /// </summary>
        public float MoveSpeed => monsterStatsSO != null ? monsterStatsSO.MoveSpeed : 0;
        
        /// <summary>
        /// 몬스터의 아바타 데이터를 반환합니다.
        /// </summary>
        public MonsterAvatarSO AvatarData => monsterAvatarSO;
        
        /// <summary>
        /// 몬스터의 스탯 데이터를 반환합니다.
        /// </summary>
        public MonsterStatsSO StatsData => monsterStatsSO;
        
        /// <summary>
        /// 몬스터의 스탯 데이터를 가져옵니다.
        /// </summary>
        /// <returns>몬스터의 스탯 데이터</returns>
        public MonsterStatsSO GetMonsterStatsSO()
        {
            // 직접 캐싱된 monsterStatsSO가 있으면 그것을 반환
            if (monsterStatsSO != null)
            {
                return monsterStatsSO;
            }
            
            // 부모 클래스의 GetCreatureStatsSO 메서드를 통해 가져오기 시도
            CreatureStatsSO creatureStatsSO = GetCreatureStatsSO();
            if (creatureStatsSO != null && creatureStatsSO is MonsterStatsSO)
            {
                monsterStatsSO = (MonsterStatsSO)creatureStatsSO;
                return monsterStatsSO;
            }
            
            return null;
        }
    }
}