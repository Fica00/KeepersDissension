using System;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCardImageDisplay : MonoBehaviour
{
    public static Action<CardBase> OnSelected;
    
    [SerializeField] private Image display;
    [SerializeField] private Button choose;
    private CardBase showingCard;

    private void OnEnable()
    {
        choose.onClick.AddListener(Select);
    }

    private void OnDisable()
    {
        choose.onClick.RemoveListener(Select);
    }

    private void Select()
    {
        OnSelected?.Invoke(showingCard);
    }

    public void Setup(CardBase _ability)
    {
        showingCard = _ability;
        if (_ability is AbilityCard _abilityCard)
        {
            display.sprite = _abilityCard.Details.Foreground;
        }
        else if (_ability is Card _card)
        {
            display.sprite = _card.Details.Foreground;
        }
    }
}
