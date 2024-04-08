using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public static bool IsSigningOut;
    public static Action OnOpened;
    public static Action OnClosed;
    
    [SerializeField] private Button closeButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button showProfile;
    [SerializeField] private PlayerSettings playerSettings;

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
        logoutButton.onClick.AddListener(Logout);
        showProfile.onClick.AddListener(ShowProfile);
        
        logoutButton.gameObject.SetActive(!SceneManager.IsGameplayScene);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
        logoutButton.onClick.RemoveListener(Logout);
        showProfile.onClick.RemoveListener(ShowProfile);
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
            IsSigningOut = true;
            PlayerPrefs.Save();
            SceneManager.LoadDataCollector();
        }
    }
}
