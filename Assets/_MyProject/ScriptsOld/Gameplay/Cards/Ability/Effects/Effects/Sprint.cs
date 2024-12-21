using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sprint : AbilityEffect
{
    private GameplayState state;

    protected override void ActivateForOwner()
    {
        state = GameplayManager.Instance.GameState();
        GameplayManager.Instance.SetGameState(GameplayState.SelectingCardFromTable);

        MoveToActivationField();
        List<Minion> _cards = GameplayManager.Instance.GetAllMinions().FindAll(_minion => _minion.My).ToList();
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
            DialogsManager.Instance.ShowOkDialog("You dont have minion on which this ability can be applied to");
            GameplayManager.Instance.SetGameState(state);
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }

        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        DialogsManager.Instance.ShowOkDialog("Select which Warrior will get +3 speed for one action");
        TablePlaceHandler.OnPlaceClicked += CheckClickedPlace;
        SetIsActive(true);

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

            var _card = _clickedPlace.GetCardNoWall();
            _card.ChangeSpeed(3);
            AddEffectedCard(_card.UniqueId);
            GameplayManager.Instance.SetGameState(state);
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
        Card _effectedCard = GetEffectedCards()[0];
        if(_arg1 != _effectedCard && _arg2 != _effectedCard)
        {
            return;
        }

        RemoveEffect();
    }

    private void RemoveEffect(CardBase _obj)
    {
        Card _effectedCard = GetEffectedCards()[0];
        if (_effectedCard != _obj)
        {
            return;
        }
        RemoveEffect();
    }

    private void RemoveEffect(CardBase _arg1, int _arg2, int _arg3, bool _)
    {
        Card _effectedCard = GetEffectedCards()[0];
        if (_effectedCard != _arg1)
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
        Card _effectedCard = GetEffectedCards()[0];
        RemoveEffectedCard(_effectedCard.UniqueId);
        SetIsActive(false);
    }

    protected override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        RemoveEffect();
        Card _effectedCard = GetEffectedCards()[0];
        _effectedCard.ChangeSpeed(-3);
    }
}