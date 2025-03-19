using VContainer;
using Unity.Netcode;

namespace Unity.Assets.Scripts.Network
{
    /// <summary>
    /// 앱의 연결 모드를 정의하는 열거형
    /// </summary>
    public enum ConnectionMode
    {
        OfflineOnly,    // 오프라인 전용
        OnlineRequired, // 온라인 필수
        Hybrid          // 혼합 모드
    }
    /// <summary>
    /// 네트워크 연결 상태를 나타내는 열거형
    /// 클라이언트와 호스트의 다양한 연결 상태를 정의
    /// </summary>
    public enum ConnectStatus
    {
        Undefined,               // 초기 상태
        Success,                // 연결 성공
        ServerFull,            // 서버가 가득 참
        LoggedInAgain,         // 다른 곳에서 로그인됨
        UserRequestedDisconnect, // 사용자가 연결 종료 요청
        GenericDisconnect,     // 일반적인 연결 종료
        Reconnecting,          // 재연결 시도 중
        IncompatibleBuildType, // 빌드 타입 불일치
        HostEndedSession,      // 호스트가 세션 종료
        StartHostFailed,       // 호스트 시작 실패
        StartClientFailed      // 클라이언트 시작 실패
    }

    /// <summary>
    /// 재연결 시도 정보를 담는 구조체
    /// 현재 시도 횟수와 최대 시도 횟수를 포함
    /// </summary>
    public struct ReconnectMessage
    {
        public int CurrentAttempt;  // 현재 재연결 시도 횟수
        public int MaxAttempt;      // 최대 재연결 시도 횟수

        public ReconnectMessage(int currentAttempt, int maxAttempt)
        {
            CurrentAttempt = currentAttempt;
            MaxAttempt = maxAttempt;
        }
    }

    /// <summary>
    /// 연결 이벤트 메시지 구조체
    /// 네트워크로 직렬화 가능한 연결 상태 정보
    /// </summary>
    public struct ConnectionEventMessage : INetworkSerializeByMemcpy
    {
        public ConnectStatus ConnectStatus;  // 현재 연결 상태
    }

    
} 