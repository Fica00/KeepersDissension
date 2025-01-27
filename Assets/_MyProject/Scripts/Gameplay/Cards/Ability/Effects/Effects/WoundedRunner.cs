public class WoundedRunner : AbilityEffect
{
    private int speedChange=2;
    
    protected override void ActivateForOwner()
    {
        Card _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.UpdatedHealth += CheckKeeper;
        CheckKeeper(_keeper);
        OnActivated?.Invoke();
        RemoveAction();
    }

    private void CheckKeeper()
    {
        Card _keeper = GetEffectedCards()[0];
        CheckKeeper(_keeper);
    }

    private void CheckKeeper(Card _keeper)
    {
        if (IsApplied&&_keeper.Health>1)
        {
            SetIsApplied(false);
            _keeper.ChangeSpeed(-speedChange);
            ManageActiveDisplay(false);
        }
        else if (!IsApplied&& _keeper.Health<=1)
        {
            SetIsApplied(true);
            _keeper.ChangeSpeed(speedChange);
            ManageActiveDisplay(true);
        }
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        _keeper.UpdatedHealth -= CheckKeeper;
        if (!IsApplied)
        {
            return;
        }
        
        SetIsApplied(false);
        _keeper.ChangeSpeed(-speedChange);
        ManageActiveDisplay(false);
    }
}