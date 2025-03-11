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
        // 로딩 진행 상황을 알려주는 이벤트 (진행률 0.0f ~ 1.0f)
        public event Action<float> OnLoadingProgressChanged;
        
        // 로딩 완료를 알려주는 이벤트
        public event Action OnLoadingCompleted;

        [Inject]
        private ResourceManager _resourceManager;
        private ResourceInstaller _resourceInstaller;
        public Dictionary<int, Data.MonsterData> MonsterDic { get; private set; } = new Dictionary<int, Data.MonsterData>();

        // 총 로드할 데이터 수
        private int _totalDataCount = 1; // 현재는 MonsterData만 있으므로 1
        
        // 현재까지 로드된 데이터 수
        private int _loadedDataCount = 0;

        public void Start() // VContainer가 자동으로 호출
        {
            Init();
        }

	public void Init()
	{
        Debug.Log("Init 메서드 시작!");
		MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        
        // 로드된 MonsterDic 정보 로깅
        Debug.Log($"[DataLoader] MonsterDic 로드 완료: {MonsterDic}개 항목");
        
        // 각 몬스터 데이터 로깅
        
        foreach (var monster in MonsterDic)
        {
            Debug.Log($"[DataLoader] 몬스터 ID: {monster.Key}, 이름: {monster}");
            // [DataLoader] 몬스터 ID: 202001, 이름: [202001, Unity.Assets.Scripts.Data.MonsterData]
        }
	}

        private Loader LoadJson<Loader, Key, Value>(string fileName) where Loader : ILoader<Key, Value>
        {
            // 파일 이름만 받아서 전체 경로 조합
            string fullPath = "Data/JsonData/" + fileName;
            
            // Unity의 Resources.Load 직접 사용
            TextAsset textAsset = Resources.Load<TextAsset>(fullPath);
            
            if (textAsset == null)
            {
                Debug.LogError($"[DataLoader] 파일을 찾을 수 없습니다: {fullPath}");
                return default;
            }
            
            Debug.Log($"[DataLoader] 파일 로드 성공: {fullPath}, 크기: {textAsset.text.Length} 바이트");
            return JsonConvert.DeserializeObject<Loader>(textAsset.text);
        }
    }
}