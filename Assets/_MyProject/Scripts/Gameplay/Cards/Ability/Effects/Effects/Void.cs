public class Void : AbilityEffect
{
     protected override void ActivateForOwner()
    {
        GameplayManager.Instance.SelectPlaceForSpecialAbility(PlaceId,20,PlaceLookFor.Empty,
        CardMovementType.EightDirections,false,LookForCardOwner.Both,PlaceVoid);
    }
     
    private void PlaceVoid(int _placeId)
    {
        if (_placeId==-1)
        {
            DialogsManager.Instance.ShowOkDialog("Looks like there is no empty space");
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }

        GameplayManager.Instance.TellOpponentSomething("Opponent used Void");
        Card _marker = GameplayManager.Instance.GetCardOfTypeNotPlaced(CardType.Marker,true);
        if (_marker==null)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have available marker");
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }

        GameplayManager.Instance.PlaceCard(_marker,_placeId);
        AddEffectedCard(_marker.UniqueId);
        _marker.SetIsVoid(true);
        GameplayManager.Instance.ChangeSprite(_marker.UniqueId,_marker.Details.Id,true);
        SetIsActive(true);
        OnActivated?.Invoke();
        RemoveAction();
    }

    public bool IsEffectedCard(string _uniqueId)
    {
        return EffectedCards[0] == _uniqueId;
    }

    protected override void CancelEffect()
    {
        GameplayPlayer _player = GameplayManager.Instance.MyPlayer;
        SetIsActive(false);
        var _effectedCardBase = GetEffectedCards()[0];
        _player.DestroyCard(_effectedCardBase);
        RemoveEffectedCard(_effectedCardBase.UniqueId);
    }
}