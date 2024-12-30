using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivationFieldHandler : MonoBehaviour
{
    public static Action OnShowed;
    public static Action OnHided;
    
    [SerializeField] private GameObject holder;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform cardsHolder;
    
    private void OnEnable()
    {
        TablePlaceHandler.OnPlaceClicked += CheckPlace;
        CardTableInteractions.OnPlaceClicked += CheckPlace;
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        TablePlaceHandler.OnPlaceClicked -= CheckPlace;
        CardTableInteractions.OnPlaceClicked -= CheckPlace;
        closeButton.onClick.RemoveListener(Close);
    }

    private void CheckPlace(TablePlaceHandler _clickedPlace)
    {
        if (_clickedPlace.Id != 65 && _clickedPlace.Id != -1)
        {
            return;
        }

        Debug.Log("Should show cards");
        ShowCards(_clickedPlace.GetCards());
    }

    private void ShowCards(List<CardBase> _cardBase)
    {
        if (_cardBase==null || _cardBase.Count == 0)
        {
            return;
        }
        
        holder.SetActive(true);
        OnShowed?.Invoke();
    }

    private void Close()
    {
        holder.SetActive(false);
        OnHided?.Invoke();
    }

}