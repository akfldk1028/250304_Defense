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
    /// 호스트 모드로 실행 중인 상태를 나타내는 클래스
    /// 
    /// 이 상태에서는 로컬 플레이어가 서버와 클라이언트 역할을 모두 수행합니다.
    /// 다른 클라이언트의 연결 요청을 처리하고, 게임 세션을 관리합니다.
    /// NetworkBehaviour 기능을 활용하여 서버 역할을 수행합니다.
    /// </summary>
class HostingState : OnlineState
{
    //[Inject]
    //LobbyServiceFacade m_LobbyServiceFacade;
    // [Inject]
    // IPublisher<ConnectionEventMessage> m_ConnectionEventPublisher;

    // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
    const int k_MaxConnectPayload = 1024;

    public override void Enter()
    {
        Debug.Log("[HostingState] 시작: Enter");
        // m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
        Debug.Log("[HostingState] 연결 상태 발행: Success");
        Debug.Log("[HostingState] 종료: Enter");
    }

    public override void Exit()
    {
        Debug.Log("[HostingState] Exit 호출");
    }

    public override void OnClientConnected(ulong clientId)
    {
        //var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
        //if (playerData != null)
        //{
        //    m_ConnectionEventPublisher.Publish(new ConnectionEventMessage() { ConnectStatus = ConnectStatus.Success, PlayerName = playerData.Value.PlayerName });
        //}
        //else
        //{
        //    // This should not happen since player data is assigned during connection approval
        //    Debug.LogError($"No player data associated with client {clientId}");
        //    var reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
        //    m_ConnectionManager.NetworkManager.DisconnectClient(clientId, reason);
        //}

    }

    public override void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"[HostingState] 클라이언트 연결 해제: ClientID={clientId}");
        if (clientId == m_ConnectionManager.NetworkManager.LocalClientId)
        {
            Debug.Log("[HostingState] 호스트(로컬) 연결 해제 - 오프라인으로 전환");
            m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
        }
    }

    public override void OnUserRequestedShutdown()
    {
        Debug.Log("[HostingState] 사용자 종료 요청");
        var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
        m_ConnectionManager.NetworkManager.Shutdown(true);
        // m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }

    public override void OnServerStopped()
    {
        // m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }

    /// <summary>
    /// This logic plugs into the "ConnectionApprovalResponse" exposed by Netcode.NetworkManager. It is run every time a client connects to us.
    /// The complementary logic that runs when the client starts its connection can be found in ClientConnectingState.
    /// </summary>
    /// <remarks>
    /// Multiple things can be done here, some asynchronously. For example, it could authenticate your user against an auth service like UGS' auth service. It can
    /// also send custom messages to connecting users before they receive their connection result (this is useful to set status messages client side
    /// when connection is refused, for example).
    /// Note on authentication: It's usually harder to justify having authentication in a client hosted game's connection approval. Since the host can't be trusted,
    /// clients shouldn't send it private authentication tokens you'd usually send to a dedicated server.
    /// </remarks>
    /// <param name="request"> The initial request contains, among other things, binary data passed into StartClient. In our case, this is the client's GUID,
    /// which is a unique identifier for their install of the game that persists across app restarts.
    ///  <param name="response"> Our response to the approval process. In case of connection refusal with custom return message, we delay using the Pending field.
    public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log($"[HostingState] 연결 승인 검사: ClientID={request.ClientNetworkId}");
        var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        var connectionData = JsonUtility.FromJson<ConnectionPayload>(payload);
        
        if (m_ConnectionManager.NetworkManager.ConnectedClientsIds.Count >= m_ConnectionManager.MaxConnectedPlayers)
        {
            Debug.Log("[HostingState] 연결 거부: 서버가 가득 참");
            response.Approved = false;
            response.Reason = JsonUtility.ToJson(ConnectStatus.ServerFull);
            return;
        }

        Debug.Log($"[HostingState] 연결 승인: PlayerName={connectionData.playerName}");
        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    public override void OnTransportFailure()
    {
        Debug.Log("[HostingState] 전송 실패 발생");
        // m_ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }
}
}