using System;
using System.Linq;
using UnityEngine;

public class GameplayPlayer : MonoBehaviour
{
    public Action OnStartedTurn;
    public Action OnEndedTurn;
    public Action OnUpdatedActions;
    
    [SerializeField] private TableSideHandler tableSideHandler;
    [SerializeField] private CardsInHandHandler cardsInHandHandler;
    [SerializeField] private Transform destroyedCardsHolder;

    public TableSideHandler TableSideHandler => tableSideHandler;

    private bool isMy;
    private FactionSO factionSo;
    public bool IsMy => isMy;
    
    public FactionSO FactionSo => factionSo;

    public int Actions
    {
        get => GameplayManager.Instance.AmountOfActions(IsMy);
        set
        {
            if (GameplayCheats.UnlimitedActions)
            {
                return;
            }
            GameplayManager.Instance.SetAmountOfActions(value, IsMy);
            OnUpdatedActions?.Invoke();
        }
    }

    public void Setup(int _factionId, bool _isMy)
    {
        isMy = _isMy;
        factionSo = FactionSO.Get(_factionId);
        tableSideHandler.Setup(this);
        cardsInHandHandler.Setup(this);
    }

    public void NewTurn()
    {
        Actions = GameplayManager.Instance.AmountOfActionsPerTurn();
        foreach (var _card in GameplayManager.Instance.GetAllCards())
        {
            if (_card.GetIsMy() != isMy)
            {
                continue;
            }

            _card.CardData.HasSnowWallEffect = false;
        }

        if (GameplayManager.Instance.IsAbilityActive<SlowDown>())
        {
            SlowDown _slowDown = FindObjectOfType<SlowDown>();
            _slowDown.ClearEffectedCardsForMe();
        }
        OnStartedTurn?.Invoke();
    }

    public void EndedTurn()
    {
        Actions = 0;
        OnEndedTurn?.Invoke();
    }
    
    public void ShowCards(CardType _type)
    {
        cardsInHandHandler.ShowCards(this, _type);
    }

    public void HideCards()
    {
        cardsInHandHandler.HideCards();
    }

    public void DestroyCard(Card _cardBase)
    {
        if (_cardBase == null)
        {
            return;
        }

        _cardBase.Destroy();
        _cardBase.ReturnFromHand();
        _cardBase.SetHasDied(true);
        _cardBase.PositionAsDead();
    }

    public void DestroyWithoutNotify(CardBase _cardBase)
    {
        Card _card = ((Card)_cardBase);

        if (_card == null)
        {
            return;
        }

        _cardBase.ReturnFromHand();
        _card.SetHasDied(true);
    }

    public void ActivateAbility(string _abilityId)
    {
        AbilityCard _ability = FindObjectsOfType<AbilityCard>().ToList().Find(_ability => _ability.UniqueId == _abilityId);
        cardsInHandHandler.HideCards();

        if (!IsMy)
        {
            return;
        }

        GameplayManager.Instance.PlaceAbilityOnTable(_ability.UniqueId);

        if (_ability.Details.Type == AbilityCardType.Passive)
        {
            GameplayManager.Instance.ActivateAbility(_ability.UniqueId);
        }
    }
}