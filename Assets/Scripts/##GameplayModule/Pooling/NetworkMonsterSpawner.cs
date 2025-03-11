using System.Collections.Generic;
// using Unity.Assets.Scripts.Gameplay.GameplayObjects.Monster;
using Unity.Assets.Scripts.Objects;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using System;  // GUID 사용을 위해 추가

namespace Unity.Assets.Scripts.Pooling
{
    /// <summary>
    /// 네트워크 멀티플레이어 게임에서 몬스터 생성 및 풀링을 관리하는 클래스입니다.
    /// </summary>
    public class NetworkMonsterSpawner : NetworkBehaviour
    {
        [Inject] private SOManager m_SOManager;
        
        [SerializeField] 
        private GameObject m_MonsterPrefab; // ServerMonster와 ClientMonster 컴포넌트가 모두 있는 프리팹
        
        // 몬스터 ID와 프리팹 매핑 캐시
        private Dictionary<int, GameObject> m_MonsterPrefabCache = new Dictionary<int, GameObject>();
        
        // 풀링을 위한 비활성화된 몬스터 저장소
        private Dictionary<int, Queue<NetworkObject>> m_MonsterPool = new Dictionary<int, Queue<NetworkObject>>();
        
        /// <summary>
        /// 몬스터 ID로 몬스터를 생성합니다.
        /// </summary>
        /// <param name="monsterId">몬스터 ID</param>
        /// <param name="position">생성 위치</param>
        /// <param name="rotation">생성 회전</param>
        /// <returns>생성된 몬스터 게임 오브젝트</returns>
        public GameObject SpawnMonster(Guid monsterId, Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Debug.LogWarning("SpawnMonster는 서버에서만 호출해야 합니다!");
                return null;
            }
            
            // 몬스터 아바타 데이터 가져오기
            MonsterAvatarSO monsterAvatar = m_SOManager.GetMonsterAvatarSOByGuid(monsterId);
            if (monsterAvatar == null)
            {
                Debug.LogError($"몬스터 ID {monsterId}에 해당하는 아바타를 찾을 수 없습니다!");
                return null;
            }
            
            return SpawnMonsterInternal(monsterAvatar, position, rotation);
        }
        
        /// <summary>
        /// 몬스터 아바타로 몬스터를 생성합니다.
        /// </summary>
        /// <param name="monsterAvatar">몬스터 아바타 SO</param>
        /// <param name="position">생성 위치</param>
        /// <param name="rotation">생성 회전</param>
        /// <returns>생성된 몬스터 게임 오브젝트</returns>
        public GameObject SpawnMonster(MonsterAvatarSO monsterAvatar, Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Debug.LogWarning("SpawnMonster는 서버에서만 호출해야 합니다!");
                return null;
            }
            
            if (monsterAvatar == null)
            {
                Debug.LogError("몬스터 아바타가 null입니다!");
                return null;
            }
            
            return SpawnMonsterInternal(monsterAvatar, position, rotation);
        }
        
        /// <summary>
        /// 몬스터를 생성하는 내부 메서드
        /// </summary>
        private GameObject SpawnMonsterInternal(MonsterAvatarSO monsterAvatar, Vector3 position, Quaternion rotation)
        {
            // MonsterData의 DataId를 가져옵니다
            int monsterId = monsterAvatar.MonsterData.DataId;
            
            // 풀에서 몬스터 가져오기 또는 새로 생성
            NetworkObject monsterNetObj = GetMonsterFromPool(monsterId);
            GameObject monsterObj;
            
            if (monsterNetObj == null)
            {
                // 새로운 몬스터 생성
                monsterObj = Instantiate(m_MonsterPrefab, position, rotation);
                monsterNetObj = monsterObj.GetComponent<NetworkObject>();
                
                // 네트워크에 스폰
                monsterNetObj.Spawn();
            }
            else
            {
                // 풀에서 가져온 몬스터 활성화
                monsterObj = monsterNetObj.gameObject;
                monsterObj.transform.position = position;
                monsterObj.transform.rotation = rotation;
                monsterObj.SetActive(true);
                
                // 네트워크에 스폰
                monsterNetObj.Spawn();
            }
            
            // 서버 몬스터 초기화
            ServerMonster serverMonster = monsterObj.GetComponent<ServerMonster>();
            serverMonster.Initialize(monsterAvatar);
            
            // 클라이언트 몬스터 초기화 (RPC를 통해)
            InitializeClientMonsterClientRpc(monsterNetObj, monsterId);
            
            // 몬스터 사망 이벤트 구독
            serverMonster.OnMonsterDeath += OnMonsterDeath;
            
            return monsterObj;
        }
        
        [ClientRpc]
        private void InitializeClientMonsterClientRpc(NetworkObjectReference monsterNetObjRef, int monsterId)
        {
            // 네트워크 오브젝트 참조에서 실제 오브젝트 가져오기
            if (monsterNetObjRef.TryGet(out NetworkObject monsterNetObj))
            {
                // 몬스터 아바타 데이터 가져오기
                MonsterAvatarSO monsterAvatar = m_SOManager.GetMonsterAvatarSOById(monsterId);
                
                if (monsterAvatar != null)
                {
                    // 클라이언트 몬스터 시각적 요소 초기화
                    ClientMonster clientMonster = monsterNetObj.GetComponent<ClientMonster>();
                    clientMonster.InitializeVisuals(monsterAvatar);
                }
                else
                {
                    Debug.LogError($"클라이언트: 몬스터 ID {monsterId}에 해당하는 아바타를 찾을 수 없습니다!");
                }
            }
        }
        
        /// <summary>
        /// 풀에서 몬스터를 가져옵니다.
        /// </summary>
        private NetworkObject GetMonsterFromPool(int monsterId)
        {
            // 해당 ID의 풀이 없으면 생성
            if (!m_MonsterPool.ContainsKey(monsterId))
            {
                m_MonsterPool[monsterId] = new Queue<NetworkObject>();
                return null;
            }
            
            // 풀이 비어있으면 null 반환
            if (m_MonsterPool[monsterId].Count == 0)
            {
                return null;
            }
            
            // 풀에서 몬스터 가져오기
            return m_MonsterPool[monsterId].Dequeue();
        }
        
        /// <summary>
        /// 몬스터 사망 이벤트 핸들러
        /// </summary>
        private void OnMonsterDeath(ServerMonster monster)
        {
            // 사망 이벤트 구독 해제
            monster.OnMonsterDeath -= OnMonsterDeath;
            
            // 일정 시간 후 풀로 반환
            StartCoroutine(ReturnToPoolAfterDelay(monster.gameObject, 2.0f));
        }
        
        /// <summary>
        /// 일정 시간 후 몬스터를 풀로 반환합니다.
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject monsterObj, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // 몬스터를 풀로 반환
            ReturnMonsterToPool(monsterObj);
        }
        
        /// <summary>
        /// 몬스터를 풀로 반환합니다.
        /// </summary>
        public void ReturnMonsterToPool(GameObject monsterObj)
        {
            if (!IsServer)
                return;
            
            ServerMonster serverMonster = monsterObj.GetComponent<ServerMonster>();
            int monsterId = serverMonster.MonsterId.Value;
            
            // 네트워크에서 디스폰 (파괴하지 않음)
            NetworkObject netObj = monsterObj.GetComponent<NetworkObject>();
            netObj.Despawn(false);
            
            // 오브젝트 비활성화
            monsterObj.SetActive(false);
            
            // 풀에 추가
            if (!m_MonsterPool.ContainsKey(monsterId))
            {
                m_MonsterPool[monsterId] = new Queue<NetworkObject>();
            }
            
            m_MonsterPool[monsterId].Enqueue(netObj);
        }
    }
}