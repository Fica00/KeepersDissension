public class Strength : AbilityEffect
{
    private int amount = 1;
    
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.ChangeDamage(amount);
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        ManageActiveDisplay(true);
        OnActivated?.Invoke();
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        var _keeper = GetEffectedCards()[0];
        SetIsActive(false);
        _keeper.ChangeDamage(-amount);
        RemoveEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(false);
    }
}
