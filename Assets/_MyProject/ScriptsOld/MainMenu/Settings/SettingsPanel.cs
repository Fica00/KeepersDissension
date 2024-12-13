using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public static Action OnOpened;
    public static Action OnClosed;
    
    [SerializeField] private Button closeButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button showProfile;
    [SerializeField] private Button deleteAccount;
    [SerializeField] private PlayerSettings playerSettings;

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
        logoutButton.onClick.AddListener(Logout);
        showProfile.onClick.AddListener(ShowProfile);
        deleteAccount.onClick.AddListener(DeleteAccount);
        
        logoutButton.gameObject.SetActive(!SceneManager.IsGameplayScene);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
        logoutButton.onClick.RemoveListener(Logout);
        showProfile.onClick.RemoveListener(ShowProfile);
        deleteAccount.onClick.RemoveListener(DeleteAccount);
    }

    private void ShowProfile()
    {
        playerSettings.Setup();
    }

    private void Close()
    {
        OnClosed?.Invoke();
        gameObject.SetActive(false);
    }

    public void Setup()
    {
        ManageInteractables(true);
        OnOpened?.Invoke();
        gameObject.SetActive(true);
    }

    private void DeleteAccount()
    {
        ManageInteractables(false);
        FirebaseManager.Instance.DeleteUserDataAndAccount(HandleAccountDeleted);
    }

    private void HandleAccountDeleted(bool _result)
    {
        if (!_result)
        {
            ManageInteractables(true);
            DialogsManager.Instance.ShowOkDialog("Something went wrong please try again later");
            return;
        }
        
        DialogsManager.Instance.ShowOkDialog("Account successfully deleted", Application.Quit);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private void Logout()
    {
        ManageInteractables(false);
        FirebaseManager.Instance.SignOut(SceneManager.LoadDataCollector);
    }

    private void ManageInteractables(bool _status)
    {
        logoutButton.interactable = _status;
        closeButton.interactable = _status;
        deleteAccount.interactable = _status;
    }
}
