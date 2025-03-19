using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class Util
{
	#region 컴포넌트 관리
	/// <summary>
	/// 게임 오브젝트에 컴포넌트가 없으면 추가하고, 있으면 가져옵니다.
	/// </summary>
	public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
	{
		if (go == null)
		{
			Debug.LogWarning($"[Util] GetOrAddComponent: GameObject가 null입니다.");
			return null;
		}

		T component = go.GetComponent<T>();
		if (component == null)
			component = go.AddComponent<T>();

		return component;
	}
	#endregion

	#region 자식 오브젝트 찾기
	/// <summary>
	/// 특정 이름의 자식 게임오브젝트를 찾습니다.
	/// </summary>
	/// <param name="go">부모 GameObject</param>
	/// <param name="name">찾을 자식의 이름 (null이면 모든 자식)</param>
	/// <param name="recursive">하위 계층까지 재귀적으로 검색할지 여부</param>
	/// <returns>찾은 자식 GameObject, 없으면 null</returns>
	public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
	{
		Transform transform = FindChild<Transform>(go, name, recursive);
		if (transform == null)
			return null;

		return transform.gameObject;
	}

	/// <summary>
	/// 특정 이름의 자식에서 컴포넌트를 찾습니다.
	/// </summary>
	/// <typeparam name="T">찾을 컴포넌트 타입</typeparam>
	/// <param name="go">부모 GameObject</param>
	/// <param name="name">찾을 자식의 이름 (null이면 모든 자식)</param>
	/// <param name="recursive">하위 계층까지 재귀적으로 검색할지 여부</param>
	/// <returns>찾은 컴포넌트, 없으면 null</returns>
	public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
	{
		if (go == null)
		{
			Debug.LogWarning($"[Util] FindChild: GameObject가 null입니다.");
			return null;
		}

		if (recursive == false)
		{
			for (int i = 0; i < go.transform.childCount; i++)
			{
				Transform transform = go.transform.GetChild(i);
				if (string.IsNullOrEmpty(name) || transform.name == name)
				{
					T component = transform.GetComponent<T>();
					if (component != null)
						return component;
				}
			}
		}
		else
		{
			foreach (T component in go.GetComponentsInChildren<T>(true))
			{
				if (string.IsNullOrEmpty(name) || component.name == name)
					return component;
			}
		}

		return null;
	}

	/// <summary>
	/// 게임오브젝트의 모든 직접 자식을 가져옵니다.
	/// </summary>
	/// <param name="parent">부모 GameObject</param>
	/// <returns>자식 GameObject 목록</returns>
	public static List<GameObject> GetAllChildren(GameObject parent)
	{
		List<GameObject> children = new List<GameObject>();
		
		if (parent == null)
		{
			Debug.LogWarning($"[Util] GetAllChildren: 부모 객체가 null입니다.");
			return children;
		}

		for (int i = 0; i < parent.transform.childCount; i++)
		{
			Transform child = parent.transform.GetChild(i);
			children.Add(child.gameObject);
		}

		return children;
	}

	/// <summary>
	/// 특정 이름의 자식 게임오브젝트를 활성화합니다.
	/// </summary>
	/// <param name="parent">부모 GameObject</param>
	/// <param name="childName">자식의 이름</param>
	public static void ShowChild(GameObject parent, string childName)
	{
		GameObject child = FindChild(parent, childName, false);
		if (child != null)
		{
			child.SetActive(true);
		}
		else
		{
			Debug.LogWarning($"[Util] ShowChild: '{childName}' 자식을 찾을 수 없습니다.");
		}
	}

	/// <summary>
	/// 특정 이름의 자식 게임오브젝트를 비활성화합니다.
	/// </summary>
	/// <param name="parent">부모 GameObject</param>
	/// <param name="childName">자식의 이름</param>
	public static void HideChild(GameObject parent, string childName)
	{
		GameObject child = FindChild(parent, childName, false);
		if (child != null)
		{
			child.SetActive(false);
		}
		else
		{
			Debug.LogWarning($"[Util] HideChild: '{childName}' 자식을 찾을 수 없습니다.");
		}
	}

	/// <summary>
	/// 경로로 자식 게임오브젝트를 찾습니다. (예: "Panel/Button")
	/// </summary>
	/// <param name="parent">부모 GameObject</param>
	/// <param name="path">찾을 경로 (예: "Panel/Button")</param>
	/// <returns>찾은 GameObject, 없으면 null</returns>
	public static GameObject GetChildByPath(GameObject parent, string path)
	{
		if (parent == null || string.IsNullOrEmpty(path))
		{
			Debug.LogWarning($"[Util] GetChildByPath: 부모 객체가 null이거나 경로가 비어있습니다.");
			return null;
		}

		Transform current = parent.transform;
		string[] pathParts = path.Split('/');

		foreach (string part in pathParts)
		{
			Transform child = current.Find(part);
			if (child == null)
			{
				Debug.LogWarning($"[Util] GetChildByPath: '{path}' 경로에서 '{part}' 부분을 찾을 수 없습니다.");
				return null;
			}
			current = child;
		}

		return current.gameObject;
	}

	/// <summary>
	/// 경로로 자식의 컴포넌트를 찾습니다.
	/// </summary>
	/// <typeparam name="T">찾을 컴포넌트 타입</typeparam>
	/// <param name="parent">부모 GameObject</param>
	/// <param name="path">찾을 경로 (예: "Panel/Button")</param>
	/// <returns>찾은 컴포넌트, 없으면 null</returns>
	public static T GetComponentByPath<T>(GameObject parent, string path) where T : Component
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
				Debug.LogWarning($"[Util] GetComponentByPath: '{path}'에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
			}
		}
		return null;
	}

	/// <summary>
	/// 특정 이름의 자식에서 여러 컴포넌트를 배열로 가져옵니다.
	/// </summary>
	/// <typeparam name="T">가져올 컴포넌트 타입</typeparam>
	/// <param name="parent">부모 GameObject</param>
	/// <param name="childName">자식의 이름</param>
	/// <returns>찾은 컴포넌트 배열, 없으면 빈 배열</returns>
	public static T[] GetComponents<T>(GameObject parent, string childName) where T : Component
	{
		GameObject child = FindChild(parent, childName, false);
		if (child != null)
		{
			T[] components = child.GetComponents<T>();
			if (components != null && components.Length > 0)
			{
				return components;
			}
			else
			{
				Debug.LogWarning($"[Util] GetComponents: '{childName}'에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
			}
		}
		else
		{
			Debug.LogWarning($"[Util] GetComponents: '{childName}' 자식을 찾을 수 없습니다.");
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
	public static List<T> GetComponentsInChildren<T>(GameObject parent, bool includeInactive = false) where T : Component
	{
		if (parent == null)
		{
			Debug.LogWarning($"[Util] GetComponentsInChildren: 부모 객체가 null입니다.");
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
	public static List<GameObject> FindChildrenWithTag(GameObject parent, string tag)
	{
		List<GameObject> result = new List<GameObject>();
		if (parent == null)
		{
			Debug.LogWarning($"[Util] FindChildrenWithTag: 부모 객체가 null입니다.");
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
	/// 이름 패턴으로 자식 객체들을 찾습니다.
	/// </summary>
	/// <param name="parent">부모 GameObject</param>
	/// <param name="namePattern">이름 패턴 (예: "Button")</param>
	/// <returns>찾은 게임오브젝트 리스트</returns>
	public static List<GameObject> FindChildrenByNamePattern(GameObject parent, string namePattern)
	{
		List<GameObject> result = new List<GameObject>();
		if (parent == null || string.IsNullOrEmpty(namePattern))
		{
			Debug.LogWarning($"[Util] FindChildrenByNamePattern: 부모 객체가 null이거나 이름 패턴이 비어있습니다.");
			return result;
		}

		Transform[] children = parent.GetComponentsInChildren<Transform>(true);
		foreach (Transform child in children)
		{
			if (child.gameObject != parent && child.name.Contains(namePattern))
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
	public static GameObject FindChildDeep(GameObject parent, string childName, bool includeInactive = true)
	{
		if (parent == null || string.IsNullOrEmpty(childName))
		{
			Debug.LogWarning($"[Util] FindChildDeep: 부모 객체가 null이거나 자식 이름이 비어있습니다.");
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
				Debug.Log($"<color=green>[Util] FindChildDeep: '{childName}' 찾았습니다.</color>");

				return child.gameObject;
			}
		}

		Debug.LogWarning($"[Util] FindChildDeep: '{childName}' 이름의 하위 객체를 찾을 수 없습니다.");
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
	public static T FindComponentDeep<T>(GameObject parent, string childName, bool includeInactive = true) where T : Component
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
				Debug.LogWarning($"[Util] FindComponentDeep: '{childName}'에서 {typeof(T).Name} 컴포넌트를 찾을 수 없습니다.");
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
	public static T[] FindComponentsDeep<T>(GameObject parent, bool includeInactive = true) where T : Component
	{
		if (parent == null)
		{
			Debug.LogWarning($"[Util] FindComponentsDeep: 부모 객체가 null입니다.");
			return new T[0];
		}

		return parent.GetComponentsInChildren<T>(includeInactive);
	}
	#endregion

	#region UI 이벤트 관리
	/// <summary>
	/// 버튼에 클릭 이벤트를 추가합니다.
	/// </summary>
	/// <param name="button">대상 버튼</param>
	/// <param name="action">클릭 시 실행할 액션</param>
	/// <param name="buttonName">버튼 이름 (로그용, null이면 버튼 오브젝트 이름 사용)</param>
	public static void AddButtonClickEvent(Button button, UnityAction action, string buttonName = null)
	{
		if (button != null)
		{
			button.onClick.AddListener(action);
			Debug.Log($"<color=green>[Util] {buttonName ?? button.name} 버튼 이벤트 등록 완료</color>");
		}
		else
		{
			Debug.LogError($"<color=red>[Util] {buttonName ?? "지정된"} 버튼을 찾을 수 없습니다!</color>");
		}
	}

	/// <summary>
	/// 버튼에서 클릭 이벤트를 제거합니다.
	/// </summary>
	/// <param name="button">대상 버튼</param>
	/// <param name="action">제거할 액션</param>
	/// <param name="buttonName">버튼 이름 (로그용, null이면 버튼 오브젝트 이름 사용)</param>
	public static void RemoveButtonClickEvent(Button button, UnityAction action, string buttonName = null)
	{
		if (button != null)
		{
			button.onClick.RemoveListener(action);
			Debug.Log($"<color=cyan>[Util] {buttonName ?? button.name} 버튼 이벤트 해제 완료</color>");
		}
	}
	#endregion

	#region 기타 유틸리티
	/// <summary>
	/// 문자열을 열거형으로 변환합니다.
	/// </summary>
	public static T ParseEnum<T>(string value)
	{
		return (T)Enum.Parse(typeof(T), value, true);
	}

	/// <summary>
	/// 16진수 문자열을 Color로 변환합니다.
	/// </summary>
	public static Color HexToColor(string color)
	{
		if (color.Contains("#") == false)
			color = $"#{color}";

		ColorUtility.TryParseHtmlString(color, out Color parsedColor);

		return parsedColor;
	}

	/// <summary>
	/// 자식 객체들의 계층 구조를 로그로 출력합니다.
	/// </summary>
	/// <param name="parent">부모 GameObject</param>
	public static void LogChildren(GameObject parent)
	{
		if (parent == null)
		{
			Debug.LogWarning("[Util] LogChildren: 부모 객체가 null입니다.");
			return;
		}

		Debug.Log($"<color=yellow>[Util] '{parent.name}' 자식 계층 구조:</color>");
		LogChildrenRecursive(parent.transform, 0);
	}

	private static void LogChildrenRecursive(Transform parent, int depth)
	{
		string indent = new string(' ', depth * 2);
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			Debug.Log($"<color=yellow>{indent}├─ {child.name} {(child.gameObject.activeSelf ? "(활성)" : "(비활성)")}</color>");
			LogChildrenRecursive(child, depth + 1);
		}
	}
	#endregion

	// 주석 처리된 기존 코드는 필요에 따라 복원
	/*
	public static ECreatureType DetermineTargetType(ECreatureType ownerType, bool findAllies)
	{
		if (ownerType == Define.ECreatureType.Hero)
		{
			return findAllies ? ECreatureType.Hero : ECreatureType.Monster;
		}
		else if (ownerType == Define.ECreatureType.Monster)
		{
			return findAllies ? ECreatureType.Monster : ECreatureType.Hero;
		}

		return ECreatureType.None;
	}

	public static float GetEffectRadius(EEffectSize size)
	{
		switch (size)
		{
			case EEffectSize.CircleSmall:
				return EFFECT_SMALL_RADIUS;
			case EEffectSize.CircleNormal:
				return EFFECT_NORMAL_RADIUS;
			case EEffectSize.CircleBig:
				return EFFECT_BIG_RADIUS;
			case EEffectSize.ConeSmall:
				return EFFECT_SMALL_RADIUS * 2f;
			case EEffectSize.ConeNormal:
				return EFFECT_NORMAL_RADIUS * 2f;
			case EEffectSize.ConeBig:
				return EFFECT_BIG_RADIUS * 2f;
			default:
				return EFFECT_SMALL_RADIUS;
		}
	}
	*/
}