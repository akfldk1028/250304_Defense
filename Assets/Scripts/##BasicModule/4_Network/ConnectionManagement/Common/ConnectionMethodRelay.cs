using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
  

    /// <summary>
    /// Unity Relay 서비스를 사용한 연결 설정
    /// Lobby 통합을 통해 P2P 연결을 제공합니다.
    /// </summary>
namespace Unity.Assets.Scripts.Network
{

    // class ConnectionMethodRelay : ConnectionMethodBase
    // {
    //     LobbyServiceFacade m_LobbyServiceFacade;
    //     LocalLobby m_LocalLobby;

    //     public ConnectionMethodRelay(LobbyServiceFacade lobbyServiceFacade, LocalLobby localLobby, ConnectionManager connectionManager, ProfileManager profileManager, string playerName)
    //         : base(connectionManager, profileManager, playerName)
    //     {
    //         m_LobbyServiceFacade = lobbyServiceFacade;
    //         m_LocalLobby = localLobby;
    //         m_ConnectionManager = connectionManager;
    //         Debug.Log("[릴레이 연결] 생성됨");
    //     }

    //     public override async Task SetupClientConnectionAsync()
    //     {
    //         Debug.Log("[릴레이 연결] Unity 릴레이 클라이언트 설정");

    //         SetConnectionPayload(GetPlayerId(), m_PlayerName);

    //         if (m_LobbyServiceFacade.CurrentUnityLobby == null)
    //         {
    //             Debug.LogError("[릴레이 연결] 로비가 설정되지 않은 상태에서 릴레이 시작 시도");
    //             throw new Exception("로비가 설정되지 않은 상태에서 릴레이 시작 시도");
    //         }

    //         Debug.Log($"[릴레이 연결] Unity 릴레이 클라이언트 설정 - 참가 코드: {m_LocalLobby.RelayJoinCode}");

    //         // 참가 코드로 클라이언트 할당 생성
    //         //var joinedAllocation = await RelayService.Instance.JoinAllocationAsync(m_LocalLobby.RelayJoinCode);
    //         //Debug.Log($"[릴레이 연결] 클라이언트 연결됨 - 클라이언트: {joinedAllocation.ConnectionData[0]} {joinedAllocation.ConnectionData[1]}, " +
    //         //    $"호스트: {joinedAllocation.HostConnectionData[0]} {joinedAllocation.HostConnectionData[1]}, " +
    //         //    $"할당 ID: {joinedAllocation.AllocationId}");

    //         //await m_LobbyServiceFacade.UpdatePlayerDataAsync(joinedAllocation.AllocationId.ToString(), m_LocalLobby.RelayJoinCode);

    //         //// UTP에 할당 정보 설정
    //         //var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
    //         //utp.SetRelayServerData(new RelayServerData(joinedAllocation, k_DtlsConnType));
    //         //Debug.Log("[릴레이 연결] 클라이언트 설정 완료");
    //     }

    //     public override async Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
    //     {
    //         Debug.Log("[릴레이 연결] 릴레이 재연결 시도");

    //         if (m_LobbyServiceFacade.CurrentUnityLobby == null)
    //         {
    //             Debug.LogError("[릴레이 연결] 로비가 더 이상 존재하지 않음, 재연결 시도 중단");
    //             return (false, false);
    //         }

    //         var lobby = await m_LobbyServiceFacade.ReconnectToLobbyAsync();
    //         var success = lobby != null;
    //         Debug.Log($"[릴레이 연결] 재연결 {(success ? "성공" : "실패")}");
    //         return (success, true);
    //     }

    //     public override async Task SetupHostConnectionAsync()
    //     {
    //         Debug.Log("[릴레이 연결] Unity 릴레이 호스트 설정");

    //         SetConnectionPayload(GetPlayerId(), m_PlayerName);

    //         // 릴레이 할당 생성
    //         //Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(m_ConnectionManager.MaxConnectedPlayers, region: null);
    //         //var joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

    //         //Debug.Log($"[릴레이 연결] 호스트 서버 데이터 - 연결: {hostAllocation.ConnectionData[0]} {hostAllocation.ConnectionData[1]}, " +
    //         //    $"할당 ID: {hostAllocation.AllocationId}, 지역: {hostAllocation.Region}");

    //         //m_LocalLobby.RelayJoinCode = joinCode;

    //         //// 로비와 릴레이 서비스 통합 활성화
    //         //await m_LobbyServiceFacade.UpdateLobbyDataAndUnlockAsync();
    //         //await m_LobbyServiceFacade.UpdatePlayerDataAsync(hostAllocation.AllocationIdBytes.ToString(), joinCode);

    //         //// UTP에 릴레이 연결 정보 설정
    //         //var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
    //         //utp.SetRelayServerData(new RelayServerData(hostAllocation, k_DtlsConnType));

    //         Debug.Log($"[릴레이 연결] 호스트 설정 완료 - 참가 코드: {m_LocalLobby.RelayJoinCode}");
    //     }
    // }

}
   
