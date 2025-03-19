using UnityEngine;
using System.Collections.Generic;
using Unity.Assets.Scripts.Data;
using System;  // GUID 사용을 위해 추가
using Unity.Assets.Scripts.Resource;
using VContainer;
using UnityEditor.Animations;  // VContainer 네임스페이스 추가


//TODO 모든 SO 통합 클래스로 변화해야함
//list, 딕셔너리 , 폴더기반, 메타데이터기반 음악전용 SO

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// 모든 ScriptableObject를 관리하는 통합 매니저 클래스
    /// 몬스터, 영웅 등 다양한 타입의 ScriptableObject를 관리합니다.
    /// </summary>
    public class SOManager
    {
        private static SOManager instance;

        #region 캐싱 딕셔너리
        // GUID 기반 캐싱
        private Dictionary<Guid, CreatureStatsSO> statsSOByGuid = new Dictionary<Guid, CreatureStatsSO>();
        private Dictionary<Guid, CreatureAvatarSO> avatarSOByGuid = new Dictionary<Guid, CreatureAvatarSO>();
        
        // ID 기반 캐싱
        private Dictionary<int, CreatureStatsSO> statsSOById = new Dictionary<int, CreatureStatsSO>();
        private Dictionary<int, CreatureAvatarSO> avatarSOById = new Dictionary<int, CreatureAvatarSO>();
        
        // 타입별 캐싱 (몬스터)
        private Dictionary<Guid, MonsterStatsSO> monsterStatsSOByGuid = new Dictionary<Guid, MonsterStatsSO>();
        private Dictionary<int, MonsterStatsSO> monsterStatsSOById = new Dictionary<int, MonsterStatsSO>();

        private Dictionary<Guid, MonsterAvatarSO> monsterAvatarSOByGuid = new Dictionary<Guid, MonsterAvatarSO>();
        private Dictionary<int, MonsterAvatarSO> monsterAvatarSOById = new Dictionary<int, MonsterAvatarSO>();
        
        // 타입별 캐싱 (영웅)
        private Dictionary<Guid, HeroStatsSO> heroStatsSOByGuid = new Dictionary<Guid, HeroStatsSO>();
        private Dictionary<int, HeroStatsSO> heroStatsSOById = new Dictionary<int, HeroStatsSO>();
        #endregion

        // 싱글톤 접근자 (VContainer 환경에서는 이 방식 대신 DI를 사용해야 함)
        public static SOManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("[SOManager] 싱글톤 인스턴스가 없습니다. VContainer를 통해 생성된 인스턴스를 사용하는 것이 권장됩니다.");
                    instance = new SOManager();
                }
                return instance;
            }
            // VContainer에서 생성된 인스턴스를 설정할 수 있도록 setter 추가
            set
            {
                instance = value;
            }
        }
        
        // Addressable 에셋 로드를 위한 필드 추가 (필드 주입 방식)
        [Inject] 
        private ResourceManager resourceManager;
        
        [Inject]
        private DataLoader dataLoader;
        
        // 초기화 상태 추적
        private bool isInitialized = false;
        
        // 기본 생성자
        public SOManager()
        {
            // VContainer에서 자동으로 resourceManager를 주입함
        }
        
        // 초기화 메서드
        public void Initialize()
        {
            // 이미 초기화되었으면 중복 실행 방지
            if (isInitialized)
            {
                Debug.Log("[SOManager] 이미 초기화되었습니다.");
                return;
            }
            
            // 모든 ScriptableObject 에셋 로드
            LoadAllScriptableObjects();
            
            // DataLoader 초기화 확인 및 이벤트 구독
            if (dataLoader != null)
            {
                // DataLoader에 초기화 완료 이벤트 추가
                if (!dataLoader.IsInitialized)
                {
                    Debug.Log("[SOManager] DataLoader가 아직 초기화되지 않았습니다. 초기화 완료 이벤트를 기다립니다.");
                    
                    // DataLoader에 초기화 완료 이벤트 추가 (DataLoader 클래스에 이벤트 추가 필요)
                    dataLoader.OnInitialized += OnDataLoaderInitialized;
                }
                else
                {
                    Debug.Log("[SOManager] DataLoader가 이미 초기화되었습니다. JSON 데이터를 바로 적용합니다.");
                    ApplyJSONDataToScriptableObjects();
                }
            }
            else
            {
                Debug.LogWarning("[SOManager] DataLoader가 null입니다. JSON 데이터를 적용할 수 없습니다.");
            }
            
            // ResourceManager의 로딩 완료 이벤트 구독
            if (resourceManager != null)
            {
                // 이미 로딩이 완료되었는지 확인
                bool isResourceLoaded = false;
                
                // 로딩 완료 이벤트 구독
                resourceManager.OnLoadingCompleted += () => 
                {
                    Debug.Log("[SOManager] ResourceManager 로딩 완료 이벤트 수신");
                    
                    // 중복 실행 방지
                    if (!isResourceLoaded)
                    {
                        isResourceLoaded = true;
                        
                        // Addressable에서 애니메이션 컨트롤러와 스프라이트 로드하여 적용
                        LoadAndApplyAddressableAssets();
                    }
                };
                
                Debug.Log("[SOManager] ResourceManager 로딩 완료 이벤트 구독 완료");
                
                // 이미 리소스가 로드되었는지 확인
                if (resourceManager.Resources.Count > 0)
                {
                    Debug.Log($"[SOManager] 이미 {resourceManager.Resources.Count}개의 리소스가 로드되어 있습니다. 바로 적용합니다.");
                    isResourceLoaded = true;
                    LoadAndApplyAddressableAssets();
                }
                else
                {
                    Debug.Log("[SOManager] ResourceManager에 로드된 리소스가 없습니다. 로딩 완료 이벤트를 기다립니다.");
                }
            }
            else
            {
                Debug.LogWarning("[SOManager] ResourceManager가 null입니다. Addressable 에셋을 로드할 수 없습니다.");
            }
            
            // 초기화 완료 표시
            isInitialized = true;
            Debug.Log("[SOManager] 초기화 완료");
        }
        
        // DataLoader 초기화 완료 이벤트 핸들러
        private void OnDataLoaderInitialized()
        {
            Debug.Log("[SOManager] DataLoader 초기화 완료 이벤트 수신");
            
            // JSON 데이터 적용
            ApplyJSONDataToScriptableObjects();
            
            // 이벤트 구독 해제
            dataLoader.OnInitialized -= OnDataLoaderInitialized;
        }
        
        // 모든 ScriptableObject 로드 및 캐싱
        private void LoadAllScriptableObjects()
        {
            // 모든 타입의 ScriptableObject 로드
            LoadAllCreatureStatsSO();
            LoadAllCreatureAvatarSO();
            
            // 타입별 로드 및 캐싱
            LoadAllMonsterStatsSO();
            LoadAllMonsterAvatarSO();
            // LoadAllHeroStatsSO();
            
            Debug.Log("[SOManager] 모든 ScriptableObject 로드 완료");
        }
        
        // 모든 CreatureStatsSO 로드 및 캐싱 (기본 타입)
        private void LoadAllCreatureStatsSO()
        {
            CreatureStatsSO[] assets = Resources.LoadAll<CreatureStatsSO>("");
            Debug.Log($"[SOManager] {assets.Length}개의 CreatureStatsSO 로드됨");
            
            statsSOById.Clear();
            statsSOByGuid.Clear();
            
            foreach (var asset in assets)
            {
                statsSOById[asset.DataId] = asset;
                statsSOByGuid[asset.Guid] = asset;
            }
        }
        
        // 모든 CreatureAvatarSO 로드 및 캐싱 (기본 타입)
        private void LoadAllCreatureAvatarSO()
        {
            CreatureAvatarSO[] assets = Resources.LoadAll<CreatureAvatarSO>("");
            Debug.Log($"[SOManager] {assets.Length}개의 CreatureAvatarSO 로드됨");
            
            avatarSOById.Clear();
            avatarSOByGuid.Clear();
            
            foreach (var asset in assets)
            {
                if (asset.DataId > 0)
                {
                    avatarSOById[asset.DataId] = asset;
                }
                avatarSOByGuid[asset.Guid] = asset;
            }
        }
        
        // 모든 MonsterStatsSO 로드 및 캐싱
        private void LoadAllMonsterStatsSO()
        {
            MonsterStatsSO[] assets = Resources.LoadAll<MonsterStatsSO>("");
            Debug.Log($"[SOManager] {assets.Length}개의 MonsterStatsSO 로드됨");
            
            monsterStatsSOById.Clear();
            monsterStatsSOByGuid.Clear();
            
            foreach (var asset in assets)
            {
                monsterStatsSOById[asset.DataId] = asset;
                monsterStatsSOByGuid[asset.Guid] = asset;
            }
        }
        
        // 모든 MonsterAvatarSO 로드 및 캐싱
        private void LoadAllMonsterAvatarSO()
        {
            Debug.Log($"<color=magenta>[SOManager] ▶ MonsterAvatarSO 로드 시작</color>");
            
            MonsterAvatarSO[] assets = Resources.LoadAll<MonsterAvatarSO>("");
            Debug.Log($"<color=magenta>[SOManager] {assets.Length}개의 MonsterAvatarSO 로드됨</color>");
            
            monsterAvatarSOById.Clear();
            monsterAvatarSOByGuid.Clear();
            
            Debug.Log($"<color=magenta>[SOManager] 캐시 초기화 완료, 각 MonsterAvatarSO 처리 시작:</color>");
            
            foreach (var asset in assets)
            {
                Debug.Log($"<color=magenta>  ▶ 처리 중: {asset.name}</color>");
                
                // 1. MonsterData의 DataId를 키로 사용
                // if (asset.MonsterData != null)
                // {
                //     int dataId = asset.MonsterData.DataId;
                //     Guid guid = asset.MonsterData.Guid;
                    
                //     monsterAvatarSOById[dataId] = asset;
                //     monsterAvatarSOByGuid[guid] = asset;
                    
                //     Debug.Log($"<color=magenta>    ✓ MonsterData 있음 - ID: {dataId}, GUID: {guid}</color>");
                    
                //     // 2. CreatureAvatarSO의 DataId 설정 (MonsterData의 DataId와 동일하게)
                //     if (asset.DataId == 0)  // DataId가 설정되지 않은 경우에만
                //     {
                //         asset.SetDataId(dataId);
                //         Debug.Log($"<color=magenta>    ✓ {asset.name}의 DataId를 {dataId}로 설정</color>");
                //     }
                // }
                // else
                // {
                //     Debug.LogWarning($"<color=orange>    ⚠ {asset.name}의 MonsterData가 null입니다.</color>");
                    
                //     // MonsterData가 null이면 DataId를 키로 사용 (있는 경우)
                //     if (asset.DataId > 0)
                //     {
                //         monsterAvatarSOById[asset.DataId] = asset;
                //         Debug.Log($"<color=magenta>    ✓ DataId로 캐싱: {asset.DataId}</color>");
                //     }
                //     else
                //     {
                //         Debug.LogWarning($"<color=orange>    ⚠ {asset.name}의 DataId가 0 이하입니다. ID로 캐싱되지 않습니다.</color>");
                //     }
                    
                //     // GUID로 캐싱
                //     monsterAvatarSOByGuid[asset.Guid] = asset;
                //     Debug.Log($"<color=magenta>    ✓ GUID로 캐싱: {asset.Guid}</color>");
                // }
            }
            
            Debug.Log($"<color=magenta>[SOManager] ◀ MonsterAvatarSO 로드 완료 - ID 캐시: {monsterAvatarSOById.Count}개, GUID 캐시: {monsterAvatarSOByGuid.Count}개</color>");
        }
        
        // 모든 HeroStatsSO 로드 및 캐싱
        private void LoadAllHeroStatsSO()
        {
            HeroStatsSO[] assets = Resources.LoadAll<HeroStatsSO>("");
            Debug.Log($"[SOManager] {assets.Length}개의 HeroStatsSO 로드됨");
            
            heroStatsSOById.Clear();
            heroStatsSOByGuid.Clear();
            
            foreach (var asset in assets)
            {
                heroStatsSOById[asset.DataId] = asset;
                heroStatsSOByGuid[asset.Guid] = asset;
            }
        }
        
        // JSON 데이터를 ScriptableObject에 자동 적용
        private void ApplyJSONDataToScriptableObjects()
        {
            if (dataLoader == null)
            {
                Debug.LogError("[SOManager] DataLoader가 주입되지 않았습니다. JSON 데이터를 적용할 수 없습니다.");
                return;
            }
            
            try
            {
                // 몬스터 데이터 적용
                ApplyMonsterJSONData(dataLoader);
                
                // 영웅 데이터 적용 (나중에 구현)
                // ApplyHeroJSONData(dataLoader);
                
                Debug.Log($"[SOManager] JSON 데이터 적용 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SOManager] JSON 데이터 적용 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // 몬스터 JSON 데이터 적용
        private void ApplyMonsterJSONData(DataLoader dataLoader)
        {
            if (dataLoader.MonsterDic == null || dataLoader.MonsterDic.Count == 0)
            {
                Debug.LogWarning("[SOManager] 몬스터 JSON 데이터를 로드하지 못했습니다.");
                return;
            }
            
            int updatedCount = 0;
            
            // 각 ScriptableObject에 JSON 데이터 적용
            foreach (var kvp in monsterStatsSOById)
            {
                int monsterId = kvp.Key;
                MonsterStatsSO monsterSO = kvp.Value;
                
                if (dataLoader.MonsterDic.TryGetValue(monsterId, out MonsterData monsterData))
                {
                    // JSON 데이터 적용
                    monsterSO.InitializeFromData(monsterData);
                    updatedCount++;
                }
                else
                {
                    Debug.LogWarning($"[SOManager] ID {monsterId}에 해당하는 JSON 데이터를 찾을 수 없습니다.");
                }
            }
            
            Debug.Log($"[SOManager] {updatedCount}개의 MonsterStatsSO에 JSON 데이터 적용 완료");
        }
        
        // Addressable에서 애니메이션 컨트롤러와 스프라이트 로드하여 적용하는 메서드
        private void LoadAndApplyAddressableAssets()
        {
            Debug.Log("<color=lime>[SOManager] ▶ Addressable에서 에셋 로드 시작</color>");
            
            // 모든 CreatureAvatarSO 처리
            // foreach (var avatarSO in avatarSOById.Values)
            // {
            //     // CreatureData가 있는 경우에만 처리
            //     if (avatarSO.CreatureData != null)
            //     {
            //         string prefabLabel = avatarSO.CreatureData.PrefabLabel;
            //         if (!string.IsNullOrEmpty(prefabLabel))
            //         {
            //             // 타입에 따라 적절한 처리
            //             if (avatarSO is MonsterAvatarSO monsterAvatarSO)
            //             {
            //                 LoadAssets(monsterAvatarSO, prefabLabel);
            //             }
            //             // 나중에 HeroAvatarSO 등 다른 타입 추가 가능
            //         }
            //     }
            // }
            
            Debug.Log("<color=lime>[SOManager] ◀ Addressable 에셋 로드 완료</color>");
        }
        
        // 몬스터 에셋 로드
        private void LoadAssets(MonsterAvatarSO avatarSO, string prefabLabel)
        {
            // 애니메이션 컨트롤러 로드
            // LoadAssetGeneric<AnimatorController>(
            //     prefabLabel,
            //     new List<string> {
            //         $"Assets/01_Animations/{prefabLabel}/{prefabLabel}.controller",
            //         $"Assets/01_Animations/{prefabLabel}"
            //     },
            //     avatarSO.AnimatorController,
            //     (controller) => avatarSO.SetAnimatorController(controller),
            //     "Animation Controller"
            // );
            
            // // 스프라이트 로드
            // LoadAssetGeneric<Sprite>(
            //     prefabLabel,
            //     new List<string> {
            //         $"Assets/04_Sprites/Character/{prefabLabel}/{prefabLabel}.asset",
            //         $"Assets/04_Sprites/Character/{prefabLabel}"
            //     },
            //     avatarSO.CreatureSprite,
            //     (sprite) => avatarSO.SetCreatureSprite(sprite),
            //     "Sprite"
            // );
            
            // // 프리팹 로드
            // LoadAssetGeneric<GameObject>(
            //     prefabLabel,
            //     new List<string> {
            //         $"Assets/02_Prefabs/Character/{prefabLabel}/{prefabLabel}.prefab",
            //         $"Assets/02_Prefabs/Character/{prefabLabel}"
            //     },
            //     avatarSO.CreaturePrefab,
            //     (prefab) => avatarSO.SetCreaturePrefab(prefab),
            //     (path) => avatarSO.SetPrefabPath(path),
            //     "Prefab"
            // );
        }
        
        /// <summary>
        /// 제네릭 에셋 로드 메서드 - 다양한 유형의 에셋을 로드하는 공통 로직
        /// </summary>
        /// <typeparam name="T">로드할 에셋 유형</typeparam>
        /// <param name="assetLabel">에셋 라벨/이름</param>
        /// <param name="possiblePaths">시도할 가능한 경로 목록</param>
        /// <param name="existingAsset">이미 존재하는 에셋 (있으면 로드 건너뜀)</param>
        /// <param name="setAssetAction">에셋을 설정하는 액션</param>
        /// <param name="setPathAction">경로를 설정하는 액션</param>
        /// <param name="assetTypeName">로그 출력용 에셋 유형 이름</param>
        private void LoadAssetGeneric<T>(
            string assetLabel,
            List<string> possiblePaths,
            T existingAsset,
            Action<T> setAssetAction,
            Action<string> setPathAction,
            string assetTypeName) where T : UnityEngine.Object
        {
            // 이미 설정되어 있으면 건너뛰기
            if (existingAsset != null)
            {
                return;
            }
            
            // 성공한 경로를 저장할 변수
            string successPath = "";
            
            // 캐시에서 먼저 찾기
            T foundAsset = null;
            foreach (var path in possiblePaths)
            {
                foundAsset = resourceManager.Load<T>(path);
                if (foundAsset != null)
                {
                    successPath = path;  // 성공한 경로 저장
                    Debug.Log($"<color=yellow>★★★ 성공! 캐시에서 {assetTypeName} 찾음: {path}, 이름: {foundAsset.name} ★★★</color>");
                    break;
                }
            }
            
            // 캐시에서 찾지 못했으면 이름으로 찾기
            if (foundAsset == null)
            {
                // 이미 로드된 에셋 중에서 이름으로 찾기
                foreach (var kvp in resourceManager.Resources)
                {
                    Debug.Log($"<color=green>kvp.Key: {kvp.Key}, kvp.Value: {kvp.Value} ===> {kvp}</color>");
                    if (kvp.Value is T)
                    {
                        // 이름 또는 경로에 assetLabel이 포함되어 있는지 확인
                        bool keyMatches = kvp.Key.IndexOf(assetLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool nameMatches = kvp.Value.name.IndexOf(assetLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (keyMatches || nameMatches)
                        {
                            foundAsset = kvp.Value as T;
                            successPath = kvp.Key;  // 성공한 경로 저장
                            Debug.Log($"<color=yellow>★★★ 성공! 이름 매칭으로 {assetTypeName} 찾음: {kvp.Key}, 이름: {kvp.Value.name} ★★★</color>");
                            break;
                        }
                    }
                }
            }
            
            // 찾은 에셋 설정
            if (foundAsset != null)
            {
                setAssetAction(foundAsset);
                
                // 성공한 경로를 에셋 경로 필드에 저장
                if (!string.IsNullOrEmpty(successPath))
                {
                    setPathAction(successPath);
                }
            }
        }
        
        // CharacterType에 따라 몬스터 키 결정
        private string GetMonsterKeyFromCharacterType(CharacterTypeEnum characterType)
        {
            switch (characterType)
            {
                case CharacterTypeEnum.green_slime:
                    return "green_slime";
                case CharacterTypeEnum.Imp:
                    return "imp";
                case CharacterTypeEnum.ImpBoss:
                    return "imp_boss";
                case CharacterTypeEnum.VandalImp:
                    return "vandal_imp";
                // 다른 몬스터 타입 추가
                default:
                    return characterType.ToString().ToLower();
            }
        }
        
        #region 범용 접근자 메서드
        // 범용 CreatureStatsSO 접근자
        public CreatureStatsSO GetStatsSOById(int id)
        {
            if (statsSOById.TryGetValue(id, out var statsSO))
            {
                return statsSO;
            }
            
            Debug.LogWarning($"[SOManager] ID {id}에 해당하는 CreatureStatsSO를 찾을 수 없습니다.");
            return null;
        }
        
        public CreatureStatsSO GetStatsSOByGuid(Guid guid)
        {
            if (statsSOByGuid.TryGetValue(guid, out var statsSO))
            {
                return statsSO;
            }
            
            Debug.LogWarning($"[SOManager] GUID {guid}에 해당하는 CreatureStatsSO를 찾을 수 없습니다.");
            return null;
        }
        
        // 범용 CreatureAvatarSO 접근자
        public CreatureAvatarSO GetAvatarSOById(int id)
        {
            if (avatarSOById.TryGetValue(id, out var avatarSO))
            {
                return avatarSO;
            }
            
            Debug.LogWarning($"[SOManager] ID {id}에 해당하는 CreatureAvatarSO를 찾을 수 없습니다.");
            return null;
        }
        
        public CreatureAvatarSO GetAvatarSOByGuid(Guid guid)
        {
            if (avatarSOByGuid.TryGetValue(guid, out var avatarSO))
            {
                return avatarSO;
            }
            
            Debug.LogWarning($"[SOManager] GUID {guid}에 해당하는 CreatureAvatarSO를 찾을 수 없습니다.");
            return null;
        }
        #endregion
        
        #region 몬스터 전용 접근자 메서드
        // 몬스터 접근자 (기존 코드 유지)
        public MonsterAvatarSO GetMonsterAvatarSOById(int id)
        {
            if (monsterAvatarSOById.TryGetValue(id, out var avatarSO))
            {
                return avatarSO;
            }
            
            Debug.LogWarning($"[SOManager] ID {id}에 해당하는 MonsterAvatarSO를 찾을 수 없습니다.");
            return null;
        }
        
        public MonsterAvatarSO GetMonsterAvatarSOByGuid(Guid guid)
        {
            if (monsterAvatarSOByGuid.TryGetValue(guid, out var avatarSO))
            {
                return avatarSO;
            }
            
            Debug.LogWarning($"[SOManager] GUID {guid}에 해당하는 MonsterAvatarSO를 찾을 수 없습니다.");
            return null;
        }
        
        public MonsterStatsSO GetMonsterStatsSOById(int id)
        {
            if (monsterStatsSOById.TryGetValue(id, out var statsSO))
            {
                return statsSO;
            }
            
            Debug.LogWarning($"[SOManager] ID {id}에 해당하는 MonsterStatsSO를 찾을 수 없습니다.");
            return null;
        }
        
        public MonsterStatsSO GetMonsterStatsSOByGuid(Guid guid)
        {
            if (monsterStatsSOByGuid.TryGetValue(guid, out var statsSO))
            {
                return statsSO;
            }
            
            Debug.LogWarning($"[SOManager] GUID {guid}에 해당하는 MonsterStatsSO를 찾을 수 없습니다.");
            return null;
        }
        #endregion
        
        #region 영웅 전용 접근자 메서드
        // 영웅 접근자
        public HeroStatsSO GetHeroStatsSOById(int id)
        {
            if (heroStatsSOById.TryGetValue(id, out var statsSO))
            {
                return statsSO;
            }
            
            Debug.LogWarning($"[SOManager] ID {id}에 해당하는 HeroStatsSO를 찾을 수 없습니다.");
            return null;
        }
        
        public HeroStatsSO GetHeroStatsSOByGuid(Guid guid)
        {
            if (heroStatsSOByGuid.TryGetValue(guid, out var statsSO))
            {
                return statsSO;
            }
            
            Debug.LogWarning($"[SOManager] GUID {guid}에 해당하는 HeroStatsSO를 찾을 수 없습니다.");
            return null;
        }
        #endregion
    }
} 