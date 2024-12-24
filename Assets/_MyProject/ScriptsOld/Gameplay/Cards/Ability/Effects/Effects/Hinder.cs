public class Hinder : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
