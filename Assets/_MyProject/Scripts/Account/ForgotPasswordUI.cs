using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgotPasswordUI : MonoBehaviour, IPanel
{
    [SerializeField] private GameObject holder;
    [SerializeField] private TMP_InputField email;
    [SerializeField] private Button reset;
    [SerializeField] private Button signIn;

    public void Setup()
    {
        holder.SetActive(true);    
    }

    private void OnEnable()
    {
        reset.onClick.AddListener(Reset);
        signIn.onClick.AddListener(Login);
    }

    private void OnDisable()
    {
        reset.onClick.RemoveListener(Reset);
        signIn.onClick.RemoveListener(Login);
    }

    private void Reset()
    {
        string _email = email.text;
        if (CredentialsValidator.VerifyEmail(_email))
        {
            ManageInteractables(false);
            FirebaseManager.Instance.SendPasswordResetEmail(_email,HandlePasswordSentResult);
        }
    }

    private void HandlePasswordSentResult(bool _result)
    {
        ManageInteractables(true);
        if (_result)
        {
            UIManager.Instance.ShowOkDialog("Email with instructions to reset your password has been sent");
            Login();
        }
        else
        {
            UIManager.Instance.ShowOkDialog("Something went wrong, please check email or try again later");
        }
    }

    private void Login()
    {
        AuthenticationUI.Instance.ShowLogin();
    }

    private void ManageInteractables(bool _status)
    {
        email.interactable = _status;
        signIn.interactable = _status;
    }

    public void Close()
    {
        holder.SetActive(false);
    }
}
