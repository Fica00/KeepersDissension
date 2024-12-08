public class Hinder : AbilityEffect
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
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
