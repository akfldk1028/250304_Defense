using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Assets.Scripts.Scene;
using VContainer;
using Unity.Assets.Scripts.Resource;
using Object = UnityEngine.Object;
using Unity.Netcode;
using Unity.Assets.Scripts.UI;
using Unity.Assets.Scripts.Objects;
using VContainer.Unity;
using System;

namespace Unity.Assets.Scripts.Scene
{
public class BasicGameScene : BaseScene
{
    [Inject] public MapSpawnerFacade _mapSpawnerFacade;
	[Inject] private ObjectManagerFacade _objectManagerFacade;
	// [Inject] private ServerMonster _serverMonster; // MonoBehaviour는 이런 방식으로 주입받을 수 없습니다.
	
    // VContainer.IObjectResolver 추가
    [Inject] private VContainer.IObjectResolver _container;
    [Inject] private BasicGameState _basicGameState;
	// greenslime 몬스터 ID
	public int MONSTER_ID = 202001;
	
	// 스폰된 몬스터 관리 리스트
	private List<ServerMonster> _spawnedMonsters = new List<ServerMonster>();

    // OnGridSpawned 이벤트가 너무 일찍 호출되는 것을 방지하기 위한 플래그
    private bool _isInitialized = false;
	
	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		SceneType = EScene.BasicGame;
        
        // 의존성 주입 확인 및 재시도
        if (_objectManagerFacade == null && _container != null)
        {
            Debug.Log("[BasicGameScene] ObjectManagerFacade 주입 시도");
            _objectManagerFacade = _container.Resolve<ObjectManagerFacade>();
        }
        
        SubscribeEvents();

        _basicGameState.Load();
		// 이벤트 구독 먼저 설정
				// ObjectManagerFacade 초기화
		// _objectManagerFacade.Awake();
		// _mapSpawnerFacade.Awake();

        _objectManagerFacade?.Load();
        _mapSpawnerFacade?.Load();
		
		// 그 다음 맵 로드
		_mapSpawnerFacade?.LoadMap();
        
        _isInitialized = true;

		return true;
	}
	


	public override void Clear()
	{
        // UI 이벤트 구독 해제
        UnsubscribeEvents();

	}

    private void OnEnable()
    {
        // UI 이벤트 구독
        SubscribeEvents();
    }

    private void OnDisable()
    {
        // UI 이벤트 구독 해제
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // 이미 구독되어 있는지 확인하기 위해 먼저 해제

        MapSpawnerFacade.GridSpawned += OnGridSpawned;
    }

    private void UnsubscribeEvents()
    {
        MapSpawnerFacade.GridSpawned -= OnGridSpawned;
        Debug.Log("[BasicGameScene] 이벤트 구독 해제#####################################");
    }


    private void OnGridSpawned()
    {
        // 초기화가 완료되지 않았으면 이벤트를 무시
        if (!_isInitialized)
        {
            Debug.LogWarning("[BasicGameScene] 초기화가 완료되지 않았습니다. GridSpawned 이벤트를 무시합니다.");
            return;
        }
        
        Debug.Log("[BasicGameScene] OnGridSpawned: 이동 경로 설정 완료 후 GridSpawned 이벤트 직접 호출");
        
        // 의존성을 한번 더 확인하고 null인 경우 컨테이너에서 재시도
        if (_objectManagerFacade == null)
        {
            Debug.LogWarning("[BasicGameScene] ObjectManagerFacade가 null입니다. 해결을 시도합니다.");
            
            if (_container != null)
            {
                try {
                    _objectManagerFacade = _container.Resolve<ObjectManagerFacade>();
                    Debug.Log("[BasicGameScene] ObjectManagerFacade 컨테이너 해결 시도: " + 
                        (_objectManagerFacade != null ? "성공" : "실패"));
                }
                catch (Exception ex) {
                    Debug.LogError($"[BasicGameScene] ObjectManagerFacade 해결 시도 중 오류: {ex.Message}");
                }
            }
            
            // 여전히 null이면 씬에서 찾기 시도
            if (_objectManagerFacade == null)
            {
                _objectManagerFacade = GameObject.FindObjectOfType<ObjectManagerFacade>();
                Debug.Log("[BasicGameScene] ObjectManagerFacade 씬에서 찾기 시도: " + 
                    (_objectManagerFacade != null ? "성공" : "실패"));
            }
            
            // 그래도 없으면 종료
            if (_objectManagerFacade == null)
            {
                Debug.LogError("[BasicGameScene] ObjectManagerFacade를 찾을 수 없습니다. 몬스터 스폰을 건너뜁니다.");
                return;
            }
        }
        
        try
        {
            // 몬스터 스폰 시도 전 MapSpawnerFacade가 제대로 설정되어 있는지 확인하면 좋을 것입니다
            Debug.Log("[BasicGameScene] 몬스터 스폰 시도 중...");
            _objectManagerFacade.Spawn_Monster(false, 202001);
            Debug.Log("[BasicGameScene] 몬스터 스폰 요청 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BasicGameScene] 몬스터 스폰 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }


}

}