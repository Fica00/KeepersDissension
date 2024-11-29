using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ArrowPanel : MonoBehaviour
{
    public static Action OnShow;
    public static Action OnHide;
    
    public Action OnOpened;
    public Action OnClosed;
    
    [SerializeField] private Button showButton;
    [SerializeField] private Transform holder;
    [SerializeField] private Button closeButton;
    [SerializeField] private Vector3 closedScale;

    private void OnEnable()
    {
        showButton.onClick.AddListener(Show);
        closeButton.onClick.AddListener(Hide);
    }

    private void OnDisable()
    {
        showButton.onClick.RemoveListener(Show);
        closeButton.onClick.RemoveListener(Hide);
    }

    public void Show()
    {
        OnOpened?.Invoke();
        OnShow?.Invoke();
        holder.DOScale(Vector3.one, 0.5f);
        showButton.gameObject.SetActive(false);
    }

    public void Hide()
    {
        OnClosed?.Invoke();
        OnHide?.Invoke();
        holder.DOScale(closedScale, 0.5f);
        showButton.gameObject.SetActive(true); 
    }
    
}
