// using Unity.Assets.Scripts.Gameplay.GameplayObjects.Character;
using Unity.Netcode;
using UnityEngine;

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// 네트워크 멀티플레이어 게임에서 몬스터의 클라이언트 측 시각화를 담당하는 클래스입니다.
    /// ClientCharacter를 상속받아 네트워크 기능을 활용합니다.
    /// </summary>
    public class ClientMonster : ClientCharacter
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AudioSource audioSource;
        
        [Header("사운드 효과")]
        [SerializeField] private AudioClip spawnSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip deathSound;
        
        // 서버 몬스터 참조
        private ServerMonster m_ServerMonster;
        
        // 이전 방향 저장 (스프라이트 뒤집기용)
        private Vector2 m_PrevDirection = Vector2.right;
        
        private void Awake()
        {
            // 컴포넌트 참조 초기화
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            
            // 서버 몬스터 참조 가져오기
            m_ServerMonster = GetComponent<ServerMonster>();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // 네트워크 변수 변경 감지
            m_ServerMonster.CurrentHp.OnValueChanged += OnHpChanged;
            m_ServerMonster.IsAttacking.OnValueChanged += OnAttackStateChanged;
            
            // 스폰 시 사운드 재생
            PlaySound(spawnSound);
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            // 네트워크 변수 변경 감지 해제
            m_ServerMonster.CurrentHp.OnValueChanged -= OnHpChanged;
            m_ServerMonster.IsAttacking.OnValueChanged -= OnAttackStateChanged;
        }
        
        private void OnHpChanged(float oldValue, float newValue)
        {
            // HP 변경 시 시각적 효과
            if (newValue < oldValue)
            {
                // 피격 애니메이션
                if (animator != null)
                {
                    animator.SetTrigger("Hit");
                }
                
                // 피격 사운드
                PlaySound(hitSound);
            }
            
            // 사망 처리
            if (newValue <= 0 && oldValue > 0)
            {
                // 사망 애니메이션
                if (animator != null)
                {
                    animator.SetBool("IsDead", true);
                    animator.SetTrigger("Die");
                }
                
                // 사망 사운드
                PlaySound(deathSound);
                
                // 콜라이더 비활성화 등 추가 처리
                var collider = GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
        
        private void OnAttackStateChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                // 공격 애니메이션
                if (animator != null)
                {
                    animator.SetTrigger("Attack");
                }
                
                // 공격 사운드
                PlaySound(attackSound);
            }
        }
        
        /// <summary>
        /// 이동 방향에 따라 애니메이션과 스프라이트 방향을 업데이트합니다.
        /// </summary>
        /// <param name="direction">이동 방향</param>
        public void UpdateMovementVisuals(Vector2 direction)
        {
            if (direction.magnitude > 0.1f)
            {
                m_PrevDirection = direction;
            }
            
            // 이동 애니메이션
            if (animator != null)
            {
                animator.SetBool("IsMoving", direction.magnitude > 0.1f);
            }
            
            // 방향에 따른 스프라이트 뒤집기
            if (spriteRenderer != null && m_PrevDirection.x != 0)
            {
                spriteRenderer.flipX = m_PrevDirection.x < 0;
            }
        }
        
        /// <summary>
        /// 사운드를 재생합니다.
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
        
        /// <summary>
        /// 몬스터의 시각적 요소를 초기화합니다.
        /// </summary>
        /// <param name="monsterAvatarSO">몬스터 아바타 데이터</param>
        public void InitializeVisuals(MonsterAvatarSO monsterAvatarSO)
        {
            if (monsterAvatarSO == null)
                return;
            
            // 스프라이트 설정
            if (spriteRenderer != null && monsterAvatarSO.CreatureSprite != null)
            {
                spriteRenderer.sprite = monsterAvatarSO.CreatureSprite;
            }
            
            // 애니메이터 컨트롤러 설정
            if (animator != null && monsterAvatarSO.AnimatorController != null)
            {
                // AnimatorController를 RuntimeAnimatorController로 캐스팅
                RuntimeAnimatorController runtimeController = monsterAvatarSO.AnimatorController;
                animator.runtimeAnimatorController = runtimeController;
                
                Debug.Log($"애니메이터 컨트롤러 설정 완료: {monsterAvatarSO.AnimatorController.name}");
            }
        }
    }
}