using TMPro;
using UnityEngine;

public class SettingsUI : UI_Base
{
    public TextMeshProUGUI GameVersionTxt;
    public GameObject BGMOnToggle;
    public GameObject BGMOffToggle;
    public GameObject SFXOnToggle;
    public GameObject SFXOffToggle;

    private const string PRIVACY_POLICY_URL = "https://www.inflearn.com/";

    // public override void SetInfo(BaseUIData uiData)
    // {
        // base.SetInfo(uiData);

        // SetGameVersion();

        // var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        // if (userSettingsData != null)
        // {
        //     SetBGMSetting(userSettingsData.BGM);
        //     SetSFXSetting(userSettingsData.SFX);
        // }
    // }
    public void SetInfo()
    {

    }

    private void SetGameVersion()
    {
        GameVersionTxt.text = $"Version:{Application.version}";
    }

    private void SetBGMSetting(bool bgm)
    {
        BGMOnToggle.SetActive(bgm);
        BGMOffToggle.SetActive(!bgm);
    }

    private void SetSFXSetting(bool sfx)
    {
        SFXOnToggle.SetActive(sfx);
        SFXOffToggle.SetActive(!sfx);
    }

    public void OnClickBGMOnToggle()
    {
        Debug.Log($"{GetType()}::OnClickBGMOnToggle");

        // AudioManager.Instance.PlaySFX(SFX.ui_button_click);

        // var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        // if (userSettingsData != null)
        // {
        //     userSettingsData.BGM = false;
        //     userSettingsData.SaveData();
        //     AudioManager.Instance.MuteBGM();
        //     SetBGMSetting(userSettingsData.BGM);
        // }
    }

    public void OnClickBGMOffToggle()
    {
        Debug.Log($"{GetType()}::OnClickBGMOffToggle");

        // AudioManager.Instance.PlaySFX(SFX.ui_button_click);

        // var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        // if (userSettingsData != null)
        // {
        //     userSettingsData.BGM = true;
        //     userSettingsData.SaveData();
        //     AudioManager.Instance.UnMuteBGM();
        //     SetBGMSetting(userSettingsData.BGM);
        // }
    }

    public void OnClickSFXOnToggle()
    {
        Debug.Log($"{GetType()}::OnClickSFXOnToggle");

        // AudioManager.Instance.PlaySFX(SFX.ui_button_click);

        // var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        // if (userSettingsData != null)
        // {
        //     userSettingsData.SFX = false;
        //     userSettingsData.SaveData();
        //     AudioManager.Instance.MuteSFX();
        //     SetSFXSetting(userSettingsData.SFX);
        // }
    }

    public void OnClickSFXOffToggle()
    {
        Debug.Log($"{GetType()}::OnClickSFXOffToggle");

        // AudioManager.Instance.PlaySFX(SFX.ui_button_click);

        // var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        // if (userSettingsData != null)
        // {
        //     userSettingsData.SFX = true;
        //     userSettingsData.SaveData();
        //     AudioManager.Instance.UnMuteSFX();
        //     SetSFXSetting(userSettingsData.SFX);
        // }
    }

    public void OnClickPrivacyPolicyBtn()
    {
        Debug.Log($"{GetType()}::OnClickPrivacyPolicyBtn");
        
        // AudioManager.Instance.PlaySFX(SFX.ui_button_click);
        Application.OpenURL(PRIVACY_POLICY_URL);
    }

    public void OnClickLogoutBtn()
    {
        Debug.Log($"{GetType()}::OnClickLogoutBtn");

        // FirebaseManager.Instance.SignOut();
    }
}
