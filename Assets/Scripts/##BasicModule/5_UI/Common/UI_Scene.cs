using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Unity.Assets.Scripts.UI;
using UnityEngine.UI;
using UnityEngine.Events;

public class UI_Scene : UI_Base
{
    // 필드 이름을 변경하여 중복 주입 방지
    [Inject] protected UIManager uiManager;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // null 체크 추가
        if (uiManager != null)
        {
            uiManager.SetCanvas(gameObject, false);
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[{GetType().Name}] uiManager가 null입니다. DI가 제대로 설정되지 않았을 수 있습니다.</color>");
        }
        
        // 이벤트 구독 호출
        SubscribeEvents();
        
        return true;
    }
    
    protected virtual void OnDestroy()
    {
        // 이벤트 구독 해제 호출
        UnsubscribeEvents();
    }
    
    /// <summary>
    /// 모든 UI 이벤트를 구독합니다.
    /// </summary>
    protected virtual void SubscribeEvents()
    {
        uiManager.SubscribeEvents();

    }
    
    /// <summary>
    /// 모든 UI 이벤트 구독을 해제합니다.
    /// </summary>
    protected virtual void UnsubscribeEvents()
    {
        uiManager.UnsubscribeEvents();

    }
    




    /// <summary>
    /// 특정 이름의 하위 요소를 활성화합니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">하위 요소의 이름</param>
    protected void ShowChild(GameObject parent, string childName)
    {
        Util.ShowChild(parent, childName);
    }

    /// <summary>
    /// 특정 이름의 하위 요소를 비활성화합니다.
    /// </summary>
    /// <param name="parent">부모 GameObject</param>
    /// <param name="childName">하위 요소의 이름</param>
    protected void HideChild(GameObject parent, string childName)
    {
        Util.HideChild(parent, childName);
    }


}
