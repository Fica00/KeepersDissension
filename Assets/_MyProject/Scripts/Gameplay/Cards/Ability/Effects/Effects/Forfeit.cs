using System.Collections.Generic;
using UnityEngine;

public class Forfeit : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        List<Card> _minions = GameplayManager.Instance.GetAllCardsOfType(CardType.Minion,true);
        List<TablePlaceHandler> _availablePlaces = new();
        foreach (var _minion in _minions)
        {
            TablePlaceHandler _tablePlace = _minion.GetTablePlace();
            if (_tablePlace == null)
            {
                continue;
            }

            _availablePlaces.Add(_tablePlace);
        }

        if (_availablePlaces.Count == 0)
        {
            DialogsManager.Instance.ShowOkDialog("You dont have minion to sacrifice");
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

            GameplayManager.Instance.DamageCardByAbility(_clickedPlace.GetCardNoWall().UniqueId, 10, Finish);
        }

        void Finish(bool _didKill)
        {
            GameplayManager.Instance.ChangeMyStrangeMatter(4);
            RemoveAction();
            OnActivated?.Invoke();
        }
    }
}
