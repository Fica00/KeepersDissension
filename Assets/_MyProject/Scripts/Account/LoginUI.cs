using FirebaseAuthHandler;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour, IPanel
{
    [SerializeField] private GameObject holder;
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private Button login;
    [SerializeField] private Button signUp;
    [SerializeField] private Button forgotPassword;
    
    public void Setup()
    {
        holder.SetActive(true);
        HandleInteractables(true);
    }

    private void OnEnable()
    {
        login.onClick.AddListener(Login);
        signUp.onClick.AddListener(SignUn);
        forgotPassword.onClick.AddListener(ForgotPassword);
    }

    private void OnDisable()
    {
        login.onClick.RemoveListener(Login);
        signUp.onClick.RemoveListener(SignUn);
        forgotPassword.onClick.RemoveListener(ForgotPassword);
    }

    private void Login()
    {
        string _email = email.text;
        string _password = password.text;
        if (CredentialsValidator.ValidateCredentials(_email,_password))
        {
            HandleInteractables(false);
            FirebaseManager.Instance.Authentication.SignInEmail(_email,_password,HandleLoginResult);
        }
    }

    private void HandleLoginResult(SignInResult _result)
    {
        if (!_result.IsSuccessful)
        {
            HandleInteractables(true);
            UIManager.Instance.ShowOkDialog(_result.Message);
            return;
        }
        
        DataManager.SaveAuthenticationCredentials(email.text,password.text);
        Initializator.Instance.CollectData();
    }

    private void SignUn()
    {
        AuthenticationUI.Instance.ShowRegister();
    }

    private void ForgotPassword()
    {
        AuthenticationUI.Instance.ShowForgotPassword();
    }

    private void HandleInteractables(bool _status)
    {
        email.interactable = _status;
        password.interactable = _status;
        login.interactable = _status;
        signUp.interactable = _status;
        forgotPassword.interactable = _status;
    }

    public void Close()
    {
        holder.SetActive(false);
    }
}
