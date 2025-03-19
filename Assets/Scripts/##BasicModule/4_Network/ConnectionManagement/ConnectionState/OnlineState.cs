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
abstract class OnlineState : ConnectionState
{
    public override void OnUserRequestedShutdown()
    {
        Debug.Log("[OnlineState] 사용자 종료 요청");

        // m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);

        Debug.Log("[OnlineState] 연결 상태 발행: UserRequestedDisconnect");

        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);

        Debug.Log("[OnlineState] 상태 변경: Offline");
    }

    public override void OnTransportFailure()
    {
        // This behaviour will be the same for every online state
        Debug.Log("[OnlineState] 연결 상태 발행: GenericDisconnect");

        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);

        Debug.Log("[OnlineState] 상태 변경: Offline");
    }
}
}