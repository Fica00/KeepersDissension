public class Vulnerable : AbilityEffect
{
    private int change = 50;
    
    public override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetOpponentKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.ChangePercentageOfHealthToRecover(-change);
        SetIsActive(true);
        ManageActiveDisplay(true);
        RemoveAction();
        OnActivated?.Invoke();
    }
    public override void CancelEffect()
    {
        if (IsActive)
        {
            return;
        }
        
        SetIsActive(false);
        var _keeper = GetEffectedCards()[0];
        _keeper.ChangePercentageOfHealthToRecover(change);
        ManageActiveDisplay(false);
    }
}
