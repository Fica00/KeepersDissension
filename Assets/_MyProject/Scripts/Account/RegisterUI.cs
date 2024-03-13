using FirebaseAuthHandler;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterUI : MonoBehaviour, IPanel
{
    [SerializeField] private GameObject holder;
    [SerializeField] private new TMP_InputField name;
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_InputField confirmPassword;
    [SerializeField] private Button register;
    [SerializeField] private Button signIp;
    
    public void Setup()
    {
        holder.SetActive(true);
    }

    private void OnEnable()
    {
        register.onClick.AddListener(Register);
        signIp.onClick.AddListener(SignUp);
    }

    private void OnDisable()
    {
        register.onClick.RemoveListener(Register);
        signIp.onClick.RemoveListener(SignUp);
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
            UIManager.Instance.ShowOkDialog("Passwords don't match");
            return;
        }

        if (ValidateName(_name))
        {
            ManageInteractables(false);
            FirebaseManager.Instance.Authentication.SignUpEmail(_email,_password, HandleRegister);
        }
    }

    private void HandleRegister(SignInResult _result)
    {
        if (!_result.IsSuccessful)
        {        
            ManageInteractables(true);
            UIManager.Instance.ShowOkDialog("Email already in use");
            return;
        }

        DataManager.SaveAuthenticationCredentials(email.text,password.text);
        DataManager.Instance.CreateNewPlayer();
        DataManager.Instance.PlayerData.Name = name.text;
        FirebaseManager.Instance.UpdatePlayerData(JsonConvert.SerializeObject(DataManager.Instance.PlayerData),HandleFinishedUpdatingPlayerData);
    }

    private void HandleFinishedUpdatingPlayerData(bool _result)
    {
        if (!_result)
        {
            UIManager.Instance.ShowOkDialog("Something went wrong while setting starting data");
            return;
        }

        Initializator.Instance.CollectData();
    }

    private bool ValidateName(string _name)
    {
        if (string.IsNullOrEmpty(_name))
        {
            UIManager.Instance.ShowOkDialog("Please enter name");
            return false;
        }

        if (_name.Length is < 4 or > 10)
        {
            UIManager.Instance.ShowOkDialog("Name must be between 4 and 10 characters");
            return false;
        }
        
        return true;
    }

    private void SignUp()
    {
        AuthenticationUI.Instance.ShowLogin();
    }

    private void ManageInteractables(bool _status)
    {
        name.interactable = _status;
        email.interactable = _status;
        password.interactable = _status;
        confirmPassword.interactable = _status;
        register.interactable = _status;
        signIp.interactable = _status;
    }

    public void Close()
    {
        holder.SetActive(false);
    }
}
