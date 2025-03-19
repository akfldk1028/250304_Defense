using UnityEngine;
using Unity.Assets.Scripts.Data;

namespace Unity.Assets.Scripts.Objects
{
    [CreateAssetMenu(fileName = "New Hero Stats", menuName = "ScriptableObjects/Hero Stats")]
    public class HeroStatsSO : CreatureStatsSO
    {
        [Header("영웅 전용 속성")]
        [SerializeField] private int experiencePoints;
        [SerializeField] private int level;
        [SerializeField] private float levelUpMultiplier = 1.1f;
        
        // 영웅 전용 프로퍼티
        public int ExperiencePoints => experiencePoints;
        public int Level => level;
        public float LevelUpMultiplier => levelUpMultiplier;
        
        // 영웅 데이터 초기화 메서드 (나중에 HeroData 클래스가 생성되면 사용)
        public void InitializeFromHeroData(HeroData data)
        {
            // 기본 CreatureData 속성 초기화
            base.InitializeFromCreatureData(data);
            
            // 영웅 전용 속성 초기화
            experiencePoints = data.ExperiencePoints;
            level = data.Level;
            levelUpMultiplier = data.LevelUpMultiplier;
        }
    }
    
    // 임시 HeroData 클래스 (나중에 별도 파일로 분리 가능)
    [System.Serializable]
    public class HeroData : CreatureData
    {
        public int ExperiencePoints;
        public int Level;
        public float LevelUpMultiplier;
    }
} 