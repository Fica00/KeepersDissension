using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public static Action OnOpened;
    public static Action OnClosed;
    
    [SerializeField] private Button closeButton;

    private void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
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
}
