using System;
// using Unity.Assets.Scripts.Gameplay.Configuration;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Assets.Scripts.Objects
{
    //TODO Inject 에서 anmationController 로 가져와야함함
    public class ServerAnimationHandler : NetworkBehaviour
    {
        [SerializeField]
        NetworkAnimator m_NetworkAnimator;

        // [SerializeField]
        // VisualizationConfiguration m_VisualizationConfiguration;

        [SerializeField]
        NetworkLifeState m_NetworkLifeState;

        public NetworkAnimator NetworkAnimator => m_NetworkAnimator;

        public override void OnNetworkSpawn()
        {
            // if (IsServer)
            // {
            //     m_NetworkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
            // }
        }

        void OnLifeStateChanged(LifeState previousValue, LifeState newValue)
        {
            switch (newValue)
            {
                // case LifeState.Alive:
                //     NetworkAnimator.SetTrigger(m_VisualizationConfiguration.AliveStateTriggerID);
                //     break;
                // case LifeState.Fainted:
                //     NetworkAnimator.SetTrigger(m_VisualizationConfiguration.FaintedStateTriggerID);
                //     break;
                // case LifeState.Dead:
                //     NetworkAnimator.SetTrigger(m_VisualizationConfiguration.DeadStateTriggerID);
                //     break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && m_NetworkLifeState != null)
            {
                m_NetworkLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
            }
        }
    }
}
