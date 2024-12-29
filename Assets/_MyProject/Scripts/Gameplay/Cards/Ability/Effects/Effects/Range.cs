public class Range : AbilityEffect
{
    private int changeAmount = 1;
    
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.ChangeRange(changeAmount);
        SetIsActive(true);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        var _keeper = GetEffectedCards()[0];
        _keeper.ChangeRange(-changeAmount);
        ManageActiveDisplay(false);
    }
}