using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : InitBase
{
    public ScrollRect m_ScrollRect;

    private void OnEnable()
    {
        m_ScrollRect.verticalNormalizedPosition = 1f;
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    // public override void SetInfo(BaseUIData uiData)
    // {
    //     base.SetInfo(uiData);
    // }
}
