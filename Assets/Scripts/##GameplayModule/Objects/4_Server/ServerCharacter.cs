using System.Collections;

using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
// using Action = Unity.Assets.Scripts.Gameplay.Actions.Action;
using System.Linq;
using System;
using VContainer;

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
    public class ServerCharacter : NetworkBehaviour, ITargetable
    {
        // [FormerlySerializedAs("m_ClientVisualization")]
        // [SerializeField]
        // ClientCharacter m_ClientCharacter;

        // public ClientCharacter clientCharacter => m_ClientCharacter;

        // [SerializeField]
        // CharacterClass m_CharacterClass;

        // public CharacterClass CharacterClass
        // {
        //     get
        //     {
        //         if (m_CharacterClass == null)
        //         {
        //             m_CharacterClass = m_State.RegisteredAvatar.CharacterClass;
        //         }

        //         return m_CharacterClass;
        //     }
        // }


        // [SerializeField]
        protected Guid creatureGuid; // 캐릭터의 GUID
        
        [Inject]
        private SOManager m_SOManager; //이거사실  ServerMonster.cs 로 가야할듯듯
        
        private CreatureStatsSO m_CreatureStatsSO;
        
        [SerializeField]
        [Tooltip("Setting negative value disables destroying object after it is killed.")]
        private float m_KilledDestroyDelaySeconds = 3.0f;

        /// <summary>
        /// 캐릭터의 스텔스 상태를 관리
        /// 디펜스 게임에서: 특정 유닛의 은폐/투명 상태를 관리
        /// </summary>
        public NetworkVariable<bool> IsStealthy { get; } = new NetworkVariable<bool>();

        /// <summary>
        /// 캐릭터의 현재 타겟 ID를 관리
        /// 디펜스 게임에서: 타워나 유닛이 공격하는 대상을 관리
        /// </summary>
        public NetworkVariable<ulong> TargetId { get; } = new NetworkVariable<ulong>();

        // public NetworkVariable<int> HitPoints { get; } = new NetworkVariable<int>();

        // public NetworkVariable<int> MaxHitPoints { get; } = new NetworkVariable<int>();

        // public NetworkDamageReceiver DamageReceiver => m_DamageReceiver;

        // [SerializeField]
        // NetworkDamageReceiver m_DamageReceiver;

        // public NetworkHealthState NetHealthState { get; private set; }

        // public ActionType AIActionType
        // {
        //     get
        //     {
        //         if (m_AIActionType == ActionType.None)
        //         {
        //             m_AIActionType = CharacterClass.DefaultAIActionType;
        //         }
        //         return m_AIActionType;
        //     }
        // }

        // private ActionType m_AIActionType = ActionType.None;

        /// <summary>
        /// 캐릭터의 생명 상태를 관리
        /// 디펜스 게임에서: 타워나 유닛의 파괴/생존 상태를 관리
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
        public bool IsNpc => GetCreatureStatsSO() != null && GetCreatureStatsSO().IsNpc;

        // public bool IsValidTarget => LifeState != LifeState.Dead;

        /// <summary>
        /// Returns true if the Character is currently in a state where it can play actions, false otherwise.
        /// </summary>
        // public bool CanPerformActions => LifeState == LifeState.Alive;

        /// <summary>
        /// Character Type. This value is populated during character selection.
        /// </summary>
        // public CharacterTypeEnum CharacterType => CharacterClass.CharacterType;

        /// <summary>
        /// 서버 측 액션 처리기
        /// 디펜스 게임에서: 타워 건설, 유닛 스킬 사용, 업그레이드 등의 액션을 처리
        /// </summary>
        // private ServerActionPlayer m_ServerActionPlayer;

        // [SerializeField]
        // ActionDescription m_StartingAction;

        // [SerializeField]
        // ServerCharacterMovement m_Movement;

        // public ServerCharacterMovement Movement => m_Movement;

        // [SerializeField]
        // PhysicsWrapper m_PhysicsWrapper;

        // public PhysicsWrapper physicsWrapper => m_PhysicsWrapper;

        // [SerializeField]
        // ServerAnimationHandler m_ServerAnimationHandler;

        // public ServerAnimationHandler serverAnimationHandler => m_ServerAnimationHandler;

        // // private AIBrain m_AIBrain;
        // NetworkAvatarGuidState m_State;



        protected void Awake()
        {
            // m_ServerActionPlayer = new ServerActionPlayer(this);
            // NetLifeState = GetComponent<NetworkLifeState>();
            // NetHealthState = GetComponent<NetworkHealthState>();
            // m_State = GetComponent<NetworkAvatarGuidState>();
            
            // CreatureStatsSO는 SetCreatureGuid에서 찾기 때문에 여기서는 호출하지 않음
            // FindCreatureStatsSO();
        }


        /// <summary>
        /// 캐릭터의 GUID를 설정합니다.
        /// </summary>
        /// <param name="guid">설정할 GUID</param>
        protected void SetCreatureGuid(Guid guid)
        {
            creatureGuid = guid;
            // GUID가 변경되었으므로 CreatureStatsSO를 다시 찾습니다.
            FindCreatureStatsSO();
        }

        
        private void FindCreatureStatsSO()
        {
            if (creatureGuid != Guid.Empty && m_SOManager != null)
            {
                m_CreatureStatsSO = m_SOManager.GetMonsterStatsSOByGuid(creatureGuid);
                
                if (m_CreatureStatsSO == null)
                {
                    Debug.LogWarning($"[ServerCharacter] GUID {creatureGuid}에 해당하는 CreatureStatsSO를 찾을 수 없습니다.");
                }
                else
                {
                    Debug.Log($"[ServerCharacter] GUID {creatureGuid}에 해당하는 CreatureStatsSO를 찾았습니다: {m_CreatureStatsSO.name}");
                }
            }
            else
            {
                if (creatureGuid == Guid.Empty)
                {
                    Debug.LogWarning("[ServerCharacter] creatureGuid가 설정되지 않았습니다.");
                }
                
                if (m_SOManager == null)
                {
                    Debug.LogWarning("[ServerCharacter] SOManager가 주입되지 않았습니다.");
                }
            }
        }
        
        public CreatureStatsSO GetCreatureStatsSO()
        {
            if (m_CreatureStatsSO == null && m_SOManager != null)
            {
                FindCreatureStatsSO();
            }
            return m_CreatureStatsSO;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // creatureGuid가 설정되어 있고 m_CreatureStatsSO가 null인 경우에만 다시 시도
            if (m_CreatureStatsSO == null && creatureGuid != Guid.Empty)
            {
                FindCreatureStatsSO();
            }
        }

        public override void OnNetworkDespawn()
        {
            // if (IsServer)
            // {
            //     NetLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
            //     m_DamageReceiver.DamageReceived -= ReceiveHP;
            //     m_DamageReceiver.CollisionEntered -= CollisionEntered;
            // }
        }

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

        // ACTION SYSTEM

        /// <summary>
        /// Client->Server RPC that sends a request to play an action.
        /// </summary>
        /// <param name="data">Data about which action to play and its associated details. </param>
        // [Rpc(SendTo.Server)]
        // public void ServerPlayActionRpc(ActionRequestData data)
        // {
        //     ActionRequestData data1 = data;
        //     if (!GameDataSource.Instance.GetActionPrototypeByID(data1.ActionID).Config.IsFriendly)
        //     {
        //         // notify running actions that we're using a new attack. (e.g. so Stealth can cancel itself)
        //         ActionPlayer.OnGameplayActivity(Action.GameplayActivity.UsingAttackAction);
        //     }

        //     PlayAction(ref data1);
        // }

        // UTILITY AND SPECIAL-PURPOSE RPCs

        /// <summary>
        /// Called on server when the character's client decides they have stopped "charging up" an attack.
        /// </summary>
        [Rpc(SendTo.Server)]
        public void ServerStopChargingUpRpc()
        {
            // m_ServerActionPlayer.OnGameplayActivity(Action.GameplayActivity.StoppedChargingUp);
        }

        void InitializeHitPoints()
        {
            // HitPoints = CharacterClass.BaseHP.Value;

            // if (!IsNpc)
            // {
            //     SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
            //     if (sessionPlayerData is { HasCharacterSpawned: true })
            //     {
            //         HitPoints = sessionPlayerData.Value.CurrentHitPoints;
            //         if (HitPoints <= 0)
            //         {
            //             LifeState = LifeState.Fainted;
            //         }
            //     }
            // }
        }

        /// <summary>
        /// 액션 실행 메서드
        /// 디펜스 게임에서: 
        /// - 타워: 건설, 업그레이드, 판매, 특수 능력 사용
        /// - 유닛: 공격, 스킬 사용, 이동 등
        /// </summary>
        // public void PlayAction(ref ActionRequestData action)
        // {
            //the character needs to be alive in order to be able to play actions
            // if (LifeState == LifeState.Alive && !m_Movement.IsPerformingForcedMovement())
            // {
            //     if (action.CancelMovement)
            //     {
            //         m_Movement.CancelMove();
            //     }

            //     m_ServerActionPlayer.PlayAction(ref action);
            // }
        // }

        // void OnLifeStateChanged(LifeState prevLifeState, LifeState lifeState)
        // {
            // if (lifeState != LifeState.Alive)
            // {
            //     m_ServerActionPlayer.ClearActions(true);
            //     m_Movement.CancelMove();
            // }
        // }

        IEnumerator KilledDestroyProcess()
        {
            yield return new WaitForSeconds(m_KilledDestroyDelaySeconds);

            if (NetworkObject != null)
            {
                NetworkObject.Despawn(true);
            }
        }

        /// <summary>
        /// Receive an HP change from somewhere. Could be healing or damage.
        /// </summary>
        /// <param name="inflicter">Person dishing out this damage/healing. Can be null. </param>
        /// <param name="HP">The HP to receive. Positive value is healing. Negative is damage.  </param>
        void ReceiveHP(ServerCharacter inflicter, int HP)
        {
            //to our own effects, and modify the damage or healing as appropriate. But in this game, we just take it straight.
//             if (HP > 0)
//             {
//                 m_ServerActionPlayer.OnGameplayActivity(Action.GameplayActivity.Healed);
//                 float healingMod = m_ServerActionPlayer.GetBuffedValue(Action.BuffableValue.PercentHealingReceived);
//                 HP = (int)(HP * healingMod);
//             }
//             else
//             {
// #if UNITY_EDITOR || DEVELOPMENT_BUILD
//                 // Don't apply damage if god mode is on
//                 if (NetLifeState.IsGodMode.Value)
//                 {
//                     return;
//                 }
// #endif

//                 m_ServerActionPlayer.OnGameplayActivity(Action.GameplayActivity.AttackedByEnemy);
//                 float damageMod = m_ServerActionPlayer.GetBuffedValue(Action.BuffableValue.PercentDamageReceived);
//                 HP = (int)(HP * damageMod);

//                 serverAnimationHandler.NetworkAnimator.SetTrigger("HitReact1");
//             }

//             HitPoints = Mathf.Clamp(HitPoints + HP, 0, CharacterClass.BaseHP.Value);

//             if (m_AIBrain != null)
//             {
//                 //let the brain know about the modified amount of damage we received.
//                 m_AIBrain.ReceiveHP(inflicter, HP);
//             }

//             //we can't currently heal a dead character back to Alive state.
//             //that's handled by a separate function.
//             if (HitPoints <= 0)
//             {
//                 if (IsNpc)
//                 {
//                     if (m_KilledDestroyDelaySeconds >= 0.0f && LifeState != LifeState.Dead)
//                     {
//                         StartCoroutine(KilledDestroyProcess());
//                     }

//                     LifeState = LifeState.Dead;
//                 }
//                 else
//                 {
//                     LifeState = LifeState.Fainted;
//                 }

//                 m_ServerActionPlayer.ClearActions(false);
//             }
        }

        /// <summary>
        /// Determines a gameplay variable for this character. The value is determined
        /// by the character's active Actions.
        /// </summary>
        /// <param name="buffType"></param>
        /// <returns></returns>
        // public float GetBuffedValue(Action.BuffableValue buffType)
        // {
        //     return m_ServerActionPlayer.GetBuffedValue(buffType);
        // }

        /// <summary>
        /// Receive a Life State change that brings Fainted characters back to Alive state.
        /// </summary>
        /// <param name="inflicter">Person reviving the character.</param>
        /// <param name="HP">The HP to set to a newly revived character.</param>
        // public void Revive(ServerCharacter inflicter, int HP)
        // {
        //     if (LifeState == LifeState.Fainted)
        //     {
        //         HitPoints = Mathf.Clamp(HP, 0, CharacterClass.BaseHP.Value);
        //         NetLifeState.LifeState.Value = LifeState.Alive;
        //     }
        // }

        // void Update()
        // {
        //     m_ServerActionPlayer.OnUpdate();
        //     if (m_AIBrain != null && LifeState == LifeState.Alive && m_BrainEnabled)
        //     {
        //         m_AIBrain.Update();
        //     }
        // }

        // void CollisionEntered(Collision collision)
        // {
        //     if (m_ServerActionPlayer != null)
        //     {
        //         m_ServerActionPlayer.CollisionEntered(collision);
        //     }
        // }

        /// <summary>
        /// This character's AIBrain. Will be null if this is not an NPC.
        /// </summary>
        // public AIBrain AIBrain { get { return m_AIBrain; } }



    }
}
