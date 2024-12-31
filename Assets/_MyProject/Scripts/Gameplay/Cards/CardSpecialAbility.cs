using System;
using UnityEngine;

public class CardSpecialAbility : MonoBehaviour
{
    [field: SerializeField] public bool IsClickable { get; protected set; }
    public bool IsMy => Card.My;
    public Card Card { get; private set; }
    public bool CanUseAbility => Card.CardData.WarriorAbilityData.CanUseAbility;
    [field: SerializeField]public Sprite Sprite { get; protected set; }
    [HideInInspector] public bool IsBaseCardsEffect=true;

    protected CardBase CardBase;
    protected TablePlaceHandler TablePlaceHandler => GetComponentInParent<TablePlaceHandler>();


    protected virtual void Awake()
    {
        CardBase = GetComponentInParent<CardBase>();
        Card = GetComponentInParent<Card>();
    }

    public virtual void UseAbility()
    {
        
    }

    public void Setup(bool _isClickable, Sprite _sprite)
    {
        IsClickable = _isClickable;
        Sprite = _sprite;
        CardBase = GetComponentInParent<CardBase>();
        Card = GetComponentInParent<Card>();
    }

    protected void SetCanUseAbility(bool _status)
    {
        Card.CardData.WarriorAbilityData.CanUseAbility = _status;
    }

    protected GameplayPlayer GetPlayer()
    {
        return Card.My ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;
    }
}
