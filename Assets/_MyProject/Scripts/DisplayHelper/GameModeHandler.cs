using UnityEngine;
using UnityEngine.UI;
using System;

public class GameModeHandler : MonoBehaviour
{
    [SerializeField] private Button normalGame;
    [SerializeField] private Button friendlyGame;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button debugButton;
    
    private Action<MatchMode> callback;
    
    private void OnEnable()
    {
        normalGame.onClick.AddListener(ChooseNormal);
        friendlyGame.onClick.AddListener(ChoosePrivate);
        cancelButton.onClick.AddListener(Cancel);
        debugButton.onClick.AddListener(DebugMode);
    }

    private void OnDisable()
    {
        normalGame.onClick.RemoveListener(ChooseNormal);
        friendlyGame.onClick.RemoveListener(ChoosePrivate);
        cancelButton.onClick.RemoveListener(Cancel);
        debugButton.onClick.RemoveListener(DebugMode);
    }

    private void Cancel()
    {
        callback?.Invoke(MatchMode.None);
    }

    private void ChooseNormal()
    {
        callback?.Invoke(MatchMode.Normal);
    }

    private void ChoosePrivate()
    {
        callback?.Invoke(MatchMode.Private);
    }

    private void DebugMode()
    {
        callback?.Invoke(MatchMode.Debug);
    }

    public void Setup(Action<MatchMode> _callBack)
    {
        callback = _callBack;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
