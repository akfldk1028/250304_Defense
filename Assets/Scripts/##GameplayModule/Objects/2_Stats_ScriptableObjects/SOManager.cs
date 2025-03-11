using UnityEngine;
using System.Collections.Generic;
using Unity.Assets.Scripts.Data;
using System;  // GUID 사용을 위해 추가
using Unity.Assets.Scripts.Resource;
using VContainer;
using UnityEditor.Animations;  // VContainer 네임스페이스 추가


//TODO 모든 SO 통합 클래스로 변화해야함

namespace Unity.Assets.Scripts.Objects
{
    public class SOManager
    {
        private static SOManager instance;
        private Dictionary<Guid, MonsterStatsSO> monsterDataSOByGuid = new Dictionary<Guid, MonsterStatsSO>();
        private Dictionary<Guid, MonsterAvatarSO> monsterAvatarSOByGuid = new Dictionary<Guid, MonsterAvatarSO>();

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
        
        // 캐싱된 ScriptableObject 인스턴스들
        private Dictionary<int, MonsterStatsSO> monsterStatsSOCache = new Dictionary<int, MonsterStatsSO>();
        private Dictionary<int, MonsterAvatarSO> monsterAvatarSOCache = new Dictionary<int, MonsterAvatarSO>();
        
        // Addressable 에셋 로드를 위한 필드 추가 (필드 주입 방식)
        [Inject] 
        private ResourceManager resourceManager;
        
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
            LoadAllMonsterDataSO();
            LoadAllMonsterAvatarSO();
            
            // JSON 데이터 자동 적용
            ApplyJSONDataToScriptableObjects();
            
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
        
        // 모든 MonsterStatsSO 로드 및 캐싱
        private void LoadAllMonsterDataSO()
        {
            MonsterStatsSO[] assets = Resources.LoadAll<MonsterStatsSO>("");
            Debug.Log($"[SOManager] {assets.Length}개의 MonsterStatsSO 로드됨");
            
            monsterStatsSOCache.Clear();
            monsterDataSOByGuid.Clear();  // GUID 캐시 초기화
            
            foreach (var asset in assets)
            {
                monsterStatsSOCache[asset.DataId] = asset;
                monsterDataSOByGuid[asset.Guid] = asset;  // GUID로 캐싱
            }
        }
        
        // 모든 MonsterAvatarSO 로드 및 캐싱
        private void LoadAllMonsterAvatarSO()
        {
            MonsterAvatarSO[] assets = Resources.LoadAll<MonsterAvatarSO>("");
            Debug.Log($"[SOManager] {assets.Length}개의 MonsterAvatarSO 로드됨");
            
            monsterAvatarSOCache.Clear();
            monsterAvatarSOByGuid.Clear();  // GUID 캐시 초기화
            
            foreach (var asset in assets)
            {
                // 1. MonsterData의 DataId를 키로 사용
                if (asset.MonsterData != null)
                {
                    monsterAvatarSOCache[asset.MonsterData.DataId] = asset;
                    monsterAvatarSOByGuid[asset.MonsterData.Guid] = asset;  // GUID로 캐싱
                    
                    // 2. CreatureAvatarSO의 DataId 설정 (MonsterData의 DataId와 동일하게)
                    if (asset.DataId == 0)  // DataId가 설정되지 않은 경우에만
                    {
                        asset.SetDataId(asset.MonsterData.DataId);
                        Debug.Log($"[SOManager] {asset.name}의 DataId를 {asset.MonsterData.DataId}로 설정");
                    }
                    
                }
                else
                {
                    Debug.LogWarning($"[SOManager] {asset.name}의 MonsterData가 null입니다.");
                    
                    // MonsterData가 null이면 DataId를 키로 사용 (있는 경우)
                    if (asset.DataId > 0)
                    {
                        monsterAvatarSOCache[asset.DataId] = asset;
                    }
                    
                    // GUID로 캐싱
                    monsterAvatarSOByGuid[asset.Guid] = asset;
                }
            }
        }
        
        // JSON 데이터를 ScriptableObject에 자동 적용
        private void ApplyJSONDataToScriptableObjects()
        {
            // DataLoader 인스턴스 생성 및 초기화
            DataLoader dataLoader = new DataLoader();
            dataLoader.Init();
            
            if (dataLoader.MonsterDic == null || dataLoader.MonsterDic.Count == 0)
            {
                Debug.LogWarning("[SOManager] JSON 데이터를 로드하지 못했습니다.");
                return;
            }
            
            int updatedCount = 0;
            
            // 각 ScriptableObject에 JSON 데이터 적용
            foreach (var kvp in monsterStatsSOCache)
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
            
            Debug.Log($"[SOManager] {updatedCount}개의 ScriptableObject에 JSON 데이터 적용 완료");
        }
        
        // Addressable에서 애니메이션 컨트롤러와 스프라이트 로드하여 적용하는 메서드
        private void LoadAndApplyAddressableAssets()
        {
            Debug.Log("[SOManager] Addressable에서 에셋 로드 시작");
            
            // 모든 MonsterAvatarSO 처리
            Debug.Log($"[SOManager] 모든 MonsterAvatarSO 목록 (총 {monsterAvatarSOCache.Count}개):");
            
            // 성공한 에셋 정보를 저장할 변수
            Dictionary<string, (string path, string name, string type)> successInfo = new Dictionary<string, (string, string, string)>();
            
            // 각 MonsterAvatarSO에 대해 처리
            foreach (var kvp in monsterAvatarSOCache)
            {
                int dataId = kvp.Key;
                var avatarSO = kvp.Value;
                Debug.Log($"  - ID: {dataId}, 이름: {avatarSO.name}, GUID: {avatarSO.Guid}");
                
                if (avatarSO.MonsterData != null)
                {
                    // PrefabLabel 기반으로 에셋 매칭
                    string prefabLabel = avatarSO.MonsterData.PrefabLabel;
                    
                    // PrefabLabel이 없으면 CharacterType 사용
                    if (string.IsNullOrEmpty(prefabLabel))
                    {
                        prefabLabel = GetMonsterKeyFromCharacterType(avatarSO.MonsterData.CharacterType);
                    }
                    
                    // 둘 다 없으면 이름에서 추출
                    if (string.IsNullOrEmpty(prefabLabel))
                    {
                        prefabLabel = ExtractMonsterKeyFromName(avatarSO.name);
                    }
                    
                    if (!string.IsNullOrEmpty(prefabLabel))
                    {
                        // 에셋 로드 및 설정
                        LoadAssets(avatarSO, prefabLabel);
                        
                        // 성공 정보 저장
                        successInfo[avatarSO.name] = (
                            $"AnimPath: {avatarSO.AnimatorControllerPath}, SprPath: {avatarSO.SpritePath}, PrefabPath: {avatarSO.PrefabPath}",
                            $"Anim: {(avatarSO.AnimatorController != null ? avatarSO.AnimatorController.name : "null")}, " +
                            $"Spr: {(avatarSO.CreatureSprite != null ? avatarSO.CreatureSprite.name : "null")}, " +
                            $"Prefab: {(avatarSO.CreaturePrefab != null ? avatarSO.CreaturePrefab.name : "null")}",
                            $"DataId: {avatarSO.DataId}, GUID: {avatarSO.Guid}"
                        );
                    }
                    else
                    {
                        Debug.LogWarning($"    - PrefabLabel을 결정할 수 없습니다: {avatarSO.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"    - MonsterData가 null입니다!");
                }
            }
            
            // 성공 정보 한꺼번에 출력
            Debug.Log("=================================================");
            Debug.Log("★★★ 에셋 로드 성공 정보 요약 ★★★");
            foreach (var kvp in successInfo)
            {
                Debug.Log($"- {kvp.Key}");
                Debug.Log($"  경로: {kvp.Value.path}");
                Debug.Log($"  에셋: {kvp.Value.name}");
                Debug.Log($"  정보: {kvp.Value.type}");
                Debug.Log($"  --------------------------------------");
            }
            Debug.Log("=================================================");
            
            Debug.Log("[SOManager] Addressable 에셋 로드 완료");
        }
        
        // 모든 에셋 로드 및 설정
        private void LoadAssets(MonsterAvatarSO avatarSO, string prefabLabel)
        {
            // 애니메이션 컨트롤러 로드
            LoadAnimatorController(avatarSO, prefabLabel);
            
            // 스프라이트 로드
            LoadSprite(avatarSO, prefabLabel);
            
            // 프리팹 로드
            LoadPrefab(avatarSO, prefabLabel);
        }
        
        // 애니메이션 컨트롤러 로드 및 설정
        private void LoadAnimatorController(MonsterAvatarSO avatarSO, string prefabLabel)
        {
            // 이미 설정되어 있으면 건너뛰기
            if (avatarSO.AnimatorController != null)
            {
                return;
            }
            
            // 성공한 경로를 저장할 변수
            string successPath = "";
            
            // 가능한 경로 목록 생성
            List<string> possiblePaths = new List<string>
            {
                // 일반적인 패턴
                $"Assets/01_Animations/{prefabLabel}/{prefabLabel}.controller",
                $"Assets/01_Animations/{prefabLabel}/{prefabLabel}",
                $"Assets/Animations/{prefabLabel}/{prefabLabel}.controller",
                $"Assets/Animations/{prefabLabel}/{prefabLabel}",
                $"{prefabLabel}.controller",
                $"{prefabLabel}"
            };
            
            // 캐시에서 먼저 찾기
            AnimatorController foundController = null;
            foreach (var path in possiblePaths)
            {
                foundController = resourceManager.Load<AnimatorController>(path);
                if (foundController != null)
                {
                    successPath = path;  // 성공한 경로 저장
                    Debug.Log($"★★★ 성공! 캐시에서 애니메이션 컨트롤러 찾음: {path}, 이름: {foundController.name} ★★★");
                    break;
                }
            }
            
            // 캐시에서 찾지 못했으면 이름으로 찾기
            if (foundController == null)
            {
                // 이미 로드된 에셋 중에서 이름으로 찾기
                foreach (var kvp in resourceManager.Resources)
                {
                    if (kvp.Value is AnimatorController)
                    {
                        // 이름 또는 경로에 prefabLabel이 포함되어 있는지 확인
                        bool keyMatches = kvp.Key.IndexOf(prefabLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool nameMatches = kvp.Value.name.IndexOf(prefabLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (keyMatches || nameMatches)
                        {
                            foundController = kvp.Value as AnimatorController;
                            successPath = kvp.Key;  // 성공한 경로 저장
                            Debug.Log($"★★★ 성공! 이름 매칭으로 애니메이션 컨트롤러 찾음: {kvp.Key}, 이름: {kvp.Value.name} ★★★");
                            break;
                        }
                    }
                }
            }
            
            // 찾은 컨트롤러 설정
            if (foundController != null)
            {
                avatarSO.SetAnimatorController(foundController);
                
                // 성공한 경로를 에셋 경로 필드에 저장
                if (!string.IsNullOrEmpty(successPath))
                {
                    avatarSO.SetAnimatorControllerPath(successPath);
                }
            }
        }
        
        // 스프라이트 로드 및 설정
        private void LoadSprite(MonsterAvatarSO avatarSO, string prefabLabel)
        {
            // 이미 설정되어 있으면 건너뛰기
            if (avatarSO.CreatureSprite != null)
            {
                return;
            }
            
            // 성공한 경로를 저장할 변수
            string successPath = "";
            
            // 가능한 경로 목록 생성
            List<string> possiblePaths = new List<string>
            {
                // 일반적인 패턴
                $"Assets/02_Sprites/{prefabLabel}/{prefabLabel}",
                $"Assets/02_Sprites/{prefabLabel}_sprite",
                $"Assets/02_Sprites/{prefabLabel}",
                $"Assets/Sprites/{prefabLabel}/{prefabLabel}",
                $"Assets/Sprites/{prefabLabel}",
                $"Assets/Package/UI/Data_gameplay/{prefabLabel}",
                $"{prefabLabel}_sprite",
                $"{prefabLabel}_icon"
            };
            
            // 캐시에서 먼저 찾기
            Sprite foundSprite = null;
            foreach (var path in possiblePaths)
            {
                foundSprite = resourceManager.Load<Sprite>(path);
                if (foundSprite != null)
                {
                    successPath = path;  // 성공한 경로 저장
                    Debug.Log($"★★★ 성공! 캐시에서 스프라이트 찾음: {path}, 이름: {foundSprite.name} ★★★");
                    break;
                }
                
                // .sprite 접미사 추가 시도
                foundSprite = resourceManager.Load<Sprite>($"{path}");
                if (foundSprite != null)
                {
                    successPath = $"{path}.sprite";  // 성공한 경로 저장
                    Debug.Log($"★★★ 성공! 캐시에서 스프라이트 찾음: {path}.sprite, 이름: {foundSprite.name} ★★★");
                    break;
                }
            }
            
            // 캐시에서 찾지 못했으면 이름으로 찾기
            if (foundSprite == null)
            {
                // 이미 로드된 에셋 중에서 이름으로 찾기
                foreach (var kvp in resourceManager.Resources)
                {
                    // 스프라이트 직접 찾기
                    if (kvp.Value is Sprite)
                    {
                        // 이름 또는 경로에 prefabLabel이 포함되어 있는지 확인
                        bool keyMatches = kvp.Key.IndexOf(prefabLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool nameMatches = kvp.Value.name.IndexOf(prefabLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (keyMatches || nameMatches)
                        {
                            foundSprite = kvp.Value as Sprite;
                            successPath = kvp.Key;  // 성공한 경로 저장
                            Debug.Log($"★★★ 성공! 이름 매칭으로 스프라이트 찾음: {kvp.Key}, 이름: {kvp.Value.name} ★★★");
                            break;
                        }
                    }
                    
                    // Texture2D에서 스프라이트 생성 시도
                    if (foundSprite == null && kvp.Value is Texture2D)
                    {
                        // 이름 또는 경로에 prefabLabel이 포함되어 있는지 확인
                        bool keyMatches = kvp.Key.IndexOf(prefabLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool nameMatches = kvp.Value.name.IndexOf(prefabLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (keyMatches || nameMatches)
                        {
                            Texture2D texture = kvp.Value as Texture2D;
                            try
                            {
                                foundSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                                foundSprite.name = texture.name;
                                successPath = $"{kvp.Key}.sprite";  // 성공한 경로 저장
                                Debug.Log($"★★★ 성공! Texture2D에서 스프라이트 생성: {kvp.Key}, 이름: {texture.name} ★★★");
                                break;
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogError($"    - Texture2D에서 스프라이트 생성 실패: {e.Message}");
                            }
                        }
                    }
                }
            }
            
            // 찾은 스프라이트 설정
            if (foundSprite != null)
            {
                avatarSO.SetCreatureSprite(foundSprite);
                
                // 성공한 경로를 에셋 경로 필드에 저장
                if (!string.IsNullOrEmpty(successPath))
                {
                    avatarSO.SetSpritePath(successPath);
                }
            }
        }
        
        // 프리팹 로드 및 설정
        private void LoadPrefab(MonsterAvatarSO avatarSO, string prefabLabel)
        {
            // 이미 설정되어 있으면 건너뛰기
            if (avatarSO.CreaturePrefab != null)
            {
                return;
            }
            
            // 성공한 경로를 저장할 변수
            string successPath = "";
            
            // 가능한 경로 목록 생성
            List<string> possiblePaths = new List<string>
            {
                // 일반적인 패턴
                "Assets/02_Prefabs/Character/green_slime/green_slime.prefab",

                $"Assets/02_Prefabs/Character/{prefabLabel}/{prefabLabel}.prefab",
                $"Assets/02_Prefabs/Character/{prefabLabel}/{prefabLabel}",
                $"Assets/02_Prefabs/Character/{prefabLabel}",
                $"Assets/AddressableAssets/Prefabs/Character/{prefabLabel}",
                $"Assets/AddressableAssetsData/AssetGroups/Creature_Prefab.asset",
                $"Assets/02_Prefabs/Creature_Prefab/{prefabLabel}",
                $"Assets/02_Prefabs/Monsters/{prefabLabel}",
                $"Assets/02_Prefabs/{prefabLabel}",
                $"Assets/Prefabs/{prefabLabel}/{prefabLabel}",
                $"Assets/Prefabs/Monsters/{prefabLabel}",
                $"Assets/Prefabs/{prefabLabel}",
                $"Assets/AddressableAssetsData/AssetGroups/Creature_Prefab/{prefabLabel}",
                $"{prefabLabel}_prefab",
                $"{prefabLabel}.prefab",
                $"{prefabLabel}"
            };
            
            // 캐시에서 먼저 찾기
            GameObject foundPrefab = null;
            foreach (var path in possiblePaths)
            {
                foundPrefab = resourceManager.Load<GameObject>(path);
                if (foundPrefab != null)
                {
                    successPath = path;  // 성공한 경로 저장
                    Debug.Log($"★★★ 성공! 캐시에서 프리팹 찾음: {path}, 이름: {foundPrefab.name} ★★★");
                    break;
                }
            }
            
            // 캐시에서 찾지 못했으면 이름으로 찾기
            if (foundPrefab == null)
            {
                // 이미 로드된 에셋 중에서 이름으로 찾기
                foreach (var kvp in resourceManager.Resources)
                {
                    if (kvp.Value is GameObject)
                    {
                        // 이름 또는 경로에 prefabLabel이 포함되어 있는지 확인
                        bool keyMatches = kvp.Key.IndexOf(prefabLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool nameMatches = kvp.Value.name.IndexOf(prefabLabel, StringComparison.OrdinalIgnoreCase) >= 0;
                        
                        if (keyMatches || nameMatches)
                        {
                            foundPrefab = kvp.Value as GameObject;
                            successPath = kvp.Key;  // 성공한 경로 저장
                            Debug.Log($"★★★ 성공! 이름 매칭으로 프리팹 찾음: {kvp.Key}, 이름: {kvp.Value.name} ★★★");
                            break;
                        }
                    }
                }
            }
            
            // 찾은 프리팹 설정
            if (foundPrefab != null)
            {
                avatarSO.SetCreaturePrefab(foundPrefab);
                
                // 성공한 경로를 에셋 경로 필드에 저장
                if (!string.IsNullOrEmpty(successPath))
                {
                    avatarSO.SetPrefabPath(successPath);
                }
            }
        }
        
        // 몬스터 이름에서 키 추출 (예: "MonsterAvatar_GreenSlime" -> "green_slime")
        private string ExtractMonsterKeyFromName(string name)
        {
            // 이름 정규화 (소문자로 변환, 공백 및 특수문자 제거)
            string normalizedName = name.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
            
            Debug.Log($"[SOManager] 이름 정규화: {name} -> {normalizedName}");
            
            // 정규화된 이름으로 매칭
            if (normalizedName.Contains("greenslime"))
                return "green_slime";
            if (normalizedName.Contains("charwood"))
                return "char_wood";
            
            // 원본 이름에서 직접 매칭 시도
            if (name.Contains("green") && name.Contains("slime"))
                return "green_slime";
            if (name.Contains("char") && name.Contains("wood"))
                return "char_wood";
                
            // 기본값
            return name.ToLower();
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
        
        public MonsterAvatarSO GetMonsterAvatarSOById(int id)
        {
            if (monsterAvatarSOCache.TryGetValue(id, out MonsterAvatarSO avatar))
                return avatar;
                
            Debug.LogWarning($"[SOManager] ID {id}의 MonsterAvatarSO를 찾을 수 없습니다");
            return null;
        }
        // GUID로 몬스터 AvatarSO 가져오기
        public MonsterAvatarSO GetMonsterAvatarSOByGuid(Guid guid)
        {
            if (monsterAvatarSOByGuid.TryGetValue(guid, out MonsterAvatarSO avatar))
                return avatar;
                
            Debug.LogWarning($"[SOManager] GUID {guid}의 MonsterAvatarSO를 찾을 수 없습니다");
            return null;
        }
        
        // ID로 몬스터 StatsSO 가져오기
        public MonsterStatsSO GetMonsterStatsSOById(int id)
        {
            if (monsterStatsSOCache.TryGetValue(id, out MonsterStatsSO data))
                return data;
                
            Debug.LogWarning($"[SOManager] ID {id}의 MonsterStatsSO를 찾을 수 없습니다");
            return null;
        }
        public MonsterStatsSO GetMonsterStatsSOByGuid(Guid guid)
        {
            if (monsterDataSOByGuid.TryGetValue(guid, out MonsterStatsSO data))
                return data;
                
            Debug.LogWarning($"[SOManager] GUID {guid}의 MonsterStatsSO를 찾을 수 없습니다");
            return null;
        }
    }
} 