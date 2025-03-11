using System;
using System.Collections;
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
    /// 클라이언트가 서버와의 연결이 끊어진 후 재연결을 시도하는 상태를 나타내는 클래스
    /// 
    /// 이 상태에서는 클라이언트가 서버와의 연결을 복구하기 위해 여러 번 재연결을 시도합니다.
    /// NetworkBehaviour 기능을 활용하여 서버와의 재연결을 처리합니다.
    /// </summary>
class ClientReconnectingState : ClientConnectingState
{
    // [Inject]
    // IPublisher<ReconnectMessage> m_ReconnectMessagePublisher;

    Coroutine m_ReconnectCoroutine;
    int m_NbAttempts;

    const float k_TimeBeforeFirstAttempt = 1;
    const float k_TimeBetweenAttempts = 5;

    public override void Enter()
    {
        Debug.Log("[ClientReconnectingState] 시작: Enter");
        m_NbAttempts = 0;
        Debug.Log($"[ClientReconnectingState] 재연결 시도 #{m_NbAttempts + 1}/{m_ConnectionManager.NbReconnectAttempts}");
        // m_ReconnectMessagePublisher.Publish(new ReconnectMessage(m_NbAttempts, m_ConnectionManager.NbReconnectAttempts));
        AttemptReconnect();
        Debug.Log("[ClientReconnectingState] 종료: Enter");
    }

    public override void Exit()
    {
        Debug.Log("[ClientReconnectingState] Exit 호출");
    }

    private void AttemptReconnect()
    {
        if (m_NbAttempts < m_ConnectionManager.NbReconnectAttempts)
        {
            Debug.Log($"[ClientReconnectingState] 재연결 시도 #{m_NbAttempts + 1} 시작");
            if (m_ConnectionManager.NetworkManager.StartClient())
            {
                Debug.Log("[ClientReconnectingState] 클라이언트 시작 성공");
            }
            else
            {
                Debug.LogError("[ClientReconnectingState] 클라이언트 시작 실패");
                OnClientDisconnect(0);
            }
        }
        else
        {
            Debug.Log("[ClientReconnectingState] 최대 재시도 횟수 초과 - 오프라인으로 전환");
            // m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }

    public override void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[ClientReconnectingState] 클라이언트 연결됨: ClientID={clientId}");
        if (clientId == m_ConnectionManager.NetworkManager.LocalClientId)
        {
            Debug.Log("[ClientReconnectingState] 재연결 성공 - Connected 상태로 전환");
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
        }
    }

    public override void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"[ClientReconnectingState] 클라이언트 연결 해제: ClientID={clientId}");
        m_NbAttempts++;
        if (m_NbAttempts < m_ConnectionManager.NbReconnectAttempts)
        {
            Debug.Log($"[ClientReconnectingState] 재연결 재시도 #{m_NbAttempts + 1}/{m_ConnectionManager.NbReconnectAttempts}");
            // m_ReconnectMessagePublisher.Publish(new ReconnectMessage(m_NbAttempts, m_ConnectionManager.NbReconnectAttempts));
            AttemptReconnect();
        }
        else
        {
            Debug.Log("[ClientReconnectingState] 최대 재시도 횟수 초과 - 오프라인으로 전환");
            // m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }

    public override void OnTransportFailure()
    {
        Debug.LogError("[ClientReconnectingState] 전송 실패 발생");
        // m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }
}
}