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
        _marker.SetIsVoid(true);
        GameplayManager.Instance.ChangeSprite(_marker.UniqueId,0,true);
        SetIsActive(true);
        OnActivated?.Invoke();
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        GameplayPlayer _player = GameplayManager.Instance.MyPlayer;
        SetIsActive(false);
        var _effectedCardBase = GetEffectedCards()[0];
        _player.DestroyCard(_effectedCardBase);
    }
}