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
        OnActivated?.Invoke();
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        var _keeper = GetEffectedCards()[0];
        _keeper.ChangePercentageOfHealthToRecover(change);
        RemoveEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(false);
    }
}
