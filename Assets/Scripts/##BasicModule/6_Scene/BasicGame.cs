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
    [Inject] private SceneManagerEx _sceneManager;
    [Inject] private MapSpawnerFacade _mapSpawnerFacade;
	[Inject] private ObjectManagerFacade _objectManagerFacade;
	[Inject] private ResourceManager _resourceManager;
	[Inject] private GameManager _gameManager;
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
		_mapSpawnerFacade.LoadMap("Spawner");
		// _objectManagerFacade.LoadMap("green_slime");
		// 이벤트 구독 먼저 설정
		SubscribeEvents();


		return true;
	}
	


	public override void Clear()
	{
        // UI 이벤트 구독 해제
        UnsubscribeEvents();
		
		// 모든 몬스터 제거
		DespawnAllMonsters();
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
        // null 체크 추가
        if (_objectManagerFacade == null) return;
        
        // 몬스터 이벤트 구독
        // _objectManagerFacade.OnBossSpawned += OnBossSpawned;
        // _objectManagerFacade.OnMonsterSpawned += OnMonsterSpawned;
        
        // 게임 이벤트 구독
        // _gameManager.OnWaveChanged += OnWaveChanged;
    }

    private void UnsubscribeEvents()
    {
        // null 체크 추가
        if (_objectManagerFacade == null) return;
        
        // 몬스터 이벤트 구독 해제
        // _objectManagerFacade.OnBossSpawned -= OnBossSpawned;
        // _objectManagerFacade.OnMonsterSpawned -= OnMonsterSpawned;
        
        // 게임 이벤트 구독 해제
        // _gameManager.OnWaveChanged -= OnWaveChanged;
    }


	// 이벤트 핸들러
	private void OnBossSpawned(ServerMonster boss)
	{
		Debug.Log($"[BasicGameScene] 보스 몬스터 스폰: {boss.name}");
		// 보스 스폰 UI 표시 등
	}

	private void OnMonsterSpawned(ServerMonster monster)
	{
		// 필요한 경우 처리
		Debug.Log($"[BasicGameScene] 몬스터 스폰됨: {monster.name}");
	}

	private void OnWaveChanged(int newWave)
	{
		Debug.Log($"[BasicGameScene] 웨이브 변경: {newWave}");
		// 웨이브 변경 UI 표시 등
	}

	// 몬스터 풀 미리 생성

	
	// 한 번에 모든 몬스터 스폰 (10마리)
	private void SpawnAllMonsters(int count, string prefabName)
	{
		GameObject prefab = _resourceManager.Load<GameObject>(prefabName);

		// 이미 몬스터가 스폰되어 있으면 모두 제거
		if (_spawnedMonsters.Count > 0)
		{
			DespawnAllMonsters();
		}
		
		// 코루틴을 사용하여 시간 간격을 두고 몬스터 스폰
		StartCoroutine(SpawnMonstersWithDelay(count, prefabName));
		
		Debug.Log($"[BasicGameScene] {prefabName} 몬스터 스폰 시작");
	}
	
	// 시간 간격을 두고 몬스터를 스폰하는 코루틴
	private IEnumerator SpawnMonstersWithDelay(int count, string prefabName)
	{
		for (int i = 0; i < count; i++)
		{
			// ObjectManagerFacade를 통해 몬스터 스폰 (타입 인자 명시)
			// ServerMonster monster = _objectManagerFacade.SpawnMonster(MONSTER_ID, 1, prefabName);
			
			// // 스폰된 몬스터를 리스트에 추가
			// if (monster != null)
			// {
			// 	_spawnedMonsters.Add(monster);
			// }
			
			// Debug.Log($"[BasicGameScene] {prefabName} 몬스터 {i+1}/{count} 스폰됨");
			
			// 다음 몬스터 스폰 전에 대기
			yield return new WaitForSeconds(0.5f);
		}
		
		Debug.Log($"[BasicGameScene] {count}마리의 {prefabName} 몬스터 스폰 완료");
	}
	
	// 모든 몬스터 제거
	private void DespawnAllMonsters()
	{
		foreach (ServerMonster monster in _spawnedMonsters)
		{
			if (monster != null)
			{
				// ObjectManager를 통해 몬스터 제거
				// _objectManagerFacade.Despawn(monster);
			}
		}
		
		Debug.Log($"[BasicGameScene] {_spawnedMonsters.Count}마리의 몬스터를 제거했습니다.");
		_spawnedMonsters.Clear();
	}
}

}