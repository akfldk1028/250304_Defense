// using System.Collections;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class TitleManager : MonoBehaviour
// {
//     //�ΰ�
//     public Animation LogoAnim;
//     public TextMeshProUGUI LogoTxt;

//     //Ÿ��Ʋ
//     public GameObject Title;
//     public Slider LoadingSlider;
//     public TextMeshProUGUI LoadingProgressTxt;

//     private AsyncOperation m_AsyncOperation;

//     private void Awake()
//     {
//         LogoAnim.gameObject.SetActive(true);
//         Title.SetActive(false);
//     }

//     private void Start()
//     {
//         //���� ������ �ε�
//         UserDataManager.Instance.LoadUserData();

//         //����� ���� �����Ͱ� ������ �⺻������ ���� �� ����
//         if (!UserDataManager.Instance.ExistsSavedData)
//         {
//             UserDataManager.Instance.SetDefaultUserData();
//             UserDataManager.Instance.SaveUserData();
//         }

//         AudioManager.Instance.OnLoadUserData();
//         UIManager.Instance.EnableGoodsUI(false);

//         StartCoroutine(LoadGameCo());
//     }

//     private IEnumerator LoadGameCo()
//     {
//         Logger.Log($"{GetType()}::LoadGameCo");

//         LogoAnim.Play();
//         yield return new WaitForSeconds(LogoAnim.clip.length);

//         LogoAnim.gameObject.SetActive(false);
//         Title.SetActive(true);

//         //Check third party service init
//         if (!CheckThirdPartyServiceInit())
//             yield break;

//         //Validate app version
//         if(!ValidateAppVersion())
//             yield break;

//         //Check sign in
//         if(!FirebaseManager.Instance.IsSignedIn())
//         {
//             var uiData = new BaseUIData();
//             UIManager.Instance.OpenUI<LoginUI>(uiData);
//         }

//         //Wait until user is signed in
//         while (!FirebaseManager.Instance.IsSignedIn())
//         {
//             yield return null;
//         }

//         yield return StartCoroutine(LoadLobbyCo());
//     }

//     private bool CheckThirdPartyServiceInit()
//     {
//         return FirebaseManager.Instance.IsInit();
//     }

//     private bool ValidateAppVersion()
//     {
//         bool result = false;

//         if(Application.version == FirebaseManager.Instance.GetAppVersion())
//         {
//             result = true;
//         }
//         else
//         {
//             var uiData = new ConfirmUIData();
//             uiData.ConfirmType = ConfirmType.OK_CANCEL;
//             uiData.TitleTxt = string.Empty;
//             uiData.DescTxt = "App version is outdated. Will you update your app?";
//             uiData.OKBtnTxt = "Update";
//             uiData.CancelBtnTxt = "Cancel";
//             uiData.OnClickOKBtn = () =>
//             {
// #if UNITY_ANDROID
//                 Application.OpenURL(GlobalDefine.GOOGLE_PLAY_STORE);
// #elif UNITY_IOS
//                 Application.OpenURL(GlobalDefine.APPLE_APP_STORE);
// #endif
//             };
//             uiData.OnClickCancelBtn = () =>
//             {
//                 Application.Quit();
//             };
//             UIManager.Instance.OpenUI<ConfirmUI>(uiData);
//         }

//         return result;
//     }

//     private IEnumerator LoadLobbyCo()
//     {
//         m_AsyncOperation = SceneLoader.Instance.LoadSceneAsync(SceneType.Lobby);
//         if (m_AsyncOperation == null)
//         {
//             Logger.LogError("Lobby async loading error.");
//             yield break;
//         }

//         m_AsyncOperation.allowSceneActivation = false;

//         /*
//          * �ε� �ð��� ª�� ��� �ε� �����̴� ��ȭ�� �ʹ� ���� ������ ���� �� �ִ�.
//          * �Ϻη� �� �� �� 50%�� ���������ν� �ð������� �� �ڿ������� ó���Ѵ�.
//          */
//         LoadingSlider.value = 0.5f;
//         LoadingProgressTxt.text = ((int)(LoadingSlider.value * 100)).ToString();
//         yield return new WaitForSeconds(0.5f);

//         while (!m_AsyncOperation.isDone) //�ε��� ���� ���� ��
//         {
//             //�ε� �����̴� ������Ʈ
//             LoadingSlider.value = m_AsyncOperation.progress < 0.5f ? 0.5f : m_AsyncOperation.progress;
//             LoadingProgressTxt.text = ((int)(LoadingSlider.value * 100)).ToString();

//             //�� �ε��� �Ϸ�Ǿ��ٸ� �κ�� ��ȯ ó���ϰ� �ڷ�ƾ ����
//             //https://docs.unity3d.com/ScriptReference/AsyncOperation-progress.html
//             if (m_AsyncOperation.progress == 0.9f)
//             {
//                 m_AsyncOperation.allowSceneActivation = true;
//                 yield break;
//             }

//             yield return null;
//         }
//     }
// }
