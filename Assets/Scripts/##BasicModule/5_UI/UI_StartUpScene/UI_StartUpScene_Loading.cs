using System.Collections;
using UnityEngine;
using Unity.Assets.Scripts.Scene;
using Object = UnityEngine.Object;

namespace Unity.Assets.Scripts.UI
{
    /// <summary>
    /// UI_StartUpScene의 로딩 프로세스 관련 기능
    /// </summary>
    public partial class UI_StartUpScene
    {
        #region Loading Process
        
        /// <summary>
        /// 로딩 프로세스 초기화 및 시작
        /// </summary>
        private IEnumerator InitializeLoadingProcess()
        {
            _isLoading = true;
            
            // 1. 초기화 단계
            yield return StartCoroutine(ProcessInitializeStep());
            
            // 2. 리소스 로드 단계
            yield return StartCoroutine(ProcessResourceLoadStep());
            
            // 3. 네트워크 연결 단계
            // yield return StartCoroutine(ProcessConnectionLoadStep());
            
            // 4. 인증 단계
            yield return StartCoroutine(ProcessAuthStep());

            // 5. 완료 단계
            yield return StartCoroutine(ProcessCompleteStep());
            
            _isLoading = false;
            UpdateProgress(1.0f, "로딩 완료!");
        }
        
        /// <summary>
        /// 초기화 단계 처리 (0% ~ 30%)
        /// </summary>
        private IEnumerator ProcessInitializeStep()
        {
            _currentStep = LoadingStep.Initialize;
            UpdateProgress(INIT_PROGRESS_START, "초기화 중...");
            
            // 초기화 단계에서 진행률 서서히 증가
            float elapsed = 0f;
            float duration = 1.0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float progress = Mathf.Lerp(INIT_PROGRESS_START, INIT_PROGRESS_END, t);
                UpdateProgress(progress, "초기화 중...");
                
                yield return new WaitForSeconds(0.05f);
                elapsed += 0.05f;
            }
            
            // 마지막에 정확한 종료 진행률로 설정
            UpdateProgress(INIT_PROGRESS_END, "초기화 중...");
        }
        
        /// <summary>
        /// 리소스 로드 단계 처리 (30% ~ 70%)
        /// </summary>
        private IEnumerator ProcessResourceLoadStep()
        {
            _currentStep = LoadingStep.ResourceLoad;
            UpdateProgress(RESOURCE_PROGRESS_START, "Resource Load...");
            
            LogDebug($"[UI_StartUpScene] 리소스 로딩 시작");
            UpdateDebugInfo($"Resource Load...");
            
            // 리소스 로딩 진행 상황 추적을 위한 변수
            int totalResourceCount = 0;
            int loadedResourceCount = 0;
            
            // StartUpScene 찾기 및 대기 로직
            
            // StartUpScene이 null인지 확인
            if (_startUpScene == null)
            {
                LogError("[UI_StartUpScene] StartUpScene이 null입니다.");
                // 직접 찾기 시도
                _startUpScene = FindAnyObjectByType<StartUpScene>();
                
                if (_startUpScene == null)
                {
                    LogError("[UI_StartUpScene] StartUpScene을 찾을 수 없습니다!");
                    UpdateDebugInfo($"Error: StartUpScene을 찾을 수 없습니다!");
                    // 리소스 로드 단계를 건너뛰고 다음 단계로 진행
                    _isResourceLoaded = true;
                    yield break;
                }
                else
                {
                    LogDebug("[UI_StartUpScene] StartUpScene을 직접 찾았습니다.");
                }
            }
            
            try
            {
                LogDebug("[UI_StartUpScene] 리소스 로드 이벤트 구독 시작");
                
                // StartUpScene의 리소스 로드 이벤트 구독
                _startUpScene.OnResourceLoadProgress += (key, count, totalCount) =>
                {
                    // 총 리소스 개수 업데이트
                    totalResourceCount = totalCount;
                    loadedResourceCount = count;
                    
                    // 진행률 계산 및 업데이트 (30% ~ 70% 범위로 조정)
                    float loadProgress = (float)count / totalCount;
                    float adjustedProgress = RESOURCE_PROGRESS_START + (loadProgress * (RESOURCE_PROGRESS_END - RESOURCE_PROGRESS_START));
                    
                    // 상태 메시지 업데이트
                    string status = $"리소스 로드 중... ({count}/{totalCount})";
                    
                    // 진행률 및 상태 업데이트
                    UpdateProgress(adjustedProgress, status);
                    
                    LogDebug($"[UI_StartUpScene] 리소스 로드 중: {key}, {count}/{totalCount}, 진행률: {loadProgress:P0}, 조정된 진행률: {adjustedProgress:P0}");
                };
                
                // 리소스 로드 완료 이벤트 구독
                _startUpScene.OnResourceLoadComplete += OnResourceLoadingComplete;
                
                LogDebug("[UI_StartUpScene] 리소스 로드 이벤트 구독 완료");
            }
            catch (System.Exception e)
            {
                LogError($"[UI_StartUpScene] 리소스 로딩 중 오류 발생: {e.Message}\n{e.StackTrace}");
                UpdateDebugInfo($"Error: {e.Message}");
            }
            
            UpdateProgress(RESOURCE_PROGRESS_END, "Resource Load Complete");
            LogDebug("[UI_StartUpScene] 리소스 로드 단계 완료");
        }
        

        private IEnumerator ProcessConnectionLoadStep()
        {
            _currentStep = LoadingStep.ConnectionLoad;
            yield return new WaitForSeconds(0.2f);

            LogDebug($"[UI_StartUpScene] 네트워크 연결 시작");
            UpdateDebugInfo($"Network Connection...");
        }
        
        /// <summary>
        /// 인증 단계 처리 (70% ~ 90%)
        /// </summary>
        private IEnumerator ProcessAuthStep()
        {
            _currentStep = LoadingStep.AuthLoad;
            UpdateProgress(AUTH_PROGRESS_START, "인증 준비 중...");
            
            LogDebug("[UI_StartUpScene] 인증 단계 시작");
            
            // 1. AuthManager 확인
            if (_authManager == null)
            {
                LogError("[UI_StartUpScene] AuthManager가 null입니다");
                UpdateProgress(AUTH_PROGRESS_END, "인증 실패 (오프라인 모드)");
                yield break;
            }
            
            // 2. 진행률 표시 애니메이션
            float elapsed = 0f;
            float animDuration = 0.5f;
            while (elapsed < animDuration)
            {
                float t = elapsed / animDuration;
                float progress = Mathf.Lerp(AUTH_PROGRESS_START, AUTH_PROGRESS_START + 0.1f, t);
                UpdateProgress(progress, "인증 서비스 초기화 중...");
                
                yield return new WaitForSeconds(0.05f);
                elapsed += 0.05f;
            }
            
            // 3. Unity 서비스 초기화
            UpdateProgress(AUTH_PROGRESS_START + 0.1f, "Unity 서비스 초기화 중...");
            
            // Unity 서비스 초기화 작업 시작
            var initTask = Unity.Services.Core.UnityServices.InitializeAsync();
            
            // 작업 완료 대기
            while (!initTask.IsCompleted)
            {
                yield return null;
            }
            
            LogDebug("[UI_StartUpScene] Unity 서비스 초기화 완료");
            
            // 4. 인증 수행
            bool success = false;
            bool isAlreadyAuthenticated = false;
            
            // 이미 인증되어 있는지 확인
            try
            {
                isAlreadyAuthenticated = _authManager.IsAuthenticated;
                if (isAlreadyAuthenticated)
                {
                    LogDebug($"[UI_StartUpScene] 이미 인증됨: 플레이어 ID = {_authManager.PlayerId}");
                    success = true;
                }
            }
            catch (System.Exception e)
            {
                LogError($"[UI_StartUpScene] 인증 상태 확인 중 오류: {e.Message}");
                isAlreadyAuthenticated = false;
            }
            
            // 인증이 필요한 경우
            if (!isAlreadyAuthenticated)
            {
                // 인증 시도
                UpdateProgress(AUTH_PROGRESS_START + 0.3f, "인증 중...");
                
                // 인증 작업 시작
                var authTask = _authManager.InitializeAndAuthenticateAsync();
                
                // 인증 작업 완료 대기
                while (!authTask.IsCompleted)
                {
                    yield return null;
                }
                
                // 결과 확인
                try
                {
                    success = authTask.Result;
                }
                catch (System.Exception e)
                {
                    LogError($"[UI_StartUpScene] 인증 중 오류: {e.Message}");
                    UpdateDebugInfo($"인증 오류: {e.Message}");
                    success = false;
                }
            }
            
            // 5. 결과 처리
            if (success)
            {
                _isAuthenticated = true;
                LogDebug($"[UI_StartUpScene] 인증 성공: 플레이어 ID = {_authManager.PlayerId}");
                UpdateProgress(AUTH_PROGRESS_END, $"인증 완료: {_authManager.PlayerId}");
            }
            else
            {
                LogError("[UI_StartUpScene] 인증 실패");
                UpdateProgress(AUTH_PROGRESS_END, "인증 실패 (오프라인 모드)");
            }
            
            LogDebug("[UI_StartUpScene] 인증 단계 완료");
        }
        
        /// <summary>
        /// 완료 단계 처리 (90% ~ 100%)
        /// </summary>
        private IEnumerator ProcessCompleteStep()
        {
            _currentStep = LoadingStep.Complete;
            
            // 완료 단계에서 진행률 서서히 증가
            float elapsed = 0f;
            float duration = 1.0f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float progress = Mathf.Lerp(COMPLETE_PROGRESS_START, COMPLETE_PROGRESS_END, t);
                UpdateProgress(progress, "Finish");
                
                yield return new WaitForSeconds(0.05f);
                elapsed += 0.05f;
            }
            
            // 마지막에 정확한 종료 진행률로 설정
            UpdateProgress(COMPLETE_PROGRESS_END, "Finish");
        }
        
        /// <summary>
        /// ResourceManager 이벤트 핸들러 - 로드 완료
        /// </summary>
        private void OnResourceLoadingComplete()
        {
            _isResourceLoaded = true;
            // 완료 처리는 InitializeLoadingProcess 코루틴에서 수행
        }
        
        #endregion
    }
} 