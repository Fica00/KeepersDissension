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
        signUp.onClick.AddListener(SignUn);
        forgotPassword.onClick.AddListener(ForgotPassword);
        login.onClick.AddListener(Login);
    }

    private void OnDisable()
    {
        signUp.onClick.RemoveListener(SignUn);
        forgotPassword.onClick.RemoveListener(ForgotPassword);
        login.onClick.RemoveListener(Login);
    }
    
    private void SignUn()
    {
        AuthenticationHandler.Instance.ShowRegister();
    }

    private void ForgotPassword()
    {
        AuthenticationHandler.Instance.ShowForgotPassword();
    }

    private void Login()
    {
        string _email = email.text;
        string _password = password.text;
        if (!CredentialsValidator.ValidateCredentials(_email, _password))
        {
            return;
        }
        
        HandleInteractables(false);
        AuthenticationCredentials _credentials = new AuthenticationCredentials { Email = _email, Password = _password };
        AuthenticationHandler.Instance.TryLogin(_credentials);
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