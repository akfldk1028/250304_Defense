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


namespace Unity.Assets.Scripts.Scene
{
public class MainMenuScene : BaseScene
{
    [Inject] private SceneManagerEx _sceneManager;
    
    [Inject] private NetworkManager _networkManager;
    // 서버 연결 정보
    [SerializeField] private string _ipAddress = "127.0.0.1"; // 기본값은 로컬호스트
    [SerializeField] private ushort _port = 7777; // 기본 포트

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
        // NetworkManager에 UnityTransport가 있는지 확인하고 없으면 추가



        _networkManager.StartHost();
        Debug.Log("ȣ��Ʈ�� ���۵Ǿ����ϴϴ�");

        _networkManager.OnClientConnectedCallback += OnClientConnected;
        _networkManager.OnClientDisconnectCallback += OnHostDisconnected;
        _sceneManager.LoadScene(EScene.BasicGame);
    }

        private void OnClientConnected(ulong clientId)
    {
        // OnPlayerJoined();
    }

    private void OnHostDisconnected(ulong clientId)
    {
        if(clientId == _networkManager.LocalClientId && _networkManager.IsHost)
        {
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnClientDisconnectCallback -= OnHostDisconnected;
        }
    }


}

}
