using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Assets.Scripts.Scene;
using VContainer;
using Unity.Assets.Scripts.Resource;
using Object = UnityEngine.Object;
using Unity.Netcode;
using Unity.Assets.Scripts.UI;
using Unity.Netcode.Transports.UTP;
using Unity.Assets.Scripts.Network;

namespace Unity.Assets.Scripts.Scene
{
public class MainMenuScene : BaseScene
{
    
    [Inject] private NetworkManager _networkManager;
    [Inject] private ConnectionManager _connectionManager;
    // 서버 연결 정보
	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		SceneType = EScene.MainMenu;

        // UI 이벤트 구독
        SubscribeEvents();

		return true;
	}


	public override void Clear()
	{
        // UI 이벤트 구독 해제
        UnsubscribeEvents();
	}

    private void OnEnable()
    {
        // UI 이벤트 구독
        SubscribeEvents();
    }

    private void OnDisable()
    {
        // UI 이벤트 구독 해제
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        // UI_MainMenu의 정적 이벤트 구독
        UI_MainMenu.OnRandomMatchRequested += OnRandomMatchRequested;
        Debug.Log("[MainMenuScene] 이벤트 구독 완료");
    }

    private void UnsubscribeEvents()
    {
        // UI_MainMenu의 정적 이벤트 구독 해제
        UI_MainMenu.OnRandomMatchRequested -= OnRandomMatchRequested;
        Debug.Log("[MainMenuScene] 이벤트 구독 해제");
    }

    // 이벤트 핸들러
    private void OnRandomMatchRequested()
    {
        _connectionManager.StartHostLobby();
        // 연결 상태 변경을 구독하고 연결 성공 시 씬 전환
        _connectionManager.OnConnectionStatusChanged += OnConnectionStatusChanged;
    }

    private void OnConnectionStatusChanged(ConnectStatus status)
    {
        if (status == ConnectStatus.Connected)
        {
            _connectionManager.OnConnectionStatusChanged -= OnConnectionStatusChanged;
            _sceneManager.LoadScene(EScene.BasicGame);
        }
    }
}

}