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
    /// 연결 상태를 나타내는 추상 클래스
    /// 상태 패턴의 기본 클래스로, 모든 연결 상태는 이 클래스를 상속받음
    /// 
    /// 이 클래스는 NetworkBehaviour를 직접 상속하지 않지만, ConnectionManager를 통해
    /// 간접적으로 NetworkBehaviour의 RPC 기능을 사용합니다.
    /// 
    /// 각 상태 클래스는 이 클래스를 상속하여 특정 연결 상태에서의 동작을 구현합니다.
    /// 상태 전환은 ConnectionManager.ChangeState() 메서드를 통해 이루어집니다.
    /// </summary>
    abstract class ConnectionState
    {
        [Inject]
        protected ConnectionManager m_ConnectionManager;

        // [Inject]
        // protected IPublisher<ConnectStatus> m_ConnectStatusPublisher;

        /// <summary>
        /// 상태 진입 시 호출되는 메서드
        /// 각 상태 클래스는 이 메서드를 구현하여 상태 진입 시 필요한 작업을 수행합니다.
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// 상태 종료 시 호출되는 메서드
        /// 각 상태 클래스는 이 메서드를 구현하여 상태 종료 시 필요한 정리 작업을 수행합니다.
        /// </summary>
        public abstract void Exit();

        /// <summary>
        /// 클라이언트 연결 시 호출되는 메서드
        /// </summary>
        public virtual void OnClientConnected(ulong clientId) { }
        
        /// <summary>
        /// 클라이언트 연결 해제 시 호출되는 메서드
        /// </summary>
        public virtual void OnClientDisconnect(ulong clientId) { }

        /// <summary>
        /// 서버 시작 시 호출되는 메서드
        /// </summary>
        public virtual void OnServerStarted() { }

        /// <summary>
        /// IP 주소를 통한 클라이언트 연결 시작 메서드
        /// </summary>
        public virtual void StartClientIP(string playerName, string ipaddress, int port) { }

        /// <summary>
        /// 로비를 통한 클라이언트 연결 시작 메서드
        /// </summary>
        public virtual void StartClientLobby(string playerName) { }

        /// <summary>
        /// IP 주소를 통한 호스트 시작 메서드
        /// </summary>
        public virtual void StartHostIP(string playerName, string ipaddress, int port) { }

        /// <summary>
        /// 로비를 통한 호스트 시작 메서드
        /// </summary>
        public virtual void StartHostLobby(string playerName) { }

        /// <summary>
        /// 사용자가 종료 요청 시 호출되는 메서드
        /// </summary>
        public virtual void OnUserRequestedShutdown() { }

        /// <summary>
        /// 연결 승인 검사 메서드
        /// </summary>
        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }

        /// <summary>
        /// 전송 실패 시 호출되는 메서드
        /// </summary>
        public virtual void OnTransportFailure() { }

        /// <summary>
        /// 서버 중지 시 호출되는 메서드
        /// </summary>
        public virtual void OnServerStopped() { }

        /// <summary>
        /// 릴레이 연결 시작 메서드
        /// </summary>
        public virtual void StartRelayConnection() { }


    }
}