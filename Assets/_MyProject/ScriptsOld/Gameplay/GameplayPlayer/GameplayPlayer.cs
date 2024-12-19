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
    public Action UpdatedActions;
    public Action<CardData> OnAddedCard;
    public Action<AbilityData> OnAddedAbility;
    public Action<string> OnRemovedCard;
    public Action<string> OnRemovedAbility;
    
    [SerializeField] private TableSideHandler tableSideHandler;
    [SerializeField] private CardsInHandHandler cardsInHandHandler;
    [SerializeField] private Transform destroyedCardsHolder;

    private bool isMy;
    private int actions;
    private FactionSO factionSo;
    private List<AbilityCard> ownedAbilities = new();
    private List<Card> cardsInDeck = new();
    private List<AbilityCard> abilitiesInHand = new();

    public List<AbilityCard> OwnedAbilities => ownedAbilities;
    
    public TableSideHandler TableSideHandler => tableSideHandler;
    
    public bool IsMy => isMy;
    
    public FactionSO FactionSo => factionSo;

    public int Actions
    {
        get => actions;
        set
        {
            if (actions > value && IsMy && GameplayCheats.UnlimitedActions)
            {
                return;
            }

            actions = value;
            if (actions < 0)
            {
                actions = 0;
            }
            else if (actions > 5)
            {
                actions = 5;
            }

            UpdatedActions?.Invoke();
        }
    }

    public void Setup(int _factionId, bool _isMy)
    {
        isMy = _isMy;
        factionSo = FactionSO.Get(_factionId);
        tableSideHandler.Setup(this);
        SetupCardsInDeck();
        cardsInHandHandler.Setup(this);
    }

    private void SetupCardsInDeck()
    {
        Transform _cardsHolder = tableSideHandler.CardsHolder;
        int _lastIdOfCard = 0;
        Card _wallCard = default;
        foreach (var _cardInDeck in CardsManager.Instance.Get(FactionSo))
        {
            if (_cardInDeck == null)
            {
                continue;
            }

            Card _card = CardsManager.Instance.CreateCard(_cardInDeck.Details.Id, IsMy);
            if (_card is Wall)
            {
                _wallCard = _card;
            }

            if (_card.Details.Id > _lastIdOfCard)
            {
                _lastIdOfCard = _card.Details.Id;
            }

            _card.transform.SetParent(_cardsHolder);
            _card.SetParent(_cardsHolder);
            _card.Setup(IsMy ? FirebaseManager.Instance.PlayerId : FirebaseManager.Instance.OpponentId);
            AddNewCard(_card);
        }

        for (int _i = 0; _i < 30; _i++)
        {
            Card _card = CardsManager.Instance.CreateCard(_wallCard.Details.Id, IsMy);
            _card.Details.Id = 500 + _i;
            _card.transform.SetParent(_cardsHolder);
            _card.SetParent(_cardsHolder);
            _card.Setup(IsMy ? FirebaseManager.Instance.PlayerId : FirebaseManager.Instance.OpponentId);
            AddNewCard(_card);
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
    
    public void ShowCards(CardType _type)
    {
        cardsInHandHandler.ShowCards(this, _type);
    }

    public void HideCards()
    {
        cardsInHandHandler.HideCards();
    }

    public void DestroyCard(CardBase _cardBase)
    {
        Card _card = ((Card)_cardBase);

        if (_card == null)
        {
            return;
        }

        if (!ContainsCard(_card))
        {
            AddNewCard(_card);
        }

        _cardBase.Destroy();
        _cardBase.ReturnFromHand();
        _card.SetHasDied(true);
    }

    public void DestroyWithoutNotify(CardBase _cardBase)
    {
        Card _card = ((Card)_cardBase);

        if (_card == null)
        {
            return;
        }

        if (!ContainsCard(_card))
        {
            AddNewCard(_card);
        }

        _cardBase.ReturnFromHand();
        _card.SetHasDied(true);
    }

    public void AddOwnedAbility(string _abilityId)
    {
        AbilityCard _ability = FindObjectsOfType<AbilityCard>().ToList().Find(_ability => _ability.UniqueId == _abilityId);
        ownedAbilities.Add(_ability);
        cardsInHandHandler.HideCards();

        if (!IsMy)
        {
            return;
        }

        GameplayManager.Instance.PlaceAbilityOnTable(_ability.UniqueId);

        if (_ability.Details.Type == AbilityCardType.Passive)
        {
            StartCoroutine(ActivateAbility());
        }


        IEnumerator ActivateAbility()
        {
            yield return new WaitForSeconds(1);
            GameplayManager.Instance.ActivateAbility(_ability.UniqueId);
        }
    }
    
    public void AddNewCard(Card _card)
    {
        cardsInDeck.Add(_card);
        OnAddedCard?.Invoke(_card.Data);
    }
    
    public void AddNewCard(AbilityCard _card)
    {
        abilitiesInHand.Add(_card);
        OnAddedAbility?.Invoke(_card.Data);
    }

    public void RemoveCard(string _uniqueId)
    {
        Card _card = null;
        foreach (var _cardInDeck in cardsInDeck)
        {
            if (_cardInDeck.UniqueId != _uniqueId)
            {
                continue;
            }

            _card = _cardInDeck;
            break;
        }
        
        if (_card==null)
        {
            return;
        }
        cardsInDeck.Remove(_card);
        UpdatedDeck?.Invoke();
        OnRemovedCard?.Invoke(_card.UniqueId);
    }

    public void RemoveAbility(string _uniqueId)
    {
        AbilityCard _ability = null;
        foreach (var _abilityInDeck in abilitiesInHand)
        {
            if (_abilityInDeck.UniqueId != _uniqueId)
            {
                continue;
            }

            _ability = _abilityInDeck;
            break;
        }
        
        if (_ability == null)
        {
            return;    
        }
        abilitiesInHand.Remove(_ability);
        OnRemovedAbility?.Invoke(_ability.UniqueId);
    }
    
    public bool ContainsCard(Card _card)
    {
        return cardsInDeck.Find(_cardInDeck => _cardInDeck.UniqueId == _card.UniqueId);
    }
    
    public List<AbilityCard> GetAbilities()
    {
        return abilitiesInHand;
    }

    public AbilityCard GetAbility(string _uniqueId)
    {
        return abilitiesInHand.Find(_ability => _ability.UniqueId == _uniqueId);
    }
    
    public Card GetCardOfType(CardType _type)
    {
        return cardsInDeck.Find(_card => _card.Details.Type == _type);
    }
    
    public List<Card> GetAllCardsOfType(CardType _type)
    {
        return cardsInDeck.FindAll(_card => _card.Details.Type == _type);
    }
    
    public Card DrawCard(string _id)
    {
        return cardsInDeck.Find(_card => _card.UniqueId == _id);
    }
}