public class Range : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.ChangeRange(1);
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        var _keeper = GetEffectedCards()[0];
        _keeper.ChangeRange(-1);
        ManageActiveDisplay(false);
    }
}