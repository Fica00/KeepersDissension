using System;
using UnityEngine;

public class CardSpecialAbility : MonoBehaviour
{
    public Action OnActivated;
    [field: SerializeField] public bool IsClickable { get; protected set; }
    public bool IsMy => Card.My;
    public Card Card { get; private set; }
    public bool CanUseAbility {get; set; } =  true;
    [field: SerializeField]public Sprite Sprite { get; protected set; }
    [HideInInspector] public bool IsBaseCardsEffect=true;

    protected CardBase CardBase;
  
    protected GameplayPlayer Player =>
        Card.My ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;
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
}
