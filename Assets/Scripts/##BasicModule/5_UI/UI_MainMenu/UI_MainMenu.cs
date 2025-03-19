using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Unity.Assets.Scripts.Resource;
using Unity.Assets.Scripts.Scene;
using Object = UnityEngine.Object;
using Unity.Assets.Scripts.UI;
using VContainer.Unity;
using Unity.Services.Lobbies.Models;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Assets.Scripts.UI
{
    /// <summary>
    /// 시작 화면 UI 관리 클래스
    /// </summary>
    public partial class UI_MainMenu : UI_Scene
    {
        #region Enums
        
        enum Texts
        {
        }
        
        enum Images
        {
        }
        
        enum GameObjects
        {
            Matching,
            Main
        }

        #endregion



        #region Injected Dependencies and Action
        
        // [Inject] private MainMenuScene _MainMenuScene;
        public static event Action OnRandomMatchRequested;

        #endregion



        #region Properties
        

        private GameObject MatchingObject => GetObject((int)GameObjects.Matching);
        private GameObject MainObject => GetObject((int)GameObjects.Main);
        #endregion

        #region Events
        
        // UI 이벤트 정의 - 다른 클래스에서 구독할 수 있는 정적 이벤트
        
        #endregion

        #region Unity Lifecycle Methods
        
        private void Start()
        {
        }
        
        protected override void OnDestroy()
        {
            Debug.Log("[UI_MainMenu] OnDestroy 메서드 호출됨");
            UnsubscribeEvents();
            
            // 부모 클래스의 OnDestroy 호출
            base.OnDestroy();
        }
        
        #endregion

        #region Initialization
        
        public override bool Init()
        {
            BindUI();

            if (base.Init() == false)
                return false;

            Debug.Log("[UI_MainMenu] Init 메서드 호출됨");
            
            try
            {
                
                // 바인딩 상태 확인
                bool matchingBound = MatchingObject != null;
                bool mainBound = MainObject != null;

                if (matchingBound && mainBound)
                {
                    Debug.Log($"<color=green>[UI_MainMenu] Init 완료: 객체 바인딩 성공 (Matching: {MatchingObject.transform.childCount}개, Main: {MainObject.transform.childCount}개)</color>");
                }
                else
                {
                    string errorMsg = "<color=red>[UI_MainMenu] Init 완료: 바인딩 실패! ";
                    if (!matchingBound) errorMsg += "Matching 객체 없음. ";
                    if (!mainBound) errorMsg += "Main 객체 없음. ";
                    errorMsg += "</color>";
                    Debug.LogError(errorMsg);
                }
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UI_MainMenu] 초기화 중 오류 발생: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }
        
        private void BindUI()
        {
            // UI 요소 바인딩
            BindTexts(typeof(Texts));
            BindImages(typeof(Images));
            BindObjects(typeof(GameObjects));


            // 계층 구조 출력
            DebugComponents.LogHierarchy(gameObject, "[UI_MainMenu]");
        }
        
        /// <summary>
        /// 모든 UI 이벤트를 구독합니다.
        /// </summary>
        protected override void SubscribeEvents()
        {
            // null 체크 추가
            if (uiManager == null)
            {
                Debug.LogWarning($"<color=yellow>[{GetType().Name}] uiManager가 null입니다. DI가 제대로 설정되지 않았을 수 있습니다.</color>");
                return;
            }
            
            // 부모 클래스의 SubscribeEvents 호출 - UIManager 이벤트 구독
            base.SubscribeEvents();
            
            // 이미 바인딩된 MainObject 사용
            if (MainObject != null)
            {
                // MainObject 안에서 RandomMatch 객체 찾기
                GameObject randomMatchButton = Util.FindChildDeep(MainObject, "RandomMatch");
                
                if (randomMatchButton != null)
                {
                    // RandomMatch 객체에 클릭 이벤트 바인딩
                    randomMatchButton.BindEvent((evt) =>
                    {
                        // 이벤트 발생
                        OnRandomMatchRequested?.Invoke();
                        Debug.Log($"<color=green>[{GetType().Name}] 랜덤 매치 요청</color>");
                    }, Define.EUIEvent.Click);
                    
                    Debug.Log($"<color=green>[{GetType().Name}] RandomMatch 버튼에 이벤트 등록 완료</color>");
                }
                else
                {
                    Debug.LogError($"<color=red>[{GetType().Name}] MainObject 안에서 RandomMatch 객체를 찾을 수 없습니다!</color>");
                }
            }
            else
            {
                Debug.LogError($"<color=red>[{GetType().Name}] MainObject가 바인딩되지 않았습니다!</color>");
            }
        }
        
        /// <summary>
        /// 모든 UI 이벤트 구독을 해제합니다.
        /// </summary>
            protected override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents(); // 부모 클래스의 구현 호출
            
            // 이벤트 구독 해제는 필요 없음 (정적 이벤트이므로)
            // 단, 다른 인스턴스 이벤트가 있다면 여기서 구독 해제
        }

        
        #endregion

        #region Event Handlers
        
        
        #endregion

        #region Editor Methods
        
        #if UNITY_EDITOR
        [ContextMenu("로그: Matching 하위 객체 출력")]
        private void LogMatchingChildrenMenu()
        {
            DebugComponents.LogHierarchy(MatchingObject, "[UI_MainMenu]");
        }
        
        // Inspector에서 버튼으로 표시되는 메서드
        [CustomEditor(typeof(UI_MainMenu))]
        public class UI_MainMenuEditor : DebugComponents.UIDebugEditorBase
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                
                UI_MainMenu script = (UI_MainMenu)target;
                
                // UIDebugLogger의 에디터 확장 기능 사용
                AddDebugButtons(script.gameObject, "Matching", "[UI_MainMenu]");
            }
        }
        #endif
        
        #endregion
    }
}

