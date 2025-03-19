using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using Unity.Assets.Scripts.Resource;

public class UIManager
{

	[Inject] private DebugClassFacade _debugFacade;
	[Inject] private ResourceManager _resourceManager;

	// 싱글톤 인스턴스
	// private static UIManager s_Instance;
	// public static UIManager Instance
	// {
	// 	get
	// 	{
	// 		if (s_Instance == null)
	// 		{
	// 			s_Instance = new UIManager();		
	// 		}
	// 		return s_Instance;
	// 	}
	// }

	private int _order = 10;

	// private Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();

	private UI_Scene _sceneUI = null;
	public UI_Scene SceneUI
	{
		set { _sceneUI = value; }
		get { return _sceneUI; }
	}

	public GameObject Root
	{
		get
		{
			GameObject root = GameObject.Find("@UI_Root");
			if (root == null)
				root = new GameObject { name = "@UI_Root" };
			return root;
		}
	}
	// UIManager.cs에 추가


// 책임 분리 원칙
// UIManager의 책임:
// 전역적인 UI 시스템 이벤트 구독
// 여러 UI 화면에서 공통으로 사용되는 기능 관리
// UI 리소스 관리 및 생명주기 관리
// ex     // 2. 네트워크 상태 변화 구독,        // 5. 공통 UI 요소 초기화
    // 1. 시스템 이벤트 구독 ,     // 3. 게임 상태 변화 구독     // 4. 언어 변경 이벤트 구독

 


// MainMenu(개별 UI)의 책임:
// 자신의 UI 요소에 대한 이벤트 구독
// 특정 화면에만 필요한 기능 구현
// 사용자 상호작용 처리
        // 랜덤 매치 버튼 이벤트 구독


	public virtual void SubscribeEvents()
	{
		_debugFacade.LogInfo(GetType().Name, "이벤트 구독 시작");

	}

	public virtual void UnsubscribeEvents()
	{
		_debugFacade.LogInfo(GetType().Name, "이벤트 구독 해제");

	}
	



	public void SetCanvas(GameObject go, bool sort = true, int sortOrder = 0)
	{
		Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
		if (canvas == null)
		{
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.overrideSorting = true;
		}

		CanvasScaler cs = go.GetOrAddComponent<CanvasScaler>();
		if (cs != null)
		{
			cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			cs.referenceResolution = new Vector2(1080, 1920);
		}

		go.GetOrAddComponent<GraphicRaycaster>();

		if (sort)
		{
			canvas.sortingOrder = _order;
			_order++;
		}
		else
		{
			canvas.sortingOrder = sortOrder;
		}
	}

	public T GetSceneUI<T>() where T : UI_Base
	{
		return _sceneUI as T;
	}

	// public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
	// {
	// 	if (string.IsNullOrEmpty(name))
	// 		name = typeof(T).Name;

	// 	GameObject go = Managers.Resource.Instantiate($"{name}");
	// 	if (parent != null)
	// 		go.transform.SetParent(parent);

	// 	Canvas canvas = go.GetOrAddComponent<Canvas>();
	// 	canvas.renderMode = RenderMode.WorldSpace;
	// 	canvas.worldCamera = Camera.main;

	// 	return Util.GetOrAddComponent<T>(go);
	// }

	// public T MakeSubItem<T>(Transform parent = null, string name = null, bool pooling = true) where T : UI_Base
	// {
	// 	if (string.IsNullOrEmpty(name))
	// 		name = typeof(T).Name;

	// 	GameObject go = Managers.Resource.Instantiate(name, parent, pooling);
	// 	go.transform.SetParent(parent);

	// 	return Util.GetOrAddComponent<T>(go);
	// }

	/// <summary>
	/// UI_Base를 상속받는 UI 요소를 동적으로 생성하고 화면에 표시합니다.
	/// 주의: 해당 UI 타입과 동일한 이름의 프리팹이 리소스 폴더에 존재해야 합니다.
	/// </summary>
	/// <typeparam name="T">생성할 UI 컴포넌트 타입 (UI_Base를 상속받아야 함)</typeparam>
	/// <param name="name">리소스 이름 (null인 경우 T의 타입 이름을 사용)</param>
	/// <returns>생성된 UI 컴포넌트</returns>
	/// <example>
	/// <code>
	/// // 사용 예시 (각 UI 타입의 프리팹이 리소스 폴더에 미리 생성되어 있어야 함)
	/// 
	/// // 1. 알림창 표시 (Resources 폴더에 "UI_Notification" 프리팹 필요)
	/// UI_Notification notification = UIManager.Instance.ShowBaseUI<UI_Notification>();
	/// notification.SetMessage("아이템을 획득했습니다!");
	/// notification.SetDuration(3.0f); // 3초 후 자동으로 사라짐
	/// 
	/// // 2. 확인/취소 대화 상자 표시 (Resources 폴더에 "UI_ConfirmDialog" 프리팹 필요)
	/// UI_ConfirmDialog dialog = UIManager.Instance.ShowBaseUI<UI_ConfirmDialog>();
	/// dialog.SetMessage("정말로 게임을 종료하시겠습니까?");
	/// dialog.SetCallback(isConfirmed => {
	///     if (isConfirmed) {
	///         // 게임 종료 로직
	///         Application.Quit();
	///     }
	/// });
	/// 
	/// // 3. 로딩 화면 표시 (Resources 폴더에 "UI_Loading" 프리팹 필요)
	/// UI_Loading loading = UIManager.Instance.ShowBaseUI<UI_Loading>();
	/// loading.StartLoading(() => {
	///     // 로딩이 완료된 후 실행할 코드
	///     SceneManager.LoadScene("GameScene");
	/// });
	/// 
	/// // 4. 커스텀 이름으로 프리팹 로드 (Resources 폴더에 "CustomNotification" 프리팹 필요)
	/// UI_Notification customNotification = UIManager.Instance.ShowBaseUI<UI_Notification>("CustomNotification");
	/// customNotification.SetMessage("커스텀 알림입니다!");
	/// </code>
	/// </example>
	public T ShowBaseUI<T>(string name = null) where T : UI_Base
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		_debugFacade.LogInfo(GetType().Name, $"UI 표시: {name}");

		GameObject go = _resourceManager.Instantiate(name);
		T baseUI = Util.GetOrAddComponent<T>(go);

		go.transform.SetParent(Root.transform);

		return baseUI;
	}

	// public T ShowSceneUI<T>(string name = null) where T : UI_Scene
	// {
	// 	if (string.IsNullOrEmpty(name))
	// 		name = typeof(T).Name;

	// 	GameObject go = Managers.Resource.Instantiate(name);
	// 	T sceneUI = Util.GetOrAddComponent<T>(go);
	// 	_sceneUI = sceneUI;

	// 	go.transform.SetParent(Root.transform);

	// 	return sceneUI;
	// }

	// public T ShowPopupUI<T>(string name = null) where T : UI_Popup
	// {
	// 	if (string.IsNullOrEmpty(name))
	// 		name = typeof(T).Name;

	// 	GameObject go = Managers.Resource.Instantiate(name);
	// 	T popup = Util.GetOrAddComponent<T>(go);
	// 	_popupStack.Push(popup);

	// 	go.transform.SetParent(Root.transform);

	// 	return popup;
	// }

	// public void ClosePopupUI(UI_Popup popup)
	// {
	// 	if (_popupStack.Count == 0)
	// 		return;

	// 	if (_popupStack.Peek() != popup)
	// 	{
	// 		Debug.Log("Close Popup Failed!");
	// 		return;
	// 	}

	// 	ClosePopupUI();
	// }

	// public void ClosePopupUI()
	// {
	// 	if (_popupStack.Count == 0)
	// 		return;

	// 	UI_Popup popup = _popupStack.Pop();
	// 	Managers.Resource.Destroy(popup.gameObject);
	// 	_order--;
	// }

	// public void CloseAllPopupUI()
	// {
	// 	while (_popupStack.Count > 0)
	// 		ClosePopupUI();
	// }

	// public int GetPopupCount()
	// {
	// 	return _popupStack.Count;
	// }

	// public void Clear()
	// {
	// 	CloseAllPopupUI();
	// 	_sceneUI = null;
	// }
}
