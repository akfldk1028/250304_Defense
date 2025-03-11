using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Unity.Assets.Scripts.UI;

public class UI_Scene : UI_Base
{
    [Inject] private UIManager _uiManager;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        _uiManager.SetCanvas(gameObject, false);
        return true;
    }

    /*
    ┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐
    │                                        UI_Scene 함수 요약 표                                                 │
    ├───────────────────────────┬───────────────────────────────────┬───────────────────────────────────────────┤
    │         함수 이름          │               입력                  │                  출력                     │
    ├───────────────────────────┼───────────────────────────────────┼───────────────────────────────────────────┤
    │ GetChild                  │ parent, childName                 │ GameObject (직접 자식만)                   │
    │ ShowChild                 │ parent, childName                 │ void (자식 활성화)                         │
    │ HideChild                 │ parent, childName                 │ void (자식 비활성화)                       │
    │ GetAllChildren            │ parent                           │ List<GameObject> (모든 직접 자식)           │
    │ LogChildren               │ parent                           │ void (자식 계층 로그 출력)                  │
    │ GetComponent<T>           │ parent, childName                 │ T (직접 자식의 컴포넌트)                   │
    │ GetChildByPath            │ parent, path                      │ GameObject (경로로 자식 찾기)              │
    │ GetComponentByPath<T>     │ parent, path                      │ T (경로로 자식의 컴포넌트 찾기)            │
    │ GetComponents<T>          │ parent, childName                 │ T[] (직접 자식의 모든 컴포넌트)            │
    │ GetComponentsInChildren<T>│ parent, includeInactive          │ List<T> (모든 자식의 컴포넌트)             │
    │ FindChildrenWithTag       │ parent, tag                       │ List<GameObject> (태그로 자식 찾기)        │
    │ FindChildrenByNamePattern │ parent, namePattern               │ List<GameObject> (이름 패턴으로 자식 찾기) │
    │ FindChildDeep             │ parent, childName, includeInactive│ GameObject (깊이 상관없이 이름으로 찾기)   │
    │ FindComponentDeep<T>      │ parent, childName, includeInactive│ T (깊이 상관없이 컴포넌트 찾기)           │
    │ FindComponentsDeep<T>     │ parent, includeInactive          │ T[] (모든 자식의 특정 타입 컴포넌트)       │
    └───────────────────────────┴───────────────────────────────────┴───────────────────────────────────────────┘
    
    ※ 함수 선택 가이드:
    1. 직접 자식만 찾을 때: GetChild, GetComponent<T>
    2. 경로로 찾을 때(예: "Panel/Button"): GetChildByPath, GetComponentByPath<T>
    3. 깊이 상관없이 이름으로 찾을 때: FindChildDeep, FindComponentDeep<T>
    4. 여러 컴포넌트 찾을 때: GetComponents<T>, GetComponentsInChildren<T>, FindComponentsDeep<T>
    */

    // UI_Scene.cs에 추가할 공통 메서드들

    /// <summary>
    /// 특정 이름의 하위 요소를 가져옵니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">하위 요소의 이름</param>
    /// <returns>찾은 하위 요소, 없으면 null</returns>
    protected GameObject GetChild(GameObject parent, string childName)
    {
        return UIDebugLogger.GetChild(parent, childName);
    }

    /// <summary>
    /// 특정 이름의 하위 요소를 활성화합니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">하위 요소의 이름</param>
    protected void ShowChild(GameObject parent, string childName)
    {
        GameObject child = GetChild(parent, childName);
        if (child != null)
        {
            child.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] '{childName}' 하위 요소를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 특정 이름의 하위 요소를 비활성화합니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">하위 요소의 이름</param>
    protected void HideChild(GameObject parent, string childName)
    {
        GameObject child = GetChild(parent, childName);
        if (child != null)
        {
            child.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] '{childName}' 하위 요소를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// GameObject의 모든 하위 요소를 가져옵니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <returns>하위 요소 목록</returns>
    protected List<GameObject> GetAllChildren(GameObject parent)
    {
        return UIDebugLogger.GetAllChildren(parent);
    }

    /// <summary>
    /// GameObject의 모든 하위 객체를 로그로 출력합니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    protected void LogChildren(GameObject parent)
    {
        UIDebugLogger.LogHierarchy(parent, $"[{GetType().Name}]");
    }

    /// <summary>
    /// 특정 이름의 하위 요소에서 컴포넌트를 가져옵니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트 타입</typeparam>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">하위 요소의 이름</param>
    /// <returns>찾은 컴포넌트, 없으면 null</returns>
    protected T GetComponent<T>(GameObject parent, string childName) where T : Component
    {
        GameObject child = GetChild(parent, childName);
        if (child != null)
        {
            T component = child.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] '{childName}'에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] '{childName}' 하위 요소를 찾을 수 없습니다.");
        }
        return null;
    }

    /// <summary>
    /// 특정 경로의 하위 요소를 가져옵니다. (경로는 '/' 문자로 구분)
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="path">하위 요소의 경로 (예: "Panel/Button")</param>
    /// <returns>찾은 하위 요소, 없으면 null</returns>
    protected GameObject GetChildByPath(GameObject parent, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return parent;
        }

        string[] pathElements = path.Split('/');
        Transform current = parent.transform;

        foreach (string element in pathElements)
        {
            Transform child = current.Find(element);
            if (child == null)
            {
                Debug.LogWarning($"[{GetType().Name}] 경로 '{path}'에서 '{element}' 요소를 찾을 수 없습니다.");
                return null;
            }
            current = child;
        }

        return current.gameObject;
    }

    /// <summary>
    /// 특정 경로의 하위 요소에서 컴포넌트를 가져옵니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트 타입</typeparam>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="path">하위 요소의 경로 (예: "Panel/Button")</param>
    /// <returns>찾은 컴포넌트, 없으면 null</returns>
    protected T GetComponentByPath<T>(GameObject parent, string path) where T : Component
    {
        GameObject child = GetChildByPath(parent, path);
        if (child != null)
        {
            T component = child.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] 경로 '{path}'에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
            }
        }
        return null;
    }

    /// <summary>
    /// 특정 이름의 하위 요소에서 여러 컴포넌트를 배열로 가져옵니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트 타입</typeparam>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">하위 요소의 이름</param>
    /// <returns>찾은 컴포넌트 배열, 없으면 빈 배열</returns>
    protected T[] GetComponents<T>(GameObject parent, string childName) where T : Component
    {
        GameObject child = GetChild(parent, childName);
        if (child != null)
        {
            T[] components = child.GetComponents<T>();
            if (components != null && components.Length > 0)
            {
                return components;
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] '{childName}'에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] '{childName}' 하위 요소를 찾을 수 없습니다.");
        }
        return new T[0];
    }

    /// <summary>
    /// 부모 객체의 모든 자식에서 특정 타입의 컴포넌트를 가져옵니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트 타입</typeparam>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="includeInactive">비활성화된 게임오브젝트도 포함할지 여부</param>
    /// <returns>찾은 컴포넌트 리스트</returns>
    protected List<T> GetComponentsInChildren<T>(GameObject parent, bool includeInactive = false) where T : Component
    {
        if (parent == null)
        {
            Debug.LogWarning($"[{GetType().Name}] 부모 객체가 null입니다.");
            return new List<T>();
        }

        T[] components = parent.GetComponentsInChildren<T>(includeInactive);
        return new List<T>(components);
    }

    /// <summary>
    /// 특정 태그를 가진 자식 객체들을 찾습니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="tag">찾을 태그</param>
    /// <returns>찾은 게임오브젝트 리스트</returns>
    protected List<GameObject> FindChildrenWithTag(GameObject parent, string tag)
    {
        List<GameObject> result = new List<GameObject>();
        if (parent == null)
        {
            Debug.LogWarning($"[{GetType().Name}] 부모 객체가 null입니다.");
            return result;
        }

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.CompareTag(tag))
            {
                result.Add(child.gameObject);
            }
        }

        return result;
    }

    /// <summary>
    /// 특정 이름 패턴을 가진 자식 객체들을 찾습니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="namePattern">이름 패턴 (Contains 방식으로 검색)</param>
    /// <returns>찾은 게임오브젝트 리스트</returns>
    protected List<GameObject> FindChildrenByNamePattern(GameObject parent, string namePattern)
    {
        List<GameObject> result = new List<GameObject>();
        if (parent == null || string.IsNullOrEmpty(namePattern))
        {
            Debug.LogWarning($"[{GetType().Name}] 부모 객체가 null이거나 이름 패턴이 비어있습니다.");
            return result;
        }

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name.Contains(namePattern))
            {
                result.Add(child.gameObject);
            }
        }

        return result;
    }

    /// <summary>
    /// 깊이에 상관없이 특정 이름의 하위 객체를 찾습니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">찾을 하위 객체의 이름</param>
    /// <param name="includeInactive">비활성화된 객체도 포함할지 여부</param>
    /// <returns>찾은 첫 번째 하위 객체, 없으면 null</returns>
    protected GameObject FindChildDeep(GameObject parent, string childName, bool includeInactive = true)
    {
        if (parent == null || string.IsNullOrEmpty(childName))
        {
            Debug.LogWarning($"[{GetType().Name}] 부모 객체가 null이거나 자식 이름이 비어있습니다.");
            return null;
        }

        // 먼저 직접적인 자식 확인 (성능 최적화)
        Transform directChild = parent.transform.Find(childName);
        if (directChild != null)
        {
            return directChild.gameObject;
        }

        // 모든 하위 객체 검색
        Transform[] allChildren = parent.GetComponentsInChildren<Transform>(includeInactive);
        foreach (Transform child in allChildren)
        {
            if (child.name == childName && child.gameObject != parent)
            {
                return child.gameObject;
            }
        }

        Debug.LogWarning($"[{GetType().Name}] '{childName}' 이름의 하위 객체를 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 깊이에 상관없이 특정 이름의 하위 객체에서 컴포넌트를 가져옵니다.
    /// </summary>
    /// <typeparam name="T">가져올 컴포넌트 타입</typeparam>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">찾을 하위 객체의 이름</param>
    /// <param name="includeInactive">비활성화된 객체도 포함할지 여부</param>
    /// <returns>찾은 컴포넌트, 없으면 null</returns>
    protected T FindComponentDeep<T>(GameObject parent, string childName, bool includeInactive = true) where T : Component
    {
        GameObject child = FindChildDeep(parent, childName, includeInactive);
        if (child != null)
        {
            T component = child.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] '{childName}'에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
            }
        }
        return null;
    }

    /// <summary>
    /// 깊이에 상관없이 특정 타입의 모든 컴포넌트를 찾습니다.
    /// </summary>
    /// <typeparam name="T">찾을 컴포넌트 타입</typeparam>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="includeInactive">비활성화된 객체도 포함할지 여부</param>
    /// <returns>찾은 컴포넌트 배열</returns>
    protected T[] FindComponentsDeep<T>(GameObject parent, bool includeInactive = true) where T : Component
    {
        if (parent == null)
        {
            Debug.LogWarning($"[{GetType().Name}] 부모 객체가 null입니다.");
            return new T[0];
        }

        return parent.GetComponentsInChildren<T>(includeInactive);
    }

    // 사용 예시:
    /*
    // 예시 1: 버튼 컴포넌트 가져오기
    private void Example1()
    {
        // 게임오브젝트에서 "ConfirmButton"이라는 이름의 하위 객체의 Button 컴포넌트 가져오기
        UnityEngine.UI.Button confirmButton = GetComponent<UnityEngine.UI.Button>(gameObject, "ConfirmButton");
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(() => Debug.Log("버튼이 클릭되었습니다!"));
        }
    }

    // 예시 2: 경로로 텍스트 컴포넌트 가져오기
    private void Example2()
    {
        // "Panel/Header/Title" 경로에 있는 Text 컴포넌트 가져오기
        UnityEngine.UI.Text titleText = GetComponentByPath<UnityEngine.UI.Text>(gameObject, "Panel/Header/Title");
        if (titleText != null)
        {
            titleText.text = "새로운 제목";
        }
    }

    // 예시 3: 여러 컴포넌트 가져와서 활용하기
    private void Example3()
    {
        // 이미지 컴포넌트 가져오기
        UnityEngine.UI.Image backgroundImage = GetComponentByPath<UnityEngine.UI.Image>(gameObject, "Background");
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        }

        // 입력 필드 컴포넌트 가져오기
        UnityEngine.UI.InputField nameInput = GetComponent<UnityEngine.UI.InputField>(gameObject, "NameInput");
        if (nameInput != null)
        {
            nameInput.placeholder.GetComponent<UnityEngine.UI.Text>().text = "이름을 입력하세요";
            nameInput.onEndEdit.AddListener((text) => Debug.Log($"입력된 이름: {text}"));
        }
    }

    // 예시 4: 깊이에 상관없이 객체 찾기
    private void Example4()
    {
        // 깊이에 상관없이 "ConfirmButton" 이름의 객체 찾기
        GameObject confirmButton = FindChildDeep(gameObject, "ConfirmButton");
        if (confirmButton != null)
        {
            confirmButton.SetActive(true);
            
            // 버튼 컴포넌트 가져오기
            UnityEngine.UI.Button button = confirmButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => Debug.Log("깊은 계층의 버튼이 클릭되었습니다!"));
            }
        }
    }

    // 예시 5: 깊이에 상관없이 컴포넌트 직접 찾기
    private void Example5()
    {
        // 깊이에 상관없이 "PlayerName" 이름의 객체에서 Text 컴포넌트 찾기
        UnityEngine.UI.Text playerNameText = FindComponentDeep<UnityEngine.UI.Text>(gameObject, "PlayerName");
        if (playerNameText != null)
        {
            playerNameText.text = "플레이어 1";
            playerNameText.color = Color.blue;
        }
    }

    // 예시 6: 모든 버튼 찾기
    private void Example6()
    {
        // 모든 버튼 컴포넌트 찾기
        UnityEngine.UI.Button[] allButtons = FindComponentsDeep<UnityEngine.UI.Button>(gameObject);
        Debug.Log($"총 {allButtons.Length}개의 버튼을 찾았습니다.");
        
        // 모든 버튼에 이벤트 추가
        foreach (var button in allButtons)
        {
            button.onClick.AddListener(() => Debug.Log($"{button.name} 버튼이 클릭되었습니다!"));
        }
    }
    */
}
