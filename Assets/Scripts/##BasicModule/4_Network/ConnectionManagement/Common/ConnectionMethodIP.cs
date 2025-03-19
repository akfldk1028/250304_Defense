using System;
using System.Threading.Tasks;
using Unity.Netcode;
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
    /// IP 주소를 통한 연결 방식을 구현하는 클래스
    /// 
    /// 직접 IP 주소와 포트를 사용하여 호스트 및 클라이언트 연결을 설정합니다.
    /// </summary>
    // public class ConnectionMethodIP : ConnectionMethodBase
    // {
    //     private readonly string m_IpAddress;
    //     private readonly int m_Port;
    //     private readonly UnityTransport m_Transport;

    //     /// <summary>
    //     /// ConnectionMethodIP 생성자
    //     /// </summary>
    //     /// <param name="connectionManager">연결 관리자</param>
    //     /// <param name="profileManager">프로필 관리자 (사용하지 않는 경우 null)</param>
    //     /// <param name="playerName">플레이어 이름</param>
    //     /// <param name="ipAddress">연결할 IP 주소</param>
    //     /// <param name="port">연결할 포트</param>
    //     public ConnectionMethodIP(
    //         ConnectionManager connectionManager,
    //         ProfileManager profileManager,
    //         string playerName,
    //         string ipAddress,
    //         int port) : base(connectionManager, profileManager, playerName)
    //     {
    //         m_IpAddress = ipAddress;
    //         m_Port = port;
            
    //         // UnityTransport 컴포넌트 가져오기
    //         m_Transport = m_ConnectionManager.NetworkManager.GetComponent<UnityTransport>();
    //         if (m_Transport == null)
    //         {
    //             Debug.LogError("[ConnectionMethodIP] UnityTransport 컴포넌트를 찾을 수 없습니다!");
    //         }
            
    //         Debug.Log($"[ConnectionMethodIP] 생성됨 - IP: {ipAddress}, 포트: {port}");
    //     }

    //     /// <summary>
    //     /// 호스트 연결 설정
    //     /// 
    //     /// IP 주소와 포트를 사용하여 호스트 연결을 설정합니다.
    //     /// </summary>
    //     public override Task SetupHostConnectionAsync()
    //     {
    //         Debug.Log($"[ConnectionMethodIP] 호스트 연결 설정 - IP: {m_IpAddress}, 포트: {m_Port}");
            
    //         // 연결 페이로드 설정
    //         string playerId = GetPlayerId();
    //         SetConnectionPayload(playerId, m_PlayerName);
            
    //         // UnityTransport 설정
    //         if (m_Transport != null)
    //         {
    //             m_Transport.ConnectionData.Address = m_IpAddress;
    //             m_Transport.ConnectionData.Port = (ushort)m_Port;
    //             m_Transport.ConnectionData.ServerListenAddress = "0.0.0.0";
                
    //             Debug.Log($"[ConnectionMethodIP] UnityTransport 설정 완료 - 서버 주소: {m_IpAddress}:{m_Port}");
    //         }
    //         else
    //         {
    //             Debug.LogError("[ConnectionMethodIP] UnityTransport가 null입니다!");
    //         }
            
    //         return Task.CompletedTask;
    //     }

    //     /// <summary>
    //     /// 클라이언트 연결 설정
    //     /// 
    //     /// IP 주소와 포트를 사용하여 클라이언트 연결을 설정합니다.
    //     /// </summary>
    //     public override Task SetupClientConnectionAsync()
    //     {
    //         Debug.Log($"[ConnectionMethodIP] 클라이언트 연결 설정 - IP: {m_IpAddress}, 포트: {m_Port}");
            
    //         // 연결 페이로드 설정
    //         string playerId = GetPlayerId();
    //         SetConnectionPayload(playerId, m_PlayerName);
            
    //         // UnityTransport 설정
    //         if (m_Transport != null)
    //         {
    //             m_Transport.ConnectionData.Address = m_IpAddress;
    //             m_Transport.ConnectionData.Port = (ushort)m_Port;
                
    //             Debug.Log($"[ConnectionMethodIP] UnityTransport 설정 완료 - 서버 주소: {m_IpAddress}:{m_Port}");
    //         }
    //         else
    //         {
    //             Debug.LogError("[ConnectionMethodIP] UnityTransport가 null입니다!");
    //         }
            
    //         return Task.CompletedTask;
    //     }

    //     /// <summary>
    //     /// 클라이언트 재연결 설정
    //     /// 
    //     /// IP 주소와 포트를 사용하여 클라이언트 재연결을 설정합니다.
    //     /// </summary>
    //     public override Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
    //     {
    //         Debug.Log($"[ConnectionMethodIP] 클라이언트 재연결 설정 - IP: {m_IpAddress}, 포트: {m_Port}");
            
    //         // 연결 페이로드 설정
    //         string playerId = GetPlayerId();
    //         SetConnectionPayload(playerId, m_PlayerName);
            
    //         // UnityTransport 설정
    //         if (m_Transport != null)
    //         {
    //             m_Transport.ConnectionData.Address = m_IpAddress;
    //             m_Transport.ConnectionData.Port = (ushort)m_Port;
                
    //             Debug.Log($"[ConnectionMethodIP] UnityTransport 설정 완료 - 서버 주소: {m_IpAddress}:{m_Port}");
    //             return Task.FromResult((true, true));
    //         }
    //         else
    //         {
    //             Debug.LogError("[ConnectionMethodIP] UnityTransport가 null입니다!");
    //             return Task.FromResult((false, false));
    //         }
    //     }
    // }
}
