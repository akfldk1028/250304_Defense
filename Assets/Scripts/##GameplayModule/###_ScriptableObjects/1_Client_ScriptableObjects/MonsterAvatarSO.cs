using System;
using UnityEngine;
using Unity.Assets.Scripts.Data;
using Unity.Assets.Scripts.Resource;

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// 몬스터의 시각적 요소와 데이터 참조를 관리하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterAvatar", menuName = "GameData/MonsterAvatar", order = 2)]
    public class MonsterAvatarSO : CreatureAvatarSO, IResettable
    {
 
        
        [Header("몬스터 전용 이펙트")]
        [Tooltip("몬스터의 공격 이펙트")]
        [SerializeField] private GameObject attackEffectPrefab;
        
        [Tooltip("몬스터의 피격 이펙트")]
        [SerializeField] private GameObject hitEffectPrefab;
        

        public GameObject AttackEffectPrefab => attackEffectPrefab;
 
        public GameObject HitEffectPrefab => hitEffectPrefab;
        
  
        
        public override void Reset()
        {
            // 부모 클래스의 Reset 호출
            base.Reset();
            
            // 몬스터 특화 속성 초기화
            attackEffectPrefab = null;
            hitEffectPrefab = null;
            
            Debug.Log($"[MonsterAvatarSO] {name} 초기화 완료");
        }
    }
}