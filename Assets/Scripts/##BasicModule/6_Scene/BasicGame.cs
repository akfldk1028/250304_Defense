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

namespace Unity.Assets.Scripts.Scene
{
public class BasicGameScene : BaseScene
{
    [Inject] private MapSpawnerFacade _mapSpawnerFacade;
	[Inject] private ObjectManagerFacade _objectManagerFacade;
	// [Inject] private ServerMonster _serverMonster; // MonoBehaviour는 이런 방식으로 주입받을 수 없습니다.
	
	// greenslime 몬스터 ID
	public int MONSTER_ID = 202001;
	
	// 스폰된 몬스터 관리 리스트
	private List<ServerMonster> _spawnedMonsters = new List<ServerMonster>();
	
	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		SceneType = EScene.BasicGame;
        
        SubscribeEvents();

		// 이벤트 구독 먼저 설정
				// ObjectManagerFacade 초기화
		_objectManagerFacade.Awake();


		
		// 그 다음 맵 로드
		_mapSpawnerFacade.LoadMap("Spawner");

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
        UnsubscribeEvents();

        MapSpawnerFacade.GridSpawned += OnGridSpawned;
    }

    private void UnsubscribeEvents()
    {
        MapSpawnerFacade.GridSpawned -= OnGridSpawned;
        Debug.Log("[BasicGameScene] 이벤트 구독 해제");
    }


    private void OnGridSpawned()
    {
        Debug.Log("[ObjectManagerFacade] OnGridSpawned: 이동 경로 설정 완료 후 GridSpawned 이벤트 직접 호출");
        // _objectManagerFacade.Spawn_Monster(false, "Monster", 202001);
        _objectManagerFacade.Spawn_Monster(false, 202001);

    }


}

}