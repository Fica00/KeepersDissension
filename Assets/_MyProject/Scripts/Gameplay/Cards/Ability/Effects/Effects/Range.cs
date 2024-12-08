public class Range : AbilityEffect
{
    public override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.ChangeRange(1);
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        var _keeper = GetEffectedCards()[0];
        _keeper.ChangeRange(-1);
        ManageActiveDisplay(false);
    }
}