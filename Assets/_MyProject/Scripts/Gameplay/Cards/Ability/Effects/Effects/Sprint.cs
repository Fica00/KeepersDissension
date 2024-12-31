using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sprint : AbilityEffect
{
    List<TablePlaceHandler> availablePlaces = new();
    private int speedChange = 3;

    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        availablePlaces = new List<TablePlaceHandler>();
        
        List<Minion> _cards = GameplayManager.Instance.GetAllMinions().FindAll(_minion => _minion.My).ToList();
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

            availablePlaces.Add(_tablePlace);
        }

        if (availablePlaces.Count == 0)
        {
            DialogsManager.Instance.ShowOkDialog("You dont have minion on which this ability can be applied to");
            OnActivated?.Invoke();
            RemoveAction();
            return;
        }

        foreach (var _availablePlace in availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        DialogsManager.Instance.ShowOkDialog("Select which Warrior will get +3 speed for one action");
        TablePlaceHandler.OnPlaceClicked += CheckClickedPlace;
        SetIsActive(true);
    }
    
    private void CheckClickedPlace(TablePlaceHandler _clickedPlace)
    {
        TablePlaceHandler.OnPlaceClicked -= CheckClickedPlace;
        if (!availablePlaces.Contains(_clickedPlace))
        {
            return;
        }

        foreach (var _availablePlace in availablePlaces)
        {
            _availablePlace.SetColor(Color.white);
        }

        var _card = _clickedPlace.GetCardNoWall();
        _card.ChangeSpeed(speedChange);
        AddEffectedCard(_card.UniqueId);
        RemoveAction();
        GameplayManager.Instance.MyPlayer.OnUpdatedActions += RemoveEffect;
        OnActivated?.Invoke();
    }

    private void RemoveEffect()
    {
        GameplayManager.Instance.MyPlayer.OnUpdatedActions -= RemoveEffect;
        Card _effectedCard = GetEffectedCards()[0];
        _effectedCard.ChangeSpeed(-speedChange);
        RemoveEffectedCard(_effectedCard.UniqueId);
        SetIsActive(false);
        RoomUpdater.Instance.ForceUpdate();
    }
}