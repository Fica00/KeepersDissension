using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GameplayPlayer : MonoBehaviour
{
    public Action OnStartedTurn;
    public Action OnEndedTurn;
    public Action UpdatedDeck;
    public Action UpdatedActions;
    
    [SerializeField] private TableSideHandler tableSideHandler;
    [SerializeField] private CardsInHandHandler cardsInHandHandler;
    [SerializeField] private Transform destroyedCardsHolder;

    public TableSideHandler TableSideHandler => tableSideHandler;

    private bool isMy;
    private int actions;
    private FactionSO factionSo;
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
        cardsInHandHandler.Setup(this);
        if (_isMy)
        {
            SetupCardsInDeck();
        }
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
            GameplayManager.Instance.AddCard(_card.Data,isMy);
        }

        for (int _i = 0; _i < 30; _i++)
        {
            Card _card = CardsManager.Instance.CreateCard(_wallCard.Details.Id, IsMy);
            _card.Details.Id = 500 + _i;
            _card.transform.SetParent(_cardsHolder);
            _card.SetParent(_cardsHolder);
            _card.Setup(IsMy ? FirebaseManager.Instance.PlayerId : FirebaseManager.Instance.OpponentId);
            GameplayManager.Instance.AddCard(_card.Data,isMy);
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

        if (!GameplayManager.Instance.ContainsCard(_card.Data,isMy))
        {
            GameplayManager.Instance.AddCard(_card.Data,isMy);
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

        if (!GameplayManager.Instance.ContainsCard(_card.Data,isMy))
        {
            GameplayManager.Instance.AddCard(_card.Data,isMy);
        }

        _cardBase.ReturnFromHand();
        _card.SetHasDied(true);
    }

    public void AddOwnedAbility(string _abilityId)
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
            StartCoroutine(ActivateAbility());
        }


        IEnumerator ActivateAbility()
        {
            yield return new WaitForSeconds(1);
            GameplayManager.Instance.ActivateAbility(_ability.UniqueId);
        }
    }
}