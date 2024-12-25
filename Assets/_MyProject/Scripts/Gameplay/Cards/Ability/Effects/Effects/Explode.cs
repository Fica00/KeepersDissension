public class Explode : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        RemoveAction();
        ManageActiveDisplay(true);
        SetIsActive(true);
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
