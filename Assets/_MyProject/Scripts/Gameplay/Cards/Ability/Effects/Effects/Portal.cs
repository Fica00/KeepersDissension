using System;
using System.Linq;
using UnityEngine;

public class Portal : AbilityEffect
{
    private int portalId = 2000;

    protected override void ActivateForOwner()
    {
        DialogsManager.Instance.ShowOkDialog("Select place for the portal");
        GameplayManager.Instance.SelectPlaceForSpecialAbility(10, 10, PlaceLookFor.Empty, CardMovementType.EightDirections, false,
            LookForCardOwner.Both, OnPlaceSelected);
    }

    private void OnPlaceSelected(int _selectedPlace)
    {
        Card _portal = GameplayManager.Instance.CreateCard(portalId, GameplayManager.Instance.MyPlayer.TableSideHandler, Guid.NewGuid().ToString(),
            true, FirebaseManager.Instance.PlayerId);

        GameplayManager.Instance.PlaceCard(_portal, _selectedPlace);
        AddEffectedCard(_portal.UniqueId);
        
        if (GetEffectedCards().Count == 2)
        {
            SetIsActive(true);
            GameplayManager.OnCardMoved += CheckCardThatMoved;
            OnActivated?.Invoke();
            RemoveAction();
        }
        else
        {
            DialogsManager.Instance.ShowOkDialog("Select second place for the portal");
            GameplayManager.Instance.SelectPlaceForSpecialAbility(10, 10, PlaceLookFor.Empty, CardMovementType.EightDirections, false,
                LookForCardOwner.Both, OnPlaceSelected);
            portalId++;
        }
    }

    private void CheckCardThatMoved(CardBase _cardThatMoved, int _startingPlace, int _finishingPlace)
    {
        Card _enteredPortal = null;
        Card _exitPortal = null;

        for (int _i = 0; _i < GetEffectedCards().Count; _i++)
        {
            Card _currentPortal = GetEffectedCards()[_i];
            if (_currentPortal.GetTablePlace().Id == _finishingPlace)
            {
                _enteredPortal = _currentPortal;
                _exitPortal = _i == 0 ? GetEffectedCards()[1] : GetEffectedCards()[0];
            }
        }

        if (_enteredPortal == null)
        {
            Debug.Log("Didn't enter a portal");
            return;
        }

        int _exitIndex = GameplayManager.Instance.TableHandler.GetTeleportExitIndex(_startingPlace, _enteredPortal.GetTablePlace().Id,
            _exitPortal.GetTablePlace().Id);
        if (_exitIndex != -1 && _cardThatMoved.name.ToLower().Contains("blockader") &&
            GameplayManager.Instance.TableHandler.GetPlace(_exitIndex).IsOccupied)
        {
            Debug.Log("Blockader moved");
            //handle blockader
            int _exitPortalIndex = _exitPortal.GetTablePlace().Id;
            int _placeIdOfSecondCard = _cardThatMoved.GetTablePlace().Id;
            var _placeInFront = GameplayManager.Instance.TableHandler.CheckForPlaceInFront(_exitIndex, _exitPortalIndex);

            if (_placeInFront != null && !_placeInFront.IsOccupied)
            {
                Debug.Log("Trying to push card out of the way");
                GameplayManager.Instance.PushCard(_placeIdOfSecondCard, _exitPortalIndex, 100);
                RoomUpdater.Instance.ForceUpdate();
            }
            else
            {
                Debug.Log("Damaging card");
                GameplayManager.Instance.DamageCardByAbility(((Card)_cardThatMoved).UniqueId, 1, _ => RoomUpdater.Instance.ForceUpdate());
            }
            return;
        }

        if (_exitIndex == -1 || GameplayManager.Instance.TableHandler.GetPlace(_exitIndex).IsOccupied)
        {
            Debug.Log("Place on the other side is occupied, damaging my self");
            GameplayManager.Instance.DamageCardByAbility(((Card)_cardThatMoved).UniqueId, 1,_ => RoomUpdater.Instance.ForceUpdate());
        }
        else
        {
            Debug.Log("moving to the new place");
            GameplayManager.Instance.ExecuteMove(_startingPlace,_exitIndex, ((Card)_cardThatMoved).UniqueId,RoomUpdater.Instance.ForceUpdate);
        }
    }

    protected override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        SetIsActive(false);
        portalId = 2000;

        foreach (var _portal in GetEffectedCards().ToList())
        {
            RemoveEffectedCard(_portal.UniqueId);
            Destroy(_portal.gameObject);
        }
    }
}