public class Toughen : AbilityEffect
{
    public override void ActivateForOwner()
    {
        var keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(keeper.UniqueId);
        SetIsActive(true);
        ManageActiveDisplay(true);
        keeper.Details.Stats.Health = 7;
        keeper.HealFull();
        RemoveAction();
        OnActivated?.Invoke();
    }


    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        var keeper = GetEffectedCards()[0];
        RemoveEffectedCard(keeper.UniqueId);
        keeper.Details.Stats.Health = 5;
        keeper.HealFull();
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
