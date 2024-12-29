public class Explode : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        ManageActiveDisplay(true);
        OnActivated?.Invoke();
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}