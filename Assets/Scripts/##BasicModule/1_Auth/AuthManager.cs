using UnityEngine;
using VContainer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;

namespace Unity.Assets.Scripts.Auth
{
    /// <summary>
    /// Unity 인증 서비스를 관리하는 클래스
    /// </summary>
    public class AuthManager
    {
        private const string PLAYER_PREFS_LAST_LOGIN_ID = "LastLoginId";
        private const string DEBUG_TAG = "[AuthService]";
        // [Inject]
        // IPublisher<UnityServiceErrorMessage> m_UnityServiceErrorMessagePublisher;
        public bool IsAuthenticated => Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn;
        public string PlayerId => Unity.Services.Authentication.AuthenticationService.Instance.PlayerId;

        /// <summary>
        /// Unity 서비스 초기화 및 인증을 처리합니다.
        /// </summary>
        public async Task<bool> InitializeAndAuthenticateAsync()
        {
            try
            {
                // Unity 서비스 초기화
                await global::Unity.Services.Core.UnityServices.InitializeAsync();

                // 이전 로그인 기록 확인
                string lastLoginId = PlayerPrefs.GetString(PLAYER_PREFS_LAST_LOGIN_ID, "없음");
                Debug.Log($"<color=yellow> {DEBUG_TAG} 마지막 로그인 ID: {lastLoginId}</color>");
                Debug.Log($"<color=yellow> {DEBUG_TAG} 디바이스 ID: {SystemInfo.deviceUniqueIdentifier}</color>");
                Debug.Log($"<color=yellow> {DEBUG_TAG} PlayerId: {PlayerId}</color>");
                Debug.Log($"<color=yellow> {DEBUG_TAG} IsAuthenticated: {IsAuthenticated}</color>");

                // 인증 시도
                if (!IsAuthenticated)
                {
                    // 이전 로그인 ID가 있으면 세션 토큰으로 시도
                    string savedLoginId = PlayerPrefs.GetString(PLAYER_PREFS_LAST_LOGIN_ID);
                    if (!string.IsNullOrEmpty(savedLoginId))
                    {
                        try
                        {
                            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
                            Debug.Log($"<color=yellow> {DEBUG_TAG} 기존 계정으로 로그인 시도: {savedLoginId}</color>");
                        }
                        catch
                        {
                            // 실패하면 새 계정으로
                            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
                        }
                    }
                    else
                    {
                        await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
                    }

                    string playerId = PlayerId;
                    Debug.Log($"<color=yellow>{DEBUG_TAG} 로그인 완료: {playerId}</color>");
                    Debug.Log($"<color=yellow>{DEBUG_TAG} IsAuthenticated@@@@: {IsAuthenticated}</color>");

                    // 로그인 ID 저장
                    PlayerPrefs.SetString(PLAYER_PREFS_LAST_LOGIN_ID, playerId);
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.Log($"<color=yellow>{DEBUG_TAG} IsAuthenticated: {IsAuthenticated}</color>");
                    Debug.Log($"<color=yellow>{DEBUG_TAG} 기존 계정으로 로그인: {PlayerId}</color>");
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>{DEBUG_TAG} 인증 실패: {e.Message}</color>");
                return false;
            }
        }

        /// <summary>
        /// 현재 로그인된 계정에서 로그아웃합니다.
        /// </summary>
        public void SignOut()
        {
            if (IsAuthenticated)
            {
                Unity.Services.Authentication.AuthenticationService.Instance.SignOut();
                Debug.Log($"<color=yellow>{DEBUG_TAG} 로그아웃 완료</color>");
            }
        }
        public async Task<bool> EnsurePlayerIsAuthorized()
        {
            if (AuthenticationService.Instance.IsAuthorized)
            {
                return true;
            }

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }
            catch (AuthenticationException e)
            {
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                Debug.LogError($"<color=red>{DEBUG_TAG} 인증 실패: {reason}</color>");
                // m_UnityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));

                //not rethrowing for authentication exceptions - any failure to authenticate is considered "handled failure"
                return false;
            }
            catch (Exception e)
            {
                //all other exceptions should still bubble up as unhandled ones
                var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})";
                Debug.LogError($"<color=red>{DEBUG_TAG} 인증 실패: {reason}</color>");
                // m_UnityServiceErrorMessagePublisher.Publish(new UnityServiceErrorMessage("Authentication Error", reason, UnityServiceErrorMessage.Service.Authentication, e));
                throw;
            }
        }
        /// <summary>
        /// 현재 인증 상태를 확인합니다.
        /// </summary>
        public AuthenticationState GetAuthenticationState()
        {
            if (!IsAuthenticated)
            {
                return new AuthenticationState 
                { 
                    IsAuthenticated = false,
                    PlayerId = null,
                    ErrorMessage = "인증되지 않은 상태"
                };
            }

            return new AuthenticationState
            {
                IsAuthenticated = true,
                PlayerId = PlayerId,
                ErrorMessage = null
            };
        }
    }

    /// <summary>
    /// 인증 상태를 나타내는 구조체
    /// </summary>
    public struct AuthenticationState
    {
        public bool IsAuthenticated { get; set; }
        public string PlayerId { get; set; }
        public string ErrorMessage { get; set; }
    }
} 
