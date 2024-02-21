using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameplayPlayer : MonoBehaviour
{
    public Action OnStartedTurn;
    public Action OnEndedTurn;
    public Action UpdatedDeck;
    public Action<AbilityCard> UpdatedOwnedAbilities;
    public bool IsMy { get; private set; }
    public FactionSO FactionSO { get; private set; }
    public Action UpdatedActions;
    public Action UpdatedStrangeMatter;

    [SerializeField] private TableSideHandler tableSideHandler;
    [SerializeField] private CardsInHandHandler cardsInHandHandler;
    [SerializeField] private Transform destroyedCardsHolder;

    private List<AbilityCard> ownedAbilities = new List<AbilityCard>();
    public List<AbilityCard> OwnedAbilities => ownedAbilities;

    private Deck deck = new();
    private int lifeForce;
    private int actions;
    private int strangeMatter;

    public int AmountOfAbilitiesPlayerCanBuy;
    public TableSideHandler TableSideHandler => tableSideHandler;

    public int Actions
    {
        get => actions;
        set
        {
            if (actions>value && IsMy && GameplayCheats.UnlimitedActions)
            {
                return;
            }
            actions = value;
            if (actions<0)
            {
                actions = 0;
            }
            else if (actions>5)
            {
                actions = 5;
            }
            UpdatedActions?.Invoke();
        }
    }

    public int StrangeMatter
    {
        get => strangeMatter;
        set
        {
            if (value<strangeMatter&& GameplayCheats.HasUnlimitedGold)
            {
                return;
            }
            strangeMatter = value;
            UpdatedStrangeMatter?.Invoke();
        }
    }


    public void Setup(int _factionId, bool _isMy)
    {
        IsMy = _isMy;
        FactionSO = FactionSO.Get(_factionId);
        tableSideHandler.Setup(this);
        SetupCardsInDeck();
        cardsInHandHandler.Setup(this);
        AmountOfAbilitiesPlayerCanBuy = GameplayManager.Instance.AmountOfAbilitiesPlayerCanBuy;
    }

    private void SetupCardsInDeck()
    {
        Transform _cardsHolder = tableSideHandler.CardsHolder;
        foreach (var _cardInDeck in CardsManager.Instance.Get(FactionSO))
        {
            Card _card = CardsManager.Instance.CreateCard(_cardInDeck.Details.Id, IsMy);
            _card.transform.SetParent(_cardsHolder);
            _card.SetParent(_cardsHolder);
            _card.Setup(IsMy);
            AddCardToDeck(_card);
        }
    }

    public void NewTurn()
    {
        Actions = GameplayManager.Instance.AmountOfActionsPerTurn;
        OnStartedTurn?.Invoke();
    }

    public void EndedTurn()
    {
        Actions = 0;
        OnEndedTurn?.Invoke();
    }

    public void AddCardToDeck(AbilityCard _ability)
    {
        deck.AddNewCard(_ability);
    }

    public void AddCardToDeck(Card _card)
    {
        deck.AddNewCard(_card);
    }

    public Card GetCard(int _cardId)
    {
        return deck.DrawCard(_cardId);
    }

    public Card GetCard(CardType _type)
    {
        return deck.DrawCard(_type);
    }

    public void RemoveCardFromDeck(int _cardId)
    {
        Card _drawnCard = deck.DrawCard(_cardId);
        deck.Cards.Remove(_drawnCard);
        UpdatedDeck?.Invoke();
    }

    public void TryRemoveCardFromDeck(Card _card)
    {
        if (deck.Cards.Contains(_card))
        {
            deck.Cards.Remove(_card);
            UpdatedDeck?.Invoke();
        }
    }
    
    public void RemoveAbilityFromDeck(int _cardId)
    {
        AbilityCard _drawnCard = deck.DrawAbility(_cardId);
        deck.Abilities.Remove(_drawnCard);
    }

    public Card PeakNextCard(CardType _cardType)
    {
        return deck.DrawCard(_cardType);
    }

    public void ShowCards(CardType _type)
    {
        cardsInHandHandler.ShowCards(this,_type);
    }

    public void HideCards()
    {
        cardsInHandHandler.HideCards();
    }

    public List<Card> GetCardsInDeck(CardType _type)
    {
       return deck.Cards.FindAll(_card => _card.Details.Type == _type);
    }
    
    public List<AbilityCard> GetAbilities()
    {
        return deck.Abilities;
    }

    public void DestroyCard(CardBase _cardBase)
    {
        Card _card = ((Card)_cardBase);

        if (_card==null)
        {
            return;
        }
        
        if (!deck.Cards.Contains(_card))
        {
            deck.Cards.Add(_card);
        }
        
        _cardBase.Destroy();
        _cardBase.ReturnFromHand();
        _card.HasDied = true;
    }

    public void DestroyWithoutNotify(CardBase _cardBase)
    {
        Card _card = ((Card)_cardBase);

        if (_card==null)
        {
            return;
        }
        
        if (!deck.Cards.Contains(_card))
        {
            deck.Cards.Add(_card);
        }
        
        _cardBase.ReturnFromHand();
        _card.HasDied = true;
    }

    public void SetActionsWithoutNotify(int _amount)
    {
        actions = _amount;
    }

    public void RemoveStrangeMatter(int _amount)
    {
        if (!IsMy)
        {
            return;
        }

        if (GameplayCheats.HasUnlimitedGold)
        {
            return;
        }
        
        StrangeMatter -= _amount;
        GameplayManager.Instance.WhiteStrangeMatter.AmountInEconomy += _amount;
    }

    public void AddOwnedAbility(int _abilityId)
    {
        AbilityCard _ability = FindObjectsOfType<AbilityCard>().ToList()
            .Find(_ability => _ability.Details.Id == _abilityId);
        
        ownedAbilities.Add(_ability);
        cardsInHandHandler.HideCards();
        UpdatedOwnedAbilities?.Invoke(_ability);

        if (!IsMy)
        {
            return;
        }
        
        GameplayManager.Instance.PlaceAbilityOnTable(_ability.Details.Id);

        if (_ability.Details.Type == AbilityCardType.Passive)
        {
            StartCoroutine(ActivateAbility());
        }


        IEnumerator ActivateAbility()
        {
            yield return new WaitForSeconds(1);
            GameplayManager.Instance.ActivateAbility(_ability.Details.Id);
        }
    }

    public List<AbilityCard> GetOwnedAbilities()
    {
        return ownedAbilities;
    }
}
