public class Toughen : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        ManageActiveDisplay(true);
        _keeper.SetMaxHealth(7);
        _keeper.HealFull();
        OnActivated?.Invoke();
        RemoveAction();
        RoomUpdater.Instance.ForceUpdate();
    }


    protected override void CancelEffect()
    {
        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        _keeper.SetMaxHealth(5);
        ManageActiveDisplay(false);
        SetIsActive(false);
        RoomUpdater.Instance.ForceUpdate();
    }
}