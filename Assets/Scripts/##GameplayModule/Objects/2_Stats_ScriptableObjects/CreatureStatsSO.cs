using UnityEngine;
using Unity.Assets.Scripts.Data;
using System;

namespace Unity.Assets.Scripts.Objects
{
    public abstract class CreatureStatsSO : GuidScriptableObject  // ScriptableObject 대신 GuidScriptableObject 상속
    {
        // CreatureData 공통 필드들
        [Header("기본 식별자")]
        [SerializeField] protected int dataId;
        
        [Header("기본 정보")]
        [SerializeField] protected string descriptionTextID;
        [SerializeField] protected string prefabLabel;
        
        [Header("콜라이더 정보")]
        [SerializeField] protected float colliderOffsetX;
        [SerializeField] protected float colliderOffsetY;
        [SerializeField] protected float colliderRadius;
        
        [Header("능력치")]
        [SerializeField] protected float maxHp;
        [SerializeField] protected float upMaxHpBonus;
        [SerializeField] protected float atk;
        [SerializeField] protected float atkRange;
        [SerializeField] protected float atkBonus;
        [SerializeField] protected float moveSpeed;
        [SerializeField] protected float criRate;
        [SerializeField] protected float criDamage;
        
        [Header("시각적 요소")]
        [SerializeField] protected string iconImage;
        [SerializeField] protected string skeletonDataID;
        
        [Header("스킬 정보")]
        [SerializeField] protected int defaultSkillId;
        [SerializeField] protected int envSkillId;
        [SerializeField] protected int skillAId;
        [SerializeField] protected int skillBId;
        
        [Header("캐릭터 타입 정보")]
        [SerializeField] protected CharacterTypeEnum characterType;
        [SerializeField] protected bool isValidTarget;
        [SerializeField] protected bool isNpc;
        
        [Header("추가 정보 (에디터 전용)")]
        [TextArea(3, 10)]
        [SerializeField] protected string description;
        [SerializeField] protected Sprite creatureSprite;
        [SerializeField] protected GameObject creaturePrefab;
        
        // 프로퍼티들
        public int DataId => dataId;
        public string DescriptionTextID => descriptionTextID;
        public string PrefabLabel => prefabLabel;
        public float ColliderOffsetX => colliderOffsetX;
        public float ColliderOffsetY => colliderOffsetY;
        public float ColliderRadius => colliderRadius;
        public float MaxHp => maxHp;
        public float UpMaxHpBonus => upMaxHpBonus;
        public float Atk => atk;
        public float AtkRange => atkRange;
        public float AtkBonus => atkBonus;
        public float MoveSpeed => moveSpeed;
        public float CriRate => criRate;
        public float CriDamage => criDamage;
        public string IconImage => iconImage;
        public string SkeletonDataID => skeletonDataID;
        public int DefaultSkillId => defaultSkillId;
        public int EnvSkillId => envSkillId;
        public int SkillAId => skillAId;
        public int SkillBId => skillBId;
        public CharacterTypeEnum CharacterType => characterType;
        public bool IsValidTarget => isValidTarget;
        public bool IsNpc => isNpc;
        public Sprite CreatureSprite => creatureSprite;
        public GameObject CreaturePrefab => creaturePrefab;
        
        // CreatureData 초기화 메서드
        protected virtual void InitializeFromCreatureData(CreatureData data)
        {
            dataId = data.DataId;
            descriptionTextID = data.DescriptionTextID;
            prefabLabel = data.PrefabLabel;
            colliderOffsetX = data.ColliderOffsetX;
            colliderOffsetY = data.ColliderOffsetY;
            colliderRadius = data.ColliderRadius;
            maxHp = data.MaxHp;
            upMaxHpBonus = data.UpMaxHpBonus;
            atk = data.Atk;
            atkRange = data.AtkRange;
            atkBonus = data.AtkBonus;
            moveSpeed = data.MoveSpeed;
            criRate = data.CriRate;
            criDamage = data.CriDamage;
            iconImage = data.IconImage;
            skeletonDataID = data.SkeletonDataID;
            defaultSkillId = data.DefaultSkillId;
            envSkillId = data.EnvSkillId;
            skillAId = data.SkillAId;
            skillBId = data.SkillBId;
            
            // CharacterType은 문자열에서 Enum으로 변환
            if (Enum.TryParse<CharacterTypeEnum>(data.CharacterType, out var charType))
            {
                characterType = charType;
            }
            
            isValidTarget = data.IsValidTarget;
            isNpc = data.IsNpc;
        }
        
        // CreatureData 생성 메서드
        protected virtual void ApplyToCreatureData(CreatureData data)
        {
            data.DataId = dataId;
            data.DescriptionTextID = descriptionTextID;
            data.PrefabLabel = prefabLabel;
            data.ColliderOffsetX = colliderOffsetX;
            data.ColliderOffsetY = colliderOffsetY;
            data.ColliderRadius = colliderRadius;
            data.MaxHp = maxHp;
            data.UpMaxHpBonus = upMaxHpBonus;
            data.Atk = atk;
            data.AtkRange = atkRange;
            data.AtkBonus = atkBonus;
            data.MoveSpeed = moveSpeed;
            data.CriRate = criRate;
            data.CriDamage = criDamage;
            data.IconImage = iconImage;
            data.SkeletonDataID = skeletonDataID;
            data.DefaultSkillId = defaultSkillId;
            data.EnvSkillId = envSkillId;
            data.SkillAId = skillAId;
            data.SkillBId = skillBId;
            
            // Enum을 문자열로 변환
            data.CharacterType = characterType.ToString();
            
            data.IsValidTarget = isValidTarget;
            data.IsNpc = isNpc;
        }
    }
} 