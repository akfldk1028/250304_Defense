using UnityEngine;

public class LoginUI : UI_Base
{
    public void OnClickSignInWithGoogle()
    {
        Debug.Log($"{GetType()::OnClickSignInWithGoogle}");

        // FirebaseManager.Instance.SignInWithGoogle();
        // CloseUI();
    }

    public void OnClickSignInWithApple()
    {
        Debug.Log($"{GetType()::OnClickSignInWithApple}");

        // FirebaseManager.Instance.SignInWithApple();
        // CloseUI();
    }
}
