public class Vulnerable : AbilityEffect
{
    private int change = 50;

    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetOpponentKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.ChangePercentageOfHealthToRecover(-change);
        SetIsActive(true);
        ManageActiveDisplay(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        SetIsActive(false);
        var _keeper = GetEffectedCards()[0];
        _keeper.ChangePercentageOfHealthToRecover(change);
        RemoveEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(false);
    }
}
