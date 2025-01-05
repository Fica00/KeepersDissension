using System;

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
        ;
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
            return;
        }

        int _exitIndex = GameplayManager.Instance.TableHandler.GetTeleportExitIndex(_startingPlace, _enteredPortal.GetTablePlace().Id,
            _exitPortal.GetTablePlace().Id);
        if (_exitIndex != -1 && _cardThatMoved.name.ToLower().Contains("blockader") &&
            GameplayManager.Instance.TableHandler.GetPlace(_exitIndex).IsOccupied)
        {
            //handle blockader
            int _exitPortalIndex = _exitPortal.GetTablePlace().Id;
            int _placeIdOfSecondCard = _cardThatMoved.GetTablePlace().Id;
            var _placeInFront = GameplayManager.Instance.TableHandler.CheckForPlaceInFront(_exitIndex, _exitPortalIndex);

            if (_placeInFront == null)
            {
                GameplayManager.Instance.PushCardBack(_placeIdOfSecondCard, _exitPortalIndex, 100);
            }
            else
            {
                GameplayManager.Instance.DamageCardByAbility(((Card)_cardThatMoved).UniqueId, 1, null);
            }
            return;
        }

        if (_exitIndex == -1 || GameplayManager.Instance.TableHandler.GetPlace(_exitIndex).IsOccupied)
        {
            GameplayManager.Instance.DamageCardByAbility(((Card)_cardThatMoved).UniqueId, 1,null);
        }
        else
        {
            GameplayManager.Instance.ExecuteMove(_startingPlace,_exitIndex, ((Card)_cardThatMoved).UniqueId,null);
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

        foreach (var _portal in GetEffectedCards())
        {
            RemoveEffectedCard(_portal.UniqueId);
            Destroy(_portal.gameObject);
        }
    }
}