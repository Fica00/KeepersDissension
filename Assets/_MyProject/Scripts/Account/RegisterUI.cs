using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RegisterUI : MonoBehaviour, IPanel
{
    [SerializeField] private GameObject holder;
    [SerializeField] private new TMP_InputField name;
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_InputField confirmPassword;
    [SerializeField] private Button register;
    [SerializeField] private Button signUp;
    [SerializeField] private Button anonymous;
    
    private void OnEnable()
    {
        signUp.onClick.AddListener(SignUp);
        register.onClick.AddListener(Register);
        anonymous.onClick.AddListener(Anonymous);
    }

    private void OnDisable()
    {
        signUp.onClick.RemoveListener(SignUp);
        register.onClick.RemoveListener(Register);
        anonymous.onClick.RemoveListener(Anonymous);
    }
    
    private void SignUp()
    {
        AuthenticationHandler.Instance.ShowLogin();
    }
    
    private void Register()
    {
        string _email = email.text;
        string _password = password.text;
        string _confirmPassword = confirmPassword.text;
        string _name = name.text;

        if (!CredentialsValidator.ValidateCredentials(_email,_password))
        {
            return;
        }

        if (_password!=_confirmPassword)
        {
            DialogsManager.Instance.ShowOkDialog("Passwords don't match");
            return;
        }

        if (!CredentialsValidator.ValidateName(_name))
        {
            return;
        }

        AuthenticationCredentials _credentials = new AuthenticationCredentials { Email = _email, Password = _password };
        AuthenticationHandler.Instance.TryToRegister(_credentials, _name);
        HandleInteractables(false);
    }

    private void Anonymous()
    {
        DialogsManager.Instance.ShowYesNoDialog("Your progress won't be saved and your rank won't be recorded.\nContinue?",DoAnonymousAuth);
    }

    private void DoAnonymousAuth()
    {
        HandleInteractables(false);
        AuthenticationCredentials _credentials = new AuthenticationCredentials { Email = GetEmail(), Password = GetPassword() , IsAnonymous = true};
        string _playerName = "Player" + Random.Range(1000, 10000);
        AuthenticationHandler.Instance.TryToRegister(_credentials, _playerName);
        return;

        string GetEmail()
        {
            string _randomEmail = CreateRandom();
            string _email = string.Empty;
            for (int _i = 0; _i < 8; _i++)
            {
                _email += _randomEmail[_i];
            }
        
            return _email + "@keepers.com";
        }

        string GetPassword()
        {
            string _randomPass = CreateRandom();
            string _password = string.Empty;
            for (int _i = 0; _i < 8; _i++)
            {
                _password += _randomPass[_i];
            }

            return _password;
        }

        string CreateRandom()
        {
            return Guid.NewGuid().ToString();
        }
    }
    
    public void Setup()
    {
        holder.SetActive(true);
        HandleInteractables(true);
    }
    
    private void HandleInteractables(bool _status)
    {
        name.interactable = _status;
        email.interactable = _status;
        password.interactable = _status;
        confirmPassword.interactable = _status;
        register.interactable = _status;
        signUp.interactable = _status;
    }

    public void Close()
    {
        holder.SetActive(false);
    }
}