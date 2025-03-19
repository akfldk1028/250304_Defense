using VContainer;
using VContainer.Unity;
// using Unity.Assets.Scripts.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Assets.Scripts.Data;
using Mono.Cecil;
using Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers;
using System;
using System.Threading.Tasks;
using Unity.Assets.Scripts.Resource;

namespace Unity.Assets.Scripts.Data
{

    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }
    
    public class DataLoader : IStartable
    {

        [Inject] private DebugClassFacade _debugClassFacade;
        [Inject] private ResourceManager _resourceManager;

        private bool _isInitialized = false;
        
        // 초기화 완료 이벤트 추가
        public event Action OnInitialized;
        
        // 초기화 상태 확인 속성
        public bool IsInitialized => _isInitialized;

        public Dictionary<int, Data.MonsterData> MonsterDic { get; private set; } = new Dictionary<int, Data.MonsterData>();
        public Dictionary<int, Data.HeroData> HeroDic { get; private set; } = new Dictionary<int, Data.HeroData>();
        public Dictionary<int, Data.SkillData> SkillDic { get; private set; } = new Dictionary<int, Data.SkillData>();
        public Dictionary<int, Data.ProjectileData> ProjectileDic { get; private set; } = new Dictionary<int, Data.ProjectileData>();
        public Dictionary<int, Data.EnvData> EnvDic { get; private set; } = new Dictionary<int, Data.EnvData>();
        public Dictionary<int, Data.EffectData> EffectDic { get; private set; } = new Dictionary<int, Data.EffectData>();
        public Dictionary<int, Data.AoEData> AoEDic { get; private set; } = new Dictionary<int, Data.AoEData>();
        public static DataLoader instance;



        public void Start() // VContainer가 자동으로 호출
        {
            instance = this;
            // ResourceManager의 로딩 완료 이벤트에 구독
            _resourceManager.OnLoadingCompleted += OnResourceLoadingCompleted;
            
            // 이미 리소스 로딩이 완료되었는지 확인
            // 리소스가 이미 로드되어 있다면 바로 초기화
            if (_resourceManager.Resources.Count > 0)
            {
                Debug.Log("[DataLoader] 리소스가 이미 로드되어 있습니다. 바로 초기화합니다.");
                Init();
            }
            else
            {
                Debug.Log("[DataLoader] ResourceManager의 리소스 로딩 완료를 기다립니다.");
            }
        }
        
        private void OnResourceLoadingCompleted()
        {            
            // 이미 초기화되었는지 확인
            if (_isInitialized)
            {
                Debug.Log("[DataLoader] 이미 초기화되었습니다. 중복 초기화를 방지합니다.");
                return;
            }
            Init();
            _resourceManager.OnLoadingCompleted -= OnResourceLoadingCompleted;
        }

        public void Init()
        {
            Debug.Log("[DataLoader] Init 메서드 시작!");
            
            // 이미 초기화되었는지 확인
            if (_isInitialized)
            {
                Debug.Log("[DataLoader] 이미 초기화되었습니다. 중복 초기화를 방지합니다.");
                return;
            }
            
 
            MonsterDic = LoadJsonToResoureManager<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();

            _isInitialized = true;
            
            // 초기화 완료 이벤트 발생
            OnInitialized?.Invoke();
        }


        private Loader LoadJsonToResoureManager<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
        {
            TextAsset textAsset = _resourceManager.LoadJson<TextAsset>(path);
            return JsonConvert.DeserializeObject<Loader>(textAsset.text);
        }



    }
}