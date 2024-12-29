public class Restore : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        Card _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.HealFull();
        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }
}
