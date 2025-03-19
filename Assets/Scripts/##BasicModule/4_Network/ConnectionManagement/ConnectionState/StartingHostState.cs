using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEditor.VersionControl;
using UnityEngine;
using VContainer;
// using Unity.Assets.Scripts.ConnectionManagement;


namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 호스트 시작 중인 상태를 나타내는 클래스
    /// 
    /// 이 상태에서는 로컬 플레이어가 호스트 모드로 게임을 시작하는 과정을 관리합니다.
    /// 호스트 시작 성공 시 HostingState로 전환하고, 실패 시 OfflineState로 전환합니다.
    /// </summary>
class StartingHostState : OnlineState
{

    ConnectionMethodBase m_ConnectionMethod;
    public StartingHostState Configure(ConnectionMethodBase baseConnectionMethod)
    {
        Debug.Log("[StartingHostState] Configure 호출");
        m_ConnectionMethod = baseConnectionMethod;
        return this;
    }

    public override void Enter()
    {
        Debug.Log("[StartingHostState] 시작: Enter");
        StartHost();
        Debug.Log("[StartingHostState] 종료: Enter");
    }

    public override void Exit() 
    { 
        Debug.Log("[StartingHostState] Exit 호출");
    }

    public override void OnServerStarted()
    {
        Debug.Log("[StartingHostState] 서버 시작됨");
        // m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
        Debug.Log("[StartingHostState] 연결 상태 발행: Success");
        //m_ConnectionManager.ChangeState(m_ConnectionManager.m_Hosting);
    }

    public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var connectionData = request.Payload;
        var clientId = request.ClientNetworkId;
        Debug.Log($"[StartingHostState] 연결 승인 검사: ClientID={clientId}");
        
        // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
        if (clientId == m_ConnectionManager.NetworkManager.LocalClientId)
        {
            Debug.Log("[StartingHostState] 로컬 호스트 자체 승인");
            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            Debug.Log($"[StartingHostState] 연결 페이로드: PlayerName={connectionPayload.playerName}");

            response.Approved = true;
            response.CreatePlayerObject = true;
            Debug.Log("[StartingHostState] 연결 승인 완료");
        }
    }

    public override void OnServerStopped()
    {
        Debug.Log("[StartingHostState] 서버 중지됨");
        StartHostFailed();
    }

    async void StartHost()
    {
        Debug.Log("[StartingHostState] 호스트 시작 시도");
        try
        {
            //await m_ConnectionMethod.SetupHostConnectionAsync();

            // NGO's StartHost launches everything
            if (!m_ConnectionManager.NetworkManager.StartHost())
            {
                Debug.LogError("[StartingHostState] 호스트 시작 실패");
                StartHostFailed();
            }
            else
            {
                Debug.Log("[StartingHostState] 호스트 시작 성공");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[StartingHostState] 호스트 시작 중 예외 발생: {e.Message}");
            StartHostFailed();
            throw;
        }
    }

    void StartHostFailed()
    {
        Debug.LogError("[StartingHostState] 호스트 시작 실패 처리");
        // m_ConnectStatusPublisher.Publish(ConnectStatus.StartHostFailed);
        Debug.Log("[StartingHostState] 연결 상태 발행: StartHostFailed");
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        Debug.Log("[StartingHostState] 상태 변경: Offline");
    }

}
}

