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
using UnityEngine.EventSystems;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.Assets.Scripts.UI
{
    /// <summary>
    /// 시작 화면 UI 관리 클래스
    /// </summary>
    public partial class UI_BasicGame : UI_Scene
    {
        #region Enums
        
        enum Texts
        {
            MonsterCount_T,
            Money_T,
            Upgrade_Money_T,
            Summon_T,
            Timer_T,
            Wave_T,
            HeroCount_T
        }
        
        enum Images
        {
            Monster_Count_Fill
        }
        
        enum GameObjects
        {
            Main
        }

        enum Buttons
        {
            Summon_B
        }


        #endregion


        [Inject] private ObjectManager _objectManager;

        [Inject] private BasicGameState _basicGameState;

        
        // [Inject] private BasicGameManager _basicGameManager;
        // [Inject] private MainMenuScene _MainMenuScene;

        public int MonsterLimitCount = 100;
        private float _elapsedTime = 0.0f;
        private float _updateInterval = 1.0f;

        #region Properties
        
        [SerializeField] public UI_Spawn_Holder Spawn_Holder;

        // private GameObject MatchingObject => GetObject((int)GameObjects.Matching);
        private GameObject MainObject => GetObject((int)GameObjects.Main);
        #endregion

        #region Events
        
        // UI 이벤트 정의 - 다른 클래스에서 구독할 수 있는 정적 이벤트
        
        #endregion



        #region Initialization
        
        public override bool Init()
        {
            if (base.Init() == false)
                return false;

            BindTexts(typeof(Texts));
            BindImages(typeof(Images));
            BindObjects(typeof(GameObjects));
            BindButtons(typeof(Buttons));

            GetButton((int)Buttons.Summon_B).gameObject.BindEvent(OnClickSummonButton);
            // GetButton((int)Buttons.DiaPlusButton).gameObject.BindEvent(OnClickDiaPlusButton);
            // GetButton((int)Buttons.HeroesListButton).gameObject.BindEvent(OnClickHeroesListButton);
            // GetButton((int)Buttons.SetHeroesButton).gameObject.BindEvent(OnClickSetHeroesButton);
            // GetButton((int)Buttons.SettingButton).gameObject.BindEvent(OnClickSettingButton);
            // GetButton((int)Buttons.InventoryButton).gameObject.BindEvent(OnClickInventoryButton);
            // GetButton((int)Buttons.WorldMapButton).gameObject.BindEvent(OnClickWorldMapButton);
            // GetButton((int)Buttons.QuestButton).gameObject.BindEvent(OnClickQuestButton);
            // GetButton((int)Buttons.ChallengeButton).gameObject.BindEvent(OnClickChallengeButton);
            // GetButton((int)Buttons.PortalButton).gameObject.BindEvent(OnClickPortalButton);
            // GetButton((int)Buttons.CampButton).gameObject.BindEvent(OnClickCampButton);
            // GetButton((int)Buttons.CheatButton).gameObject.BindEvent(OnClickCheatButton);
            
            Refresh();

            return true;
        }
        

        void Refresh()
        {
            if (_init == false)
                return;
        }


        private void Update()
        {
            int monsterCount = _objectManager.MonsterRoot.childCount;
            GetText((int)Texts.MonsterCount_T).text = monsterCount.ToString() + "/" + MonsterLimitCount.ToString();
            GetImage((int)Images.Monster_Count_Fill).fillAmount = (float)monsterCount / MonsterLimitCount;
            GetText((int)Texts.Money_T).text = _basicGameState.Money.ToString();

            // GetText((int)Texts.Summon_T).text = _basicGameManager.SummonCount.ToString();
            // GetText((int)Texts.Upgrade_Money_T).text = _basicGameManager.UpgradeMoney.ToString();
            // GetText((int)Texts.Timer_T).text = _basicGameManager.GetBoss == false ? UpdateTimerText() : "In BOSS!";;
        
        

       
            // Debug.Log($"monsterCount: {monsterCount}");
            // _elapsedTime += Time.deltaTime;

            // if (_elapsedTime >= _updateInterval)
            // {
            //     float fps = 1.0f / Time.deltaTime;
            //     float ms = Time.deltaTime * 1000.0f;
            //     string text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
            //     GetText((int)Texts.GoldCountText).text = text;

            //     _elapsedTime = 0;
            // }
        }

    // string UpdateTimerText()
    // {
    //     int minutes = Mathf.FloorToInt(_basicGameManager.Timer / 60);
    //     int seconds = Mathf.FloorToInt(_basicGameManager.Timer % 60);

    //     return $"{minutes : 00}:{seconds :00}";
    // }

        protected override void SubscribeEvents()
        {
            base.SubscribeEvents();
        }
        
        protected override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents(); // 부모 클래스의 구현 호출

        }

        public static event Action OnSummonButtonRequested;

        private void OnClickSummonButton(PointerEventData evt)
        {
            OnSummonButtonRequested?.Invoke();
            Debug.Log("[UI_BasicGame] OnClickSummonButton");
        }

        protected override void OnDestroy()
        {
            Debug.Log("[UI_MainMenu] OnDestroy 메서드 호출됨");
            UnsubscribeEvents();
            
            // 부모 클래스의 OnDestroy 호출
            base.OnDestroy();
        }
        

        
        #endregion

  

        #region Editor Methods
        
        #if UNITY_EDITOR
        [ContextMenu("로그: Matching 하위 객체 출력")]
        private void LogMatchingChildrenMenu()
        {
        }
        
        // Inspector에서 버튼으로 표시되는 메서드
        [CustomEditor(typeof(UI_BasicGame))]
        public class UI_BasicGameEditor : DebugComponents.UIDebugEditorBase
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                
                UI_BasicGame script = (UI_BasicGame)target;
                
                // UIDebugLogger의 에디터 확장 기능 사용
                AddDebugButtons(script.gameObject, "-", "[UI_BasicGame]");
            }
        }
        #endif
        
        #endregion
    }
}

