using System.Collections.Generic;
using UnityEngine;

public class Forfeit : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        GameplayManager.Instance.GameState = GameplayState.SelectingCardFromTable;
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
            DialogsManager.Instance.ShowOkDialog("You dont have minion to sacrifice");
            GameplayManager.Instance.GameState = GameplayState.Playing;
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        
        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        DialogsManager.Instance.ShowOkDialog("Please select minion to be sacrificed");
        TablePlaceHandler.OnPlaceClicked += CheckClickedPlace;


        void CheckClickedPlace(TablePlaceHandler _clickedPlace)
        {
            if (!_availablePlaces.Contains(_clickedPlace))
            {
                return;
            }
            
            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.white);
            }
            
            TablePlaceHandler.OnPlaceClicked -= CheckClickedPlace;

            CardAction _attackAction = new CardAction
            {
                StartingPlaceId = _clickedPlace.Id,
                FirstCardId = _clickedPlace.GetCardNoWall().UniqueId,
                FinishingPlaceId = _clickedPlace.Id,
                SecondCardId = _clickedPlace.GetCardNoWall().UniqueId,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = false,
                Damage = 10,
                CanCounter = false
            };
            GameplayManager.Instance.ExecuteCardAction(_attackAction);
            GameplayManager.Instance.MyPlayer.StrangeMatter += 2;
            GameplayManager.Instance.GameState = GameplayState.Playing;
            RemoveAction();
            OnActivated?.Invoke();
        }
    }
}
