public class Explode : AbilityEffect
{
    public override void ActivateForOwner()
    {
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    public override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
