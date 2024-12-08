public class Refresh : AbilityEffect
{
    private int amountToHeal=3;
    
    public override void ActivateForOwner()
    {
        Card keeper = GameplayManager.Instance.GetMyKeeper();
        keeper.ChangeHealth(amountToHeal);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}
