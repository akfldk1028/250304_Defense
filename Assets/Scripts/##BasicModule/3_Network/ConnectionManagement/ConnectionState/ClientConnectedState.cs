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

class ClientConnectedState : OnlineState
{
    public override void Enter()
    {
        Debug.Log("[ClientConnectedState] 시작: Enter");
        // m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
        Debug.Log("[ClientConnectedState] 연결 상태 발행: Success");
        Debug.Log("[ClientConnectedState] 종료: Enter");
    }

    public override void Exit()
    {
        Debug.Log("[ClientConnectedState] Exit 호출");
    }

    public override void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"[ClientConnectedState] OnClientDisconnect: ClientID={clientId}");
        if (clientId == m_ConnectionManager.NetworkManager.LocalClientId)
        {
            Debug.Log("[ClientConnectedState] 로컬 클라이언트 연결 해제 - 재연결 시도");
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientReconnecting);
        }
    }

    public override void OnServerStopped()
    {
        Debug.Log("[ClientConnectedState] OnServerStopped - 오프라인 상태로 전환");
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }

    public override void OnUserRequestedShutdown()
    {
        Debug.Log("[ClientConnectedState] 사용자 종료 요청");
        // m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }
}
}