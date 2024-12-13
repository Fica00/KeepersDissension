using System.Collections.Generic;
using UnityEngine;

public class Mirage : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        GameplayManager.Instance.GameState = GameplayState.SelectingCardFromTable;

        MoveToActivationField();
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        
        List<Minion> _minions = GameplayManager.Instance.GetAllMinions().FindAll(_minion => _minion.My);
        List<TablePlaceHandler> _availablePlaces = new();
        
        foreach (var _minion in _minions)
        {
            TablePlaceHandler _tablePlace = _minion.GetTablePlace();
            if (_tablePlace==null)
            {
                continue;
            }
            _availablePlaces.Add(_tablePlace);
        }
        
        if (_availablePlaces.Count==0)
        {
            DialogsManager.Instance.ShowOkDialog("You dont have minion to switch with");
            GameplayManager.Instance.GameState = GameplayState.Playing;
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        
        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        DialogsManager.Instance.ShowOkDialog("Please select minion to switch keepers position with");
        TablePlaceHandler.OnPlaceClicked += CheckClickedPlace;
        
        void CheckClickedPlace(TablePlaceHandler _clickedPlace)
        {
            if (!_availablePlaces.Contains(_clickedPlace))
            {
                return;
            }
            
            TablePlaceHandler.OnPlaceClicked -= CheckClickedPlace;

            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.white);
            }

            CardAction _switchPosition = new CardAction
            {
                StartingPlaceId = _keeper.GetTablePlace().Id,
                FirstCardId = _keeper.Details.Id,
                FinishingPlaceId = _clickedPlace.Id,
                SecondCardId = _clickedPlace.GetCardNoWall().Details.Id,
                Type = CardActionType.SwitchPlace,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = 10,
                CanCounter = false
            };
            GameplayManager.Instance.ExecuteCardAction(_switchPosition);
            GameplayManager.Instance.GameState = GameplayState.Playing;
            RemoveAction();
            OnActivated?.Invoke();
        }
    }
}
