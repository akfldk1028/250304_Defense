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

            // 4. 완료 단계
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
            for (float t = 0; t < 1.0f; t += 0.1f)
            {
                float initProgress = Mathf.Lerp(INIT_PROGRESS_START, INIT_PROGRESS_END, t);
                UpdateProgress(initProgress, "초기화 중...");
                yield return new WaitForSeconds(0.05f);
            }
        }
        
        /// <summary>
        /// 리소스 로드 단계 처리 (30% ~ 90%)
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
                    
                    // 진행률 계산 및 업데이트 (30% ~ 90% 범위로 조정)
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
            
            // 리소스 로드가 완료될 때까지 대기
            // while (!_isResourceLoaded)
            // {
            //     // 로딩 중 상태 업데이트
            //     if (totalResourceCount > 0)
            //     {
            //         UpdateDebugInfo($"Resource Load: {loadedResourceCount}/{totalResourceCount} ({(float)loadedResourceCount/totalResourceCount:P0})");
            //     }
            //     yield return null;
            // }
            
            // 리소스 로드 완료 시 진행률 90%로 설정
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
        /// 완료 단계 처리 (90% ~ 100%)
        /// </summary>
        private IEnumerator ProcessCompleteStep()
        {
            _currentStep = LoadingStep.Complete;
            
            // 완료 단계에서 진행률 서서히 증가
            for (float t = 0; t < 1.0f; t += 0.1f)
            {
                float completeProgress = Mathf.Lerp(COMPLETE_PROGRESS_START, COMPLETE_PROGRESS_END, t);
                UpdateProgress(completeProgress, "Finish");
                yield return new WaitForSeconds(0.05f);
            }
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