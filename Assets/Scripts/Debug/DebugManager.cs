using System;
using UnityEngine;

/// <summary>
/// 디버깅 관련 기능을 제공하는 싱글톤 매니저 클래스
/// </summary>
public class DebugManager : MonoBehaviour
{
    private static DebugManager s_Instance;
    public static DebugManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                GameObject go = new GameObject("@DebugManager");
                s_Instance = go.AddComponent<DebugManager>();
                DontDestroyOnLoad(go);
            }
            return s_Instance;
        }
    }
    
    // 디버그 모드 설정
    [SerializeField] private bool _debugMode = true;
    [SerializeField] private Color _debugTextColor = Color.yellow;
    private string _debugInfo = "";
    
    // 디버그 로그 이벤트
    public event Action<string> OnDebugLogUpdated;
    
    private void Awake()
    {
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        s_Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// 디버그 모드 활성화 여부 설정
    /// </summary>
    public void SetDebugMode(bool isActive)
    {
        _debugMode = isActive;
    }
    
    /// <summary>
    /// 디버그 텍스트 색상 설정
    /// </summary>
    public void SetDebugTextColor(Color color)
    {
        _debugTextColor = color;
    }
    
    /// <summary>
    /// 디버그 정보 업데이트
    /// </summary>
    public void UpdateDebugInfo(string info, bool logToConsole = true)
    {
        if (!_debugMode) return;
        
        _debugInfo = info;
        OnDebugLogUpdated?.Invoke(info);
        
        if (logToConsole)
        {
            Debug.Log($"[DebugManager] {info}");
        }
    }
    
    /// <summary>
    /// 디버그 정보 가져오기
    /// </summary>
    public string GetDebugInfo()
    {
        return _debugInfo;
    }
    
    /// <summary>
    /// 디버그 텍스트 색상 가져오기
    /// </summary>
    public Color GetDebugTextColor()
    {
        return _debugTextColor;
    }
    
    /// <summary>
    /// 디버그 모드 여부 확인
    /// </summary>
    public bool IsDebugMode()
    {
        return _debugMode;
    }
    
    /// <summary>
    /// 디버그 정보를 화면에 표시
    /// </summary>
    private void OnGUI()
    {
        if (!_debugMode) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = _debugTextColor;
        style.wordWrap = true;
        
        GUI.Label(new Rect(10, 10, Screen.width - 20, 200), _debugInfo, style);
    }
    
    /// <summary>
    /// 디버그 로그 출력
    /// </summary>
    public void Log(string message, LogType logType = LogType.Log)
    {
        if (!_debugMode) return;
        
        switch (logType)
        {
            case LogType.Log:
                Debug.Log($"[DebugManager] {message}");
                break;
            case LogType.Warning:
                Debug.LogWarning($"[DebugManager] {message}");
                break;
            case LogType.Error:
                Debug.LogError($"[DebugManager] {message}");
                break;
        }
    }
} 