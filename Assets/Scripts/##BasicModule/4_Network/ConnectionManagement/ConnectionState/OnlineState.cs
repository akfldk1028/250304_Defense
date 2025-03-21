using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 온라인 상태의 기본 클래스
    /// 
    /// 모든 온라인 상태(ClientConnectedState, HostingState 등)의 공통 기능을 제공합니다.
    /// 네트워크 연결이 활성화된 상태에서의 기본 동작을 정의합니다.
    /// NetworkBehaviour 기능을 활용하여 네트워크 상태를 관리합니다.
    /// </summary>
    public abstract class OnlineState : ConnectionState
    {
        protected const float k_ReconnectTimeout = 10f;
        protected float m_ReconnectStartTime;
        protected bool m_IsReconnecting;

        [Inject] protected DebugClassFacade m_DebugClassFacade;

        [Inject] protected ConnectionManager m_ConnectionManager;
        public override void OnUserRequestedShutdown()
        {
            m_DebugClassFacade?.LogInfo(GetType().Name, "[OnlineState] 사용자 종료 요청");
            PublishConnectStatus(ConnectStatus.UserRequestedDisconnect);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        public override void OnTransportFailure(ulong clientId)
        {
            m_DebugClassFacade?.LogWarning(GetType().Name, "[OnlineState] 네트워크 연결 실패");
            PublishConnectStatus(ConnectStatus.GenericDisconnect);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }

        protected virtual void StartReconnect()
        {
            m_IsReconnecting = true;
            m_ReconnectStartTime = Time.time;
            m_DebugClassFacade?.LogInfo(GetType().Name, "[OnlineState] 재연결 시작");
        }

        protected virtual void StopReconnect()
        {
            m_IsReconnecting = false;
            m_DebugClassFacade?.LogInfo(GetType().Name, "[OnlineState] 재연결 중단");
        }

        protected virtual void Update()
        {
            if (m_IsReconnecting && Time.time - m_ReconnectStartTime > k_ReconnectTimeout)
            {
                m_DebugClassFacade?.LogError(GetType().Name, "[OnlineState] 재연결 시간 초과");
                OnTransportFailure(0);
            }
        }
    }
}