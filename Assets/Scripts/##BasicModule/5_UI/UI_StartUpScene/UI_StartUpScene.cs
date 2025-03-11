using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Unity.Assets.Scripts.Resource;
using Unity.Assets.Scripts.Scene;
using Object = UnityEngine.Object;

namespace Unity.Assets.Scripts.UI
{
    /// <summary>
    /// 시작 화면 UI 관리 클래스
    /// </summary>
    public partial class UI_StartUpScene : UI_Scene
    {
        #region Enums
        
        enum Texts
        {
            DisplayText
        }
        
        enum Images
        {
            ProgressBar,
            ProgressBarBackground,
            Fill
        }
        
        enum GameObjects
        {
            ProgressBarArea
        }
        
        /// <summary>
        /// 로딩 단계 정의
        /// </summary>
        public enum LoadingStep
        {
            Initialize,      // 초기화
            ResourceLoad,    // 리소스 로드
            ConnectionLoad,  // 네트워크 연결 로드
            Complete         // 완료
        }
        
        #endregion

        #region Constants
        
        private const string PRELOAD_LABEL = "PreLoad";
        private const float NEXT_SCENE_DELAY = 1.0f; // 로딩 완료 후 다음 씬으로 전환하기 전 대기 시간
        
        // 진행률 범위 상수
        private const float INIT_PROGRESS_START = 0.0f;
        private const float INIT_PROGRESS_END = 0.3f;
        private const float RESOURCE_PROGRESS_START = 0.3f;
        private const float RESOURCE_PROGRESS_END = 0.9f;
        private const float COMPLETE_PROGRESS_START = 0.9f;
        private const float COMPLETE_PROGRESS_END = 1.0f;
        
        #endregion

        #region Injected Dependencies
        
        [Inject] private StartUpScene _startUpScene;
        [Inject] private SceneManagerEx _sceneManager;

        #endregion

        #region Private Fields
        
        private bool _isResourceLoaded = false;
        private bool _isProgressBarInitialized = false;
        private bool _isLoading = false;
        
        private float _progress = 0f;
        private string _status = "";
        private LoadingStep _currentStep = LoadingStep.Initialize;
        
        #endregion

        #region Properties
        
        public float Progress 
        { 
            get => _progress;
            private set => _progress = Mathf.Clamp01(value);
        }
        
        public string Status
        {
            get => _status;
            private set => _status = value;
        }
        
        #endregion

        #region Events
        
        // 프로그레스 변경 이벤트 (진행률, 상태 메시지)
        public event Action<float, string> OnProgressChanged;
        
        // 프로그레스 완료 이벤트
        public event Action OnProgressComplete;
        
        #endregion

        #region Unity Lifecycle Methods
        
        private void Start()
        {
            LogDebug("[UI_StartUpScene] Start 메서드 호출됨");
            
            // StartUpScene이 주입되지 않은 경우 직접 찾기
            if (_startUpScene == null)
            {
                _startUpScene = FindAnyObjectByType<StartUpScene>();
                if (_startUpScene == null)
                {
                    LogError("[UI_StartUpScene] StartUpScene을 찾을 수 없습니다!");
                }
                else
                {
                    LogDebug("[UI_StartUpScene] StartUpScene을 직접 찾았습니다.");
                }
            }
        }
        
        private void OnDestroy()
        {
            LogDebug("[UI_StartUpScene] OnDestroy 메서드 호출됨");
            UnsubscribeEvents();
        }
        
        #endregion

        #region Initialization
        
        public override bool Init()
        {
            if (base.Init() == false)
                return false;

            LogDebug("[UI_StartUpScene] Init 메서드 호출됨");
            
            try
            {
                BindUI();
                SubscribeEvents();
                StartCoroutine(InitializeLoadingProcess());
                return true;
            }
            catch (System.Exception e)
            {
                LogError($"[UI_StartUpScene] 초기화 중 오류 발생: {e.Message}\n{e.StackTrace}");
                UpdateDebugInfo($"Error: {e.Message}");
                return false;
            }
        }
        
        private void BindUI()
        {
            BindTexts(typeof(Texts));
            BindImages(typeof(Images));
            LogDebug("[UI_StartUpScene] UI 요소 바인딩 완료");
        }
        
        private void SubscribeEvents()
        {
            // 프로그레스 이벤트 구독
            OnProgressChanged += UpdateProgressUI;
            OnProgressComplete += OnLoadingComplete;
            
            if (_sceneManager == null)
            {
                LogError("[UI_StartUpScene] SceneManagerEx가 주입되지 않았습니다!");
            }
        }
        
        private void UnsubscribeEvents()
        {
            // 이벤트 구독 해제
            OnProgressChanged -= UpdateProgressUI;
            OnProgressComplete -= OnLoadingComplete;
            
            // StartUpScene 이벤트 구독 해제
            StartUpScene startUpScene = _sceneManager?.CurrentScene as StartUpScene;
            if (startUpScene != null)
            {
                startUpScene.OnResourceLoadComplete -= OnResourceLoadingComplete;
            }
        }
        
        #endregion
    }
}

