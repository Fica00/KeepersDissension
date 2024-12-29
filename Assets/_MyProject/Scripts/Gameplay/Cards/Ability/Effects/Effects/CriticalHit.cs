public class CriticalHit : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        ManageActiveDisplay(true);
        Apply();
    }

    private void Apply()
    {
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        SetIsActive(true);
        SetStartingRange(_keeper.Range);
        SetStartingDamage(_keeper.Damage);
        _keeper.SetRange(1);
        _keeper.SetDamage(3);
        
        TablePlaceHandler _keeperTablePlace = _keeper.GetTablePlace();
        GameplayManager.Instance.SelectPlaceForSpecialAbility(_keeperTablePlace.Id,_keeper.Range,PlaceLookFor.Both, _keeper.MovementType,
            false,LookForCardOwner.Both,Attack,false);
    }
    
    private void Attack(int _placeId)
    {
        if (_placeId==-1)
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

        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        GameplayManager.Instance.ExecuteAttack(_keeper.UniqueId, _place.GetCard().UniqueId, Finish);
    }

    private void Finish()
    {
        Deactivate();
        OnActivated?.Invoke();
        RemoveAction();
    }

    private void Deactivate()
    {
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= Deactivate;
        SetIsActive(false);
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.SetRange(StartingRange);
        _keeper.SetDamage(StartingDamage);
        ManageActiveDisplay(false);
    }
}
