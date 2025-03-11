using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
// using Unity.Assets.Scripts.ConnectionManagement;


namespace Unity.Assets.Scripts.Network
{
class ClientConnectingState : OnlineState
{
    protected ConnectionMethodBase m_ConnectionMethod;

    public ClientConnectingState Configure(ConnectionMethodBase baseConnectionMethod)
    {
        m_ConnectionMethod = baseConnectionMethod;
        return this;
    }

    public override void Enter()
    {
#pragma warning disable 4014
        ConnectClientAsync();
#pragma warning restore 4014
    }

    public override void Exit()
    {
        Debug.Log("[ClientConnectingState] Exit 호출");
    }

    public override void OnClientConnected(ulong clientId)
    {
        // m_ConnectStatusPublisher.Publish(ConnectStatus.Success);
         Debug.Log("[ClientConnectingState] 로컬 클라이언트 연결 성공 - Connected 상태로 전환");
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_ClientConnected);
    }

    public override void OnClientDisconnect(ulong clientId)
    {
        // client ID is for sure ours here
        StartingClientFailed();
    }


    void StartingClientFailed()
    {
        var disconnectReason = m_ConnectionManager.NetworkManager.DisconnectReason;
        if (string.IsNullOrEmpty(disconnectReason))
        {
            // m_ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
        }
        else
        {
            var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
            // m_ConnectStatusPublisher.Publish(connectStatus);
        }
        m_ConnectionManager.ChangeState(m_ConnectionManager.m_Offline);
    }


    internal async Task ConnectClientAsync()
    {
        try
        {
            // Setup NGO with current connection method
            // await m_ConnectionMethod.SetupClientConnectionAsync();

            // NGO's StartClient launches everything
            if (!m_ConnectionManager.NetworkManager.StartClient())
            {
                throw new Exception("NetworkManager StartClient failed");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting client, see following exception");
            Debug.LogException(e);
            StartingClientFailed();
            throw;
        }
    }
}
}