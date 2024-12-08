public class Hunter : AbilityEffect
{
    public override void ActivateForOwner()
    {
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void CancelEffect()
    {
        SetIsActive(false);
    }
}
