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