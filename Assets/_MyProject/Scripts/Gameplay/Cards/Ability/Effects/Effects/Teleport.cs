public class Teleport : AbilityEffect
{
    private int range;
    protected override void ActivateForOwner()
    {
        range = 1;
        MoveToActivationField();
        GetPlace();
    }

    private void GetPlace()
    {
        LifeForce _lifeForce = GameplayManager.Instance.GetMyLifeForce();
        GameplayManager.Instance.SelectPlaceForSpecialAbility(_lifeForce.GetTablePlace().Id, range, PlaceLookFor.Empty,
            CardMovementType.EightDirections, false, LookForCardOwner.Both, PlaceKeeper);
    }

    private void PlaceKeeper(int _placeId)
    {
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        if (_placeId == -1)
        {
            range++;
            if (range > 10)
            {
                GameplayManager.Instance.DamageCardByAbility(_keeper.UniqueId, _keeper.Damage, Finish);
                return;
            }

            GetPlace();
        }
        else
        {
            DoTeleport(_placeId);
        }
    }

    private void DoTeleport(int _placeId)
    {
        if (_placeId == -1)
        {
            DialogsManager.Instance.ShowOkDialog("There are no empty spaces around Life Force");
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }

        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        GameplayManager.Instance.ExecuteMove(_keeper.GetTablePlace().Id, _placeId, _keeper.UniqueId, Finish);
    }
    
    private void Finish(bool _)
    {
        Finish();
    }
    
    private void Finish()
    {
        OnActivated?.Invoke();
        RemoveAction();
    }
}