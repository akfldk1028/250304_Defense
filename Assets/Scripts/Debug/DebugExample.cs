using UnityEngine;

/// <summary>
/// DebugManager와 DebugClassFacade 사용 예시
/// </summary>
public class DebugExample : MonoBehaviour
{
    private void Start()
    {
        // DebugManager 초기화 및 GUI 핸들러 생성
        DebugManager.Instance.CreateGUIHandler();
        
        // 디버그 모드 설정
        DebugManager.Instance.SetDebugMode(true);
        
        // GUI 표시 설정
        DebugManager.Instance.SetGUIVisible(true);
        
        // 디버그 텍스트 색상 설정
        DebugManager.Instance.SetDebugTextColor(Color.yellow);
        
        // 디버그 정보 업데이트
        DebugManager.Instance.UpdateDebugInfo("디버그 시작");
        
        // 로그 출력
        DebugManager.Instance.Log("일반 로그 메시지");
        DebugManager.Instance.Log("경고 메시지", LogType.Warning);
        DebugManager.Instance.Log("에러 메시지", LogType.Error);
        
        // DebugClassFacade 사용 예시
        
        // 특정 클래스 로깅 활성화 및 색상 설정
        DebugClassFacade.Instance.EnableClass("ResourceInstaller", Color.blue);
        DebugClassFacade.Instance.EnableClass(typeof(DebugExample), Color.green);
        
        // 로그 출력
        DebugClassFacade.Instance.LogInfo("ResourceInstaller", "리소스 로딩 시작");
        DebugClassFacade.Instance.LogWarning("ResourceInstaller", "리소스 로딩 지연");
        DebugClassFacade.Instance.LogError("ResourceInstaller", "리소스 로딩 실패");
        
        // 타입으로 로그 출력
        DebugClassFacade.Instance.Log(typeof(DebugExample), "디버그 예제 초기화 완료");
        
        // 특정 클래스 로깅 비활성화
        DebugClassFacade.Instance.DisableClass("ResourceInstaller");
        
        // 비활성화된 클래스 로그 출력 (출력되지 않음)
        DebugClassFacade.Instance.LogInfo("ResourceInstaller", "이 메시지는 출력되지 않음");
        
        // 클래스 색상 변경
        DebugClassFacade.Instance.EnableClass("ResourceInstaller", Color.cyan);
        DebugClassFacade.Instance.SetClassColor("ResourceInstaller", Color.magenta);
        
        // 다시 활성화된 클래스 로그 출력
        DebugClassFacade.Instance.LogInfo("ResourceInstaller", "색상이 변경된 로그");
        
        // 활성화된 클래스 목록 가져오기
        string[] enabledClasses = DebugClassFacade.Instance.GetEnabledClasses();
        string classesInfo = "활성화된 클래스: " + string.Join(", ", enabledClasses);
        Debug.Log(classesInfo);
    }
    
    private void Update()
    {
        // 키 입력에 따라 GUI 표시 여부 토글
        if (Input.GetKeyDown(KeyCode.G))
        {
            bool isVisible = DebugManager.Instance.IsGUIVisible();
            DebugManager.Instance.SetGUIVisible(!isVisible);
            Debug.Log($"GUI 표시: {!isVisible}");
        }
        
        // 키 입력에 따라 디버그 모드 토글
        if (Input.GetKeyDown(KeyCode.D))
        {
            bool isDebugMode = DebugManager.Instance.IsDebugMode();
            DebugManager.Instance.SetDebugMode(!isDebugMode);
            Debug.Log($"디버그 모드: {!isDebugMode}");
        }
    }
    
    private void OnDestroy()
    {
        // GUI 핸들러 제거
        DebugManager.Instance.DestroyGUIHandler();
    }
} 