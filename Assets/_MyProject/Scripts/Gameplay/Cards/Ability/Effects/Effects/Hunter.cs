public class Hunter : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
    }
}
