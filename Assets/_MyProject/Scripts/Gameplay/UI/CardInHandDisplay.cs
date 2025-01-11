using System;
using UnityEngine;
using UnityEngine.UI;

public class CardInHandDisplay : MonoBehaviour
{
    public static Action<CardBase> OnClicked;
    [SerializeField] private Image image;
    [SerializeField] private Button button;

    public string UniqueId { get; private set; }
    private CardBase card;

    private void OnEnable()
    {
        button.onClick.AddListener(Select);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Select);
    }

    private void Select()
    {
        OnClicked?.Invoke(card);
    }

    public void Setup(string _uniqueId, bool _isAbility)
    {
        UniqueId = _uniqueId;
        card = _isAbility ? GameplayManager.Instance.GetAbilityCard(_uniqueId) : GameplayManager.Instance.GetCard(_uniqueId);
        if (_isAbility)
        {
            var _abilityCard = (AbilityCard)card;
            image.sprite = _abilityCard.Details.Foreground;
        }
        else
        {
            var _warriorCard = (Card)card;
            image.sprite = _warriorCard.Details.Foreground;
        }
    }
}
