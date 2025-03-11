using UnityEngine;
using Unity.Assets.Scripts.Data;

namespace Unity.Assets.Scripts.Objects
{
    [CreateAssetMenu(fileName = "Monster_Stats", menuName = "GameData/Monster_Stats", order = 1)]
    public class MonsterStatsSO : CreatureStatsSO
    {
        // MonsterData 추가 필드들
        [Header("몬스터 전용 정보")]
        [SerializeField] private int dropItemId;
        
        // 몬스터 전용 프로퍼티
        public int DropItemId => dropItemId;
        
        // JSON 데이터에서 값 설정
        public void InitializeFromData(MonsterData data)
        {
            // 부모 클래스의 메서드 호출하여 CreatureData 필드 초기화
            InitializeFromCreatureData(data);
            
            // MonsterData 추가 필드 초기화
            dropItemId = data.DropItemId;
        }
        
        // MonsterData 객체 생성
        public MonsterData CreateMonsterData()
        {
            MonsterData data = new MonsterData();
            
            // 부모 클래스의 메서드 호출하여 CreatureData 필드 설정
            ApplyToCreatureData(data);
            
            // MonsterData 추가 필드 설정
            data.DropItemId = dropItemId;
            
            return data;
        }
    }
} 