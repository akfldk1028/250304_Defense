// using Unity.Assets.Scripts.Gameplay.GameplayObjects.Character;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// 네트워크 멀티플레이어 게임에서 몬스터의 클라이언트 측 시각화를 담당하는 클래스입니다.
    /// ClientCharacter를 상속받아 네트워크 기능을 활용합니다.
    /// </summary>
    public class ClientHero : ClientCreature
    {
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AudioSource audioSource;
        
        [Header("사운드 효과")]
        [SerializeField] private AudioClip spawnSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip deathSound;
        

        // 이전 방향 저장 (스프라이트 뒤집기용)
        
        private void Awake()
        {
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
   
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

        }
        
        public override void SetAvatar(HeroAvatarSO avatarSO)
        {
            base.SetAvatar(avatarSO);
       
        }
  
        

    }
}