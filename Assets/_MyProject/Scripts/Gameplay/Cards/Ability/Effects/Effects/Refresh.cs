public class Refresh : AbilityEffect
{
    private int amountToHeal=3;

    protected override void ActivateForOwner()
    {
        Card _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.ChangeHealth(amountToHeal);
        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }
}
