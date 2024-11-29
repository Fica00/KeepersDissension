using System;
using System.Collections;
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
        OnOpened?.Invoke();
        gameObject.SetActive(true);
    }

    private void DeleteAccount()
    {
        FirebaseManager.Instance.DeleteUserDataAndAccount(HandleAccountDeleted);
    }

    private void HandleAccountDeleted(bool _result)
    {
        if (!_result)
        {
            DialogsManager.Instance.ShowOkDialog("Something went wrong please try again later");
            return;
        }
        
        DialogsManager.Instance.ShowOkDialog("Account successfully deleted", CloseGameAfterAccountDeletion);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    private void CloseGameAfterAccountDeletion()
    {
        Application.Quit();
    }

    private void Logout()
    {
        StartCoroutine(LogoutRoutine());
        IEnumerator LogoutRoutine()
        {
            logoutButton.interactable = false;
            closeButton.interactable = false;
            DataManager.Instance.PlayerData.DeviceId = string.Empty;
            yield return new WaitForSeconds(2);
            FirebaseManager.Instance.Authentication.SignOut();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            yield return new WaitForSeconds(2);
            SceneManager.LoadDataCollector();
        }
    }
}
