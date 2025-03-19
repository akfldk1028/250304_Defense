using System;
using System.Threading.Tasks;

using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// ConnectionMethod는 NGO(Netcode for GameObjects)의 연결 설정을 담당하는 클래스입니다.
    /// 호스트와 클라이언트 모두의 연결 설정을 처리합니다.
    /// 새로운 전송 방식이나 연결 방법을 추가하려면 이 추상 클래스를 상속하세요.
    /// </summary>
    public abstract class ConnectionMethodBase
    {
        // protected ConnectionManager m_ConnectionManager;
        // readonly ProfileManager m_ProfileManager;
        // protected readonly string m_PlayerName;
        // protected const string k_DtlsConnType = "dtls";  // DTLS(Datagram Transport Layer Security) 연결 타입

        // public abstract Task SetupHostConnectionAsync();
        // public abstract Task SetupClientConnectionAsync();
        // public abstract Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync();

        // public ConnectionMethodBase(ConnectionManager connectionManager, ProfileManager profileManager, string playerName)
        // {
        //     m_ConnectionManager = connectionManager;
        //     m_ProfileManager = profileManager;
        //     m_PlayerName = playerName;
        //     Debug.Log($"[연결 방식] 생성됨 - 플레이어 이름: {playerName}");
        // }

        // protected void SetConnectionPayload(string playerId, string playerName)
        // {
        //     Debug.Log($"[연결 방식] 페이로드 설정 - 플레이어: {playerName}, ID: {playerId}");
        //     var payload = JsonUtility.ToJson(new ConnectionPayload()
        //     {
        //         playerId = playerId,
        //         playerName = playerName,
        //         isDebug = Debug.isDebugBuild
        //     });

        //     var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        //     m_ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        // }

        // protected string GetPlayerId()
        // {
        //     Debug.Log("[연결 방식] 플레이어 ID 조회");
        //     return "test";
        // }
    }




}
