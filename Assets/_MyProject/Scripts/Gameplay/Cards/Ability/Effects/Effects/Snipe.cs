public class Snipe : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        MoveToActivationField();
        SetIsActive(true);
        SetStartingDamage(_keeper.Damage);
        SetStartingRange(_keeper.Range);
        _keeper.SetRange(3);
        _keeper.SetDamage(1);
        ManageActiveDisplay(true);
        ForceKeeperAttack(_keeper);
    }

    private void ForceKeeperAttack(Card _keeper)
    {
        TablePlaceHandler _keeperTablePlace = _keeper.GetTablePlace();
        GameplayManager.Instance.SelectPlaceForSpecialAbility(_keeperTablePlace.Id, 3, PlaceLookFor.Both, _keeper.MovementType, false,
            LookForCardOwner.Both, Attack, false);
    }

    private void Attack(int _placeId)
    {
        if (_placeId == -1)
        {
            DialogsManager.Instance.ShowOkDialog("There are no available spaces");
            Finish();
            return;
        }

        TablePlaceHandler _place = GameplayManager.Instance.TableHandler.GetPlace(_placeId);
        if (!_place.IsOccupied)
        {
            Finish();
            return;
        }

        Card _card = _place.GetCardNoWall();
        if (_card==null)
        {
            Finish();
            return;
        }
        
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        if (_place.ContainsWall && GameplayManager.Instance.TableHandler.DistanceBetweenPlaces(_keeper.GetTablePlace(), _place) > 1)
        {
            Finish();
            return;
        }

        
        GameplayManager.Instance.ExecuteAttack(_keeper.UniqueId,_card.UniqueId, Finish);
    }

    private void Finish()
    {
        SetIsActive(false);
        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        _keeper.SetRange(StartingRange);
        _keeper.SetDamage(StartingDamage);
        ManageActiveDisplay(false);
        RemoveAction();
        OnActivated?.Invoke();
    }
}