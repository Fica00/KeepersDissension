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

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
        logoutButton.onClick.AddListener(Logout);
        
        logoutButton.gameObject.SetActive(!SceneManager.IsGameplayScene);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
        logoutButton.onClick.RemoveListener(Logout);
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
            SceneManager.LoadDataCollector();
        }
    }
}
