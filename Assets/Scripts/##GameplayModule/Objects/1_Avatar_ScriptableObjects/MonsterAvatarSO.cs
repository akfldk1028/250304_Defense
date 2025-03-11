using System;
using UnityEngine;
using Unity.Assets.Scripts.Data;

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// 몬스터의 시각적 요소와 데이터 참조를 관리하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterAvatar", menuName = "GameData/MonsterAvatar", order = 2)]
    public class MonsterAvatarSO : CreatureAvatarSO
    {
        [Header("몬스터 데이터 참조")]
        [Tooltip("몬스터의 데이터 참조")]
        [SerializeField] private MonsterStatsSO monsterData;
        
        [Header("몬스터 전용 이펙트")]
        [Tooltip("몬스터의 공격 이펙트")]
        [SerializeField] private GameObject attackEffectPrefab;
        
        [Tooltip("몬스터의 피격 이펙트")]
        [SerializeField] private GameObject hitEffectPrefab;
        
        /// <summary>
        /// 생물체 데이터 참조를 반환합니다.
        /// </summary>
        public override CreatureStatsSO CreatureData => monsterData;
        
        /// <summary>
        /// 몬스터 데이터 참조를 반환합니다.
        /// </summary>
        public MonsterStatsSO MonsterData => monsterData;
        
        /// <summary>
        /// 몬스터의 공격 이펙트 프리팹을 반환합니다.
        /// </summary>
        public GameObject AttackEffectPrefab => attackEffectPrefab;
        
        /// <summary>
        /// 몬스터의 피격 이펙트 프리팹을 반환합니다.
        /// </summary>
        public GameObject HitEffectPrefab => hitEffectPrefab;
        
        /// <summary>
        /// 유효성 검사를 수행합니다.
        /// </summary>
        // protected override void OnValidate()
        // {
        //     base.OnValidate();
            
        //     // 몬스터 데이터가 없는 경우 경고
        //     if (monsterData == null)
        //     {
        //         Debug.LogWarning($"Monster Prefab {name}의 MonsterData가 설정되지 않았습니다!");
        //     }
        // }
    }
}