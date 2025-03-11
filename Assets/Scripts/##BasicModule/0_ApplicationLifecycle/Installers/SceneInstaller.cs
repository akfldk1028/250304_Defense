using VContainer;
// using Unity.Assets.Scripts.Gameplay.GameplayObjects;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using Object = UnityEngine.Object;
using Unity.Assets.Scripts.Scene;
using VContainer.Unity;

namespace Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers
{
    /// <summary>
    /// 게임 데이터 모듈 인스톨러
    /// 게임 데이터 관련 의존성을 등록합니다.
    /// </summary>
    public class SceneInstaller : IModuleInstaller
    {

        /// <summary>
        /// 이 인스톨러가 담당하는 모듈 타입
        /// </summary>
        public ModuleType ModuleType => ModuleType.Scene;

        /// <summary>
        /// 의존성 주입 설정
        /// </summary>
        /// <param name="builder">컨테이너 빌더</param>
        public void Install(IContainerBuilder builder)
        {
            UnityEngine.Debug.Log("[SceneInstaller] 리소스 모듈 설치 시작");

            builder.Register<SceneManagerEx>(Lifetime.Singleton);
            
            // StartUpScene 등록 방식 변경
            // 1. 컴포넌트 찾기 방식으로 등록
            builder.RegisterComponentInHierarchy<StartUpScene>();

 
            UnityEngine.Debug.Log("[SceneInstaller] 리소스 모듈 설치 완료");
        }
    }
} 