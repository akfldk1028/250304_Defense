using System;
using UnityEngine;
using Unity.Assets.Scripts.Data;
using UnityEditor.Animations;

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// 생물체의 시각적 요소와 데이터 참조를 관리하는 ScriptableObject의 기본 클래스입니다.
    /// 이 클래스는 프리팹, 스프라이트, 애니메이터 컨트롤러와 같은 시각적 요소를 포함합니다.
    /// </summary>
    public abstract class CreatureAvatarSO : GuidScriptableObject
    {
        [Header("식별 정보")]
        [Tooltip("생물체의 고유 ID (CreatureStatsSO의 DataId와 일치해야 함)")]
        [SerializeField] protected int dataId;
        
        [Header("에셋 경로")]
        [Tooltip("애니메이션 컨트롤러 에셋 경로")]
        [SerializeField] protected string animatorControllerPath;
        
        [Tooltip("스프라이트 에셋 경로")]
        [SerializeField] protected string spritePath;
        
        [Tooltip("프리팹 에셋 경로")]
        [SerializeField] protected string prefabPath;
        
        [Header("시각적 요소")]
        [Tooltip("생물체의 게임 오브젝트 프리팹")]
        [SerializeField] protected GameObject creaturePrefab;
        
        [Tooltip("생물체의 대표 스프라이트")]
        [SerializeField] protected Sprite creatureSprite;
        
        [Tooltip("생물체의 애니메이션 컨트롤러")]
        [SerializeField] protected AnimatorController animatorController;
        
        [Header("오디오")]
        [Tooltip("생물체의 기본 사운드 효과")]
        [SerializeField] protected AudioClip[] creatureSounds;
        
        [Header("이펙트")]
        [Tooltip("생물체의 스폰 이펙트")]
        [SerializeField] protected GameObject spawnEffectPrefab;
        
        [Tooltip("생물체의 사망 이펙트")]
        [SerializeField] protected GameObject deathEffectPrefab;
        
        /// <summary>
        /// 생물체의 고유 ID를 반환합니다.
        /// </summary>
        public int DataId => dataId;
        
        /// <summary>
        /// 애니메이션 컨트롤러 에셋 경로를 반환합니다.
        /// </summary>
        public string AnimatorControllerPath => animatorControllerPath;
        
        /// <summary>
        /// 스프라이트 에셋 경로를 반환합니다.
        /// </summary>
        public string SpritePath => spritePath;
        
        /// <summary>
        /// 프리팹 에셋 경로를 반환합니다.
        /// </summary>
        public string PrefabPath => prefabPath;
        
        /// <summary>
        /// 생물체의 게임 오브젝트 프리팹을 반환합니다.
        /// </summary>
        public GameObject CreaturePrefab => creaturePrefab;
        
        /// <summary>
        /// 생물체의 대표 스프라이트를 반환합니다.
        /// </summary>
        public Sprite CreatureSprite => creatureSprite;
        
        /// <summary>
        /// 생물체의 애니메이션 컨트롤러를 반환합니다.
        /// </summary>
        public AnimatorController AnimatorController => animatorController;
        
        /// <summary>
        /// 생물체의 사운드 효과 배열을 반환합니다.
        /// </summary>
        public AudioClip[] CreatureSounds => creatureSounds;
        
        /// <summary>
        /// 생물체의 스폰 이펙트 프리팹을 반환합니다.
        /// </summary>
        public GameObject SpawnEffectPrefab => spawnEffectPrefab;
        
        /// <summary>
        /// 생물체의 사망 이펙트 프리팹을 반환합니다.
        /// </summary>
        public GameObject DeathEffectPrefab => deathEffectPrefab;
        
        /// <summary>
        /// 생물체의 데이터 참조를 반환합니다.
        /// 하위 클래스에서 구현해야 합니다.
        /// </summary>
        public abstract CreatureStatsSO CreatureData { get; }
        
        /// <summary>
        /// 애니메이션 컨트롤러를 설정합니다.
        /// </summary>
        /// <param name="controller">설정할 애니메이션 컨트롤러</param>
        public void SetAnimatorController(AnimatorController controller)
        {
            animatorController = controller;
            
            #if UNITY_EDITOR
            // 에디터에서 변경사항 저장
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
        
        /// <summary>
        /// 스프라이트를 설정합니다.
        /// </summary>
        /// <param name="sprite">설정할 스프라이트</param>
        public void SetCreatureSprite(Sprite sprite)
        {
            creatureSprite = sprite;
            
            #if UNITY_EDITOR
            // 에디터에서 변경사항 저장
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
        
        /// <summary>
        /// 프리팹을 설정합니다.
        /// </summary>
        /// <param name="prefab">설정할 프리팹</param>
        public void SetCreaturePrefab(GameObject prefab)
        {
            creaturePrefab = prefab;
            
            #if UNITY_EDITOR
            // 에디터에서 변경사항 저장
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
        
        /// <summary>
        /// 데이터 ID를 설정합니다.
        /// </summary>
        /// <param name="id">설정할 데이터 ID</param>
        public void SetDataId(int id)
        {
            dataId = id;
            
            #if UNITY_EDITOR
            // 에디터에서 변경사항 저장
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
        
        /// <summary>
        /// 애니메이션 컨트롤러 경로를 설정합니다.
        /// </summary>
        /// <param name="path">설정할 경로</param>
        public void SetAnimatorControllerPath(string path)
        {
            animatorControllerPath = path;
            
            #if UNITY_EDITOR
            // 에디터에서 변경사항 저장
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
        
        /// <summary>
        /// 스프라이트 경로를 설정합니다.
        /// </summary>
        /// <param name="path">설정할 경로</param>
        public void SetSpritePath(string path)
        {
            spritePath = path;
            
            #if UNITY_EDITOR
            // 에디터에서 변경사항 저장
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
        
        /// <summary>
        /// 프리팹 경로를 설정합니다.
        /// </summary>
        /// <param name="path">설정할 경로</param>
        public void SetPrefabPath(string path)
        {
            prefabPath = path;
            
            #if UNITY_EDITOR
            // 에디터에서 변경사항 저장
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }
        
        /// <summary>
        /// 유효성 검사를 수행합니다.
        /// </summary>
        // protected override void OnValidate()
        // {
        //     base.OnValidate();
            
        //     // 프리팹이 없는 경우 경고
        //     if (creaturePrefab == null)
        //     {
        //         Debug.LogWarning($"Creature Prefab {name}의 Prefab이 설정되지 않았습니다!");
        //     }
        // }
    }
}