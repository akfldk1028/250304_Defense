using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Unity.Assets.Scripts.Resource;
using Unity.Netcode;
using Unity.Assets.Scripts.Objects;

/// <summary>
/// 맵 로드 및 초기화를 담당하는 클래스입니다.
/// </summary>
public class MapSpawnerFacade : NetworkBehaviour
{
    [Inject] public ResourceManager _resourceManager;
    [Inject] private NetUtils _netUtils; // NetUtils 추가

    [Inject] private NetworkManager _networkManager;
    // MapManager는 더 이상 사용하지 않으므로 주입 제거
    // [Inject] private MapManager _mapManager;
    // [Inject] private ObjectManagerFacade _objectManagerFacade;
    // [Inject] private ObjectManagerFacade _objectManagerFacade;

    [SerializeField] private string _mapPrefabName;
    
    private GameObject _mapInstance;
    
    // 리스트를 모두 Public으로 변경
    [SerializeField] public List<Vector2> Player_move_list = new List<Vector2>();      // Host 이동 경로 포인트
    [SerializeField] public List<Vector2> Other_move_list = new List<Vector2>();    // Client 이동 경로 포인트
    [SerializeField] public List<Vector2> _hostSpawnPositions = new List<Vector2>();  // Host 스폰 위치
    [SerializeField] public List<Vector2> _clientSpawnPositions = new List<Vector2>(); // Client 스폰 위치
    [SerializeField] public List<bool> Player_spawn_list_Array = new List<bool>();         // Host 스폰 위치 사용 여부
    [SerializeField] public List<bool> Other_spawn_list_Array = new List<bool>();       // Client 스폰 위치 사용 여부
    
    private int[] Host_Client_Value_Index = new int[2];

    public static float xValue, yValue;

    public static event Action GridSpawned;

    private GameObject _mapSpawner;

    RateLimitCooldown m_RateLimitQuery;
    public void Awake(){}
    

 

    public void Initialize()
    {
        Debug.Log("[MapSpawnerFacade] Initialize 시작");
        // 자신의 게임 오브젝트를 _mapSpawner로 설정
    
    }
    
    public void Load()
    {
            _mapSpawner = this.gameObject;

    }




    /// <summary>
    /// 맵을 로드하고 초기화합니다.
    /// </summary>
        /// <param name="mapName">로드할 맵 이름</param>
    // public void LoadMap(string mapName)
    // {
 
        
    //     // GameObject mapPrefab = LoadMapPrefab(mapName);
    //     // InstantiateMap(mapPrefab, mapName);
    //     InstantiateMap(mapSpawner, mapName);

    // }
    public void LoadMap()
    {
        Debug.Log("[MapSpawnerFacade] LoadMap 시작");
        
        // _mapSpawner가 null인 경우 현재 게임 오브젝트로 설정
        if (_mapSpawner == null)
        {
            _mapSpawner = this.gameObject;
            Debug.Log("[MapSpawnerFacade] _mapSpawner를 자신의 게임 오브젝트로 설정");
        }
        
        // 그리드 시스템 초기화
        InitializeGridSystem(_mapSpawner);
        Debug.Log("[MapSpawnerFacade] LoadMap 완료");
    }


    
    private void InstantiateMap(GameObject mapPrefab, string mapName)
    {
        try
        {
            if (mapPrefab == null)
            {
                Debug.LogError("[MapSpawnerFacade] 맵 프리팹이 null입니다.");
                return;
            }
            
            Debug.Log($"[MapSpawnerFacade] InstantiateMap 시작: {mapName}");
            
            // 맵 인스턴스 설정
            ConfigureMapInstance();
            
            Debug.Log("[MapSpawnerFacade] InstantiateMap 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[MapSpawnerFacade] 맵 인스턴스화 중 예외 발생: {e.Message}\n{e.StackTrace}");
        }
    }
        #region 그리드 시스템

    private void InitializeGridSystem(GameObject mapInstance)
    {
        SetupSpawnGrids(mapInstance);
    }
    
    /// <summary>
    /// 스폰 그리드를 설정합니다.
    /// </summary>
    private void SetupSpawnGrids(GameObject mapInstance)
    {
        // 스폰 그리드 부모 찾기
        Transform playerGridParent = mapInstance.transform.Find("Spawner_Host");
        Transform enemyGridParent = mapInstance.transform.Find("Spawner_Client");


        if (playerGridParent == null || enemyGridParent == null)
        {
            Debug.LogWarning("[ObjectManagerFacade] 스폰 그리드 부모를 찾을 수 없습니다.");
            return;
        }
        
        Debug.Log($"<color=yellow>[ObjectManagerFacade] 그리드 설정 시작: {playerGridParent.name}, {enemyGridParent.name}</color>");
        // 그리드 초기화
        CreateSpawnGrid(playerGridParent, true);
        CreateSpawnGrid(enemyGridParent, false);

        Transform monsterGroundHost = playerGridParent.Find("MonsterGround_Host");
        Transform mosterGroundClient = enemyGridParent.Find("MonsterGround_Client");
        // 이동 경로 설정
        SetupMovePaths(monsterGroundHost, mosterGroundClient );
        
        Debug.Log("[ObjectManagerFacade] 그리드 설정 완료");
        
        

    }
    
    /// <summary>
    /// 스폰 그리드를 생성합니다.
    /// </summary>
    private void CreateSpawnGrid(Transform gridTransform, bool isPlayer)
    {
        // 그리드 크기 계산
        SpriteRenderer parentSprite = gridTransform.GetComponent<SpriteRenderer>();
        if (parentSprite == null)
        {
            Debug.LogError("[ObjectManagerFacade] 그리드 부모에 SpriteRenderer가 없습니다.");
            return;
        }
        

        float parentWidth = parentSprite.bounds.size.x;
        float parentHeight = parentSprite.bounds.size.y;
        Debug.Log($"<color=yellow>[ObjectManagerFacade] 스폰 그리드 생성 parentWidth : {parentWidth}, parentHeight : {parentHeight}, {isPlayer}</color>");

        // 그리드 셀 크기 계산 (6x3 그리드)
        float xCount = gridTransform.localScale.x / 6;
        float YCount = gridTransform.localScale.y / 3;


        xValue = xCount;
        yValue = YCount;


        // 그리드 셀 생성 (6x3 그리드)
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 6; col++)
            {
                // 셀 위치 계산
                float xPos = (-parentWidth / 2) + (col * xCount) + (xCount / 2);
                float yPos = ((isPlayer ? parentHeight : -parentHeight) / 2) + ((isPlayer ? -1 : 1) * (row * YCount)) + (YCount / 2);


                switch (isPlayer)
                {
                    case true:
                        _hostSpawnPositions.Add(new Vector2(
                            xPos,
                            yPos + gridTransform.localPosition.y - YCount));
                        Player_spawn_list_Array.Add(false);
                        break;
                    case false:
                        _clientSpawnPositions.Add(new Vector2(
                            xPos,
                            yPos + gridTransform.localPosition.y));
                        Other_spawn_list_Array.Add(false);
                        break;
                }

                // if (IsServer)
                // {
                //     StartCoroutine(DelayHeroHolderSpawn(isPlayer));
                // }
            }
        }
        Host_Client_Value_Index[0] = 0; //HOST
        Host_Client_Value_Index[1] = 0; //CLIENT
        GridSpawned?.Invoke();
        Debug.Log($"[ObjectManagerFacade] {(isPlayer ? "Host" : "Client")} 스폰 포인트 {_hostSpawnPositions.Count}개 설정 완료");
        
    }
    
    
    /// <summary>
    /// 이동 경로를 설정합니다.
    /// </summary>
     private void SetupMovePaths(Transform playerGridParent, Transform enemyGridParent)
    {
        Player_move_list.Clear();
        Other_move_list.Clear();
        
        // 자식 오브젝트들이 "Q_"로 시작하는지 확인하고 로깅
        for (int i = 0; i < playerGridParent.childCount; i++)
        {
            Transform child = playerGridParent.GetChild(i);
            // Q_ 로 시작하는 오브젝트만 경로 포인트로 사용
            Player_move_list.Add(child.position);
            Debug.Log($"[ObjectManagerFacade] 호스트 이동 경로 추가: {child.name} at {child.position}");
        }

        for (int i = 0; i < enemyGridParent.childCount; i++)
        {
            Transform child = enemyGridParent.GetChild(i);
            Other_move_list.Add(child.position);
            Debug.Log($"[ObjectManagerFacade] 클라이언트 이동 경로 추가: {child.name} at {child.position}");
        }
        
        // 경로가 정상적으로 설정되었는지 확인하고 이벤트 발생
        // if (Player_move_list.Count > 0 && Other_move_list.Count > 0)
        // {
        //    GridSpawned?.Invoke();
        // }
        // else
        // {
        //     Debug.LogError("[MapSpawnerFacade] ObjectManagerFacade가 유효하지 않습니다.");
        // }
        
     }
    
    
  

    #endregion

    /// <summary>
    /// 맵 인스턴스를 설정합니다.
    /// </summary>
    private void ConfigureMapInstance()
    {
        _mapSpawner.transform.localPosition = Vector3.zero;
        _mapSpawner.transform.localScale = new Vector3(1, 1, _mapSpawner.transform.localScale.z);
        Debug.Log($"[MapSpawnerFacade] 인스턴스화 성공: {_mapSpawner.name}");

    }
  
    /// <summary>
    /// 맵 인스턴스를 반환합니다.
    /// </summary>
    public GameObject GetMapInstance()
    {
        return _mapInstance;
    }
}
