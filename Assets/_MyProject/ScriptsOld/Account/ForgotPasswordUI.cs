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
        if (!CredentialsValidator.VerifyEmail(_email))
        {
            return;
        }
        
        ManageInteractables(false);
        AuthenticationHandler.Instance.SendPasswordReset(_email);
    }

    private void Login()
    {
        AuthenticationHandler.Instance.ShowLogin();
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