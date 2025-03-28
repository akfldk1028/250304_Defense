using System.Collections;

using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
// using Action = Unity.Assets.Scripts.Gameplay.Actions.Action;
using System.Linq;
using System;
using VContainer;
// using Unity.Assets.Scripts.Pooling;
using Unity.VisualScripting;
using static Define;

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// Contains all NetworkVariables, RPCs and server-side logic of a character.
    /// This class was separated in two to keep client and server context self contained. This way you don't have to continuously ask yourself if code is running client or server side.
    /// 
    /// 디펜스 게임에서의 역할:
    /// 1. 선택된 유닛/타워의 서버 측 로직 처리
    /// 2. 네트워크를 통한 유닛/타워 상태 동기화
    /// 3. 클라이언트의 명령(이동, 공격, 스킬 등)을 서버에서 처리
    /// 4. 타워/유닛의 생명주기 관리 (생성, 파괴, 업그레이드 등)
    /// </summary>
    // [RequireComponent(typeof(NetworkHealthState),
    //     typeof(NetworkLifeState),
    //     typeof(NetworkAvatarGuidState))]
    public abstract class BaseObject : NetworkBehaviour
    {

        [Inject] public ObjectManager _objectManager;
        // // ITargetable 인터페이스 구현
        // private bool _isNpc = false;

        // /// <summary>
        // /// 이 객체가 NPC인지 여부를 나타냅니다.
        // /// </summary>
        // public bool IsNpc 
        // { 
        //     get { return _isNpc; } 
        //     set { _isNpc = value; }
        // }

        // private bool _isValidTarget = true;

        // /// <summary>
        // /// 이 객체가 현재 유효한 타겟인지 여부를 나타냅니다.
        // /// </summary>
        // public bool IsValidTarget
        // {
        //     get { return _isValidTarget; }
        //     set { _isValidTarget = value; }
        // }

        // [SerializeField]
        [Tooltip("Setting negative value disables destroying object after it is killed.")]
        private float m_KilledDestroyDelaySeconds = 3.0f;

       
        /// <summary>
        /// 캐릭터의 스텔스 상태를 관리
        /// 디펜스 게임에서: 특정 유닛의 은폐/투명 상태를 관리
        /// </summary>
        public NetworkVariable<bool> IsStealthy { get; } = new NetworkVariable<bool>();


        public NetworkVariable<ulong> TargetId { get; } = new NetworkVariable<ulong>();



        /// <summary>
        /// 캐릭터의 생명 상태를 관리
        /// 디펜스 게임에서: 타워나 유닛의 파괴/생존 상태를 관
        /// </summary>
        // public NetworkLifeState NetLifeState { get; private set; }

        /// <summary>
        /// Current LifeState. Only Players should enter the FAINTED state.
        /// </summary>
        // public LifeState LifeState
        // {
        //     get => NetLifeState.LifeState.Value;
        //     private set => NetLifeState.LifeState.Value = value;
        // }

        /// <summary>
        /// Returns true if this Character is an NPC.
        /// </summary>
        // public bool IsNpc => GetCreatureStatsSO() != null && GetCreatureStatsSO().IsNpc;


  


        public int ExtraCells { get; set; } = 0;

	    public EObjectType ObjectType { get;  set; } = EObjectType.None;
        public CircleCollider2D Collider { get; private set; }
        public Rigidbody2D RigidBody { get; private set; }
        // private HurtFlashEffect HurtFlash;

        public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }
        public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }

        public int DataTemplateID { get; set; }

        bool _lookLeft = true;
        public bool LookLeft
        {
            get { return _lookLeft; }
            set
            {
                _lookLeft = value;
                // Flip(!value);
            }
        }

        protected void Awake()
        {
            // ObjectType = EObjectType.Creature;
            // m_ServerActionPlayer = new ServerActionPlayer(this);
            // NetLifeState = GetComponent<NetworkLifeState>();
            // NetHealthState = GetComponent<NetworkHealthState>();
            // m_State = GetComponent<NetworkAvatarGuidState>();

            // CreatureStatsSO는 SetCreatureGuid에서 찾기 때문에 여기서는 호출하지 않음
            // FindCreatureStatsSO();
            // Init();
        }

        private void SetObjectType(EObjectType objectType)
        {
            throw new NotImplementedException();
        }

        protected virtual void UpdateAnimation()
        {
        }
        public  virtual bool Init()
        {


            Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
            RigidBody = GetComponent<Rigidbody2D>();

            return true;
        }

        protected virtual void OnDisable()
        {
 
        }

        public void LookAtTarget(BaseObject target)
        {
            Vector2 dir = target.transform.position - transform.position;
            if (dir.x < 0)
                LookLeft = true;
            else
                LookLeft = false;
        }



        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            // if (IsServer)
            // {
            //     NetLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
            //     m_DamageReceiver.DamageReceived -= ReceiveHP;
            //     m_DamageReceiver.CollisionEntered -= CollisionEntered;
            // }
        }
        public void AddAnimation(int trackIndex, string AnimName, bool loop, float delay)
        {

        }
	public virtual void OnAnimEventHandler()
	{
		Debug.Log("OnAnimEventHandler");
	}
	

	#region Map
	public bool LerpCellPosCompleted { get; protected set; }

	Vector3Int _cellPos;
	public Vector3Int CellPos
	{
		get { return _cellPos; }
		protected set
		{
			_cellPos = value;
			LerpCellPosCompleted = false;
		}
	}

	public void SetCellPos(Vector3Int cellPos, bool forceMove = false)
	{
		CellPos = cellPos;
		LerpCellPosCompleted = false;

		// if (forceMove)
		// {
		// 	transform.position = Managers.Map.Cell2World(CellPos);
		// 	LerpCellPosCompleted = true;
		// }
	}

	public void LerpToCellPos(float moveSpeed)
	{
		// if (LerpCellPosCompleted)
		// 	return;

		// Vector3 destPos = Managers.Map.Cell2World(CellPos);
		// Vector3 dir = destPos - transform.position;

		// if (dir.x < 0)
		// 	LookLeft = true;
		// else
		// 	LookLeft = false;

		// if (dir.magnitude < 0.01f)
		// {
		// 	transform.position = destPos;
		// 	LerpCellPosCompleted = true;
		// 	return;
		// }

		// float moveDist = Mathf.Min(dir.magnitude, moveSpeed * Time.deltaTime);
		// transform.position += dir.normalized * moveDist;
	}
	#endregion

        /// <summary>
        /// RPC to send inputs for this character from a client to a server.
        /// </summary>
        /// <param name="movementTarget">The position which this character should move towards.</param>
        [Rpc(SendTo.Server)]
        public void ServerSendCharacterInputRpc(Vector3 movementTarget)
        {
            // if (LifeState == LifeState.Alive && !m_Movement.IsPerformingForcedMovement())
            // {
            //     // if we're currently playing an interruptible action, interrupt it!
            //     if (m_ServerActionPlayer.GetActiveActionInfo(out ActionRequestData data))
            //     {
            //         if (GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).Config.ActionInterruptible)
            //         {
            //             m_ServerActionPlayer.ClearActions(false);
            //         }
            //     }

            //     m_ServerActionPlayer.CancelRunningActionsByLogic(ActionLogic.Target, true); //clear target on move.
            //     m_Movement.SetMovementTarget(movementTarget);
            // }
        }

 

        // UTILITY AND SPECIAL-PURPOSE RPCs

        /// <summary>
        /// Called on server when the character's client decides they have stopped "charging up" an attack.
        /// </summary>
        [Rpc(SendTo.Server)]
        public void ServerStopChargingUpRpc()
        {
            // m_ServerActionPlayer.OnGameplayActivity(Action.GameplayActivity.StoppedChargingUp);
        }


    }
}
