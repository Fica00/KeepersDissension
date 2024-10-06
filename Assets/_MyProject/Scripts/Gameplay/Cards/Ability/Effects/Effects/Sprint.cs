using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sprint : AbilityEffect
{
    private Card card;
    private GameplayState state;
    public static bool IsActive;

    private void Awake()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.SelectingCardFromTable;

        MoveToActivationField();
        List<Card> _cards = FindObjectsOfType<Card>().ToList().FindAll(_minion => _minion.My);
        List<TablePlaceHandler> _availablePlaces = new();
        foreach (var _card in _cards)
        {
            if (!_card.IsWarrior())
            {
                continue;
            }
            TablePlaceHandler _tablePlace = _card.GetTablePlace();
            if (_tablePlace == null)
            {
                continue;
            }

            _availablePlaces.Add(_tablePlace);
        }

        if (_availablePlaces.Count == 0)
        {
            UIManager.Instance.ShowOkDialog("You dont have minion on which this ability can be applied to");
            GameplayManager.Instance.GameState = state;
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }

        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        UIManager.Instance.ShowOkDialog("Select which Warrior will get +3 speed for one action");
        TablePlaceHandler.OnPlaceClicked += CheckClickedPlace;
        IsActive = true;

        void CheckClickedPlace(TablePlaceHandler _clickedPlace)
        {
            TablePlaceHandler.OnPlaceClicked -= CheckClickedPlace;
            if (!_availablePlaces.Contains(_clickedPlace))
            {
                return;
            }

            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.white);
            }

            card = _clickedPlace.GetCardNoWall();
            card.Speed += 3;
            GameplayManager.Instance.GameState = state;
            GameplayManager.OnCardMoved += RemoveEffect;
            GameplayManager.OnCardAttacked += RemoveEffect;
            GameplayManager.OnPlacedCard += RemoveEffect;
            GameplayManager.Instance.MyPlayer.OnEndedTurn += RemoveEffect;
            
            RemoveAction();
            OnActivated?.Invoke();
        }
    }

    private void RemoveEffect(CardBase _arg1, CardBase _arg2, int _arg3)
    {
        if(_arg1 != card && _arg2 != card)
        {
            return;
        }

        RemoveEffect();
    }

    private void RemoveEffect(CardBase _obj)
    {
        if (card != _obj)
        {
            return;
        }
        RemoveEffect();
    }

    private void RemoveEffect(CardBase _arg1, int _arg2, int _arg3, bool _)
    {
        if (card != _arg1)
        {
            return;
        }
        RemoveEffect();
    }

    private void RemoveEffect()
    {
        GameplayManager.OnCardMoved -= RemoveEffect;
        GameplayManager.OnCardAttacked -= RemoveEffect;
        GameplayManager.OnPlacedCard -= RemoveEffect;
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= RemoveEffect;
        IsActive = false;
    }

    public override void CancelEffect()
    {
        if (card==null)
        {
            return;
        }
        
        RemoveEffect();
        card.Speed -= 3;
    }
}