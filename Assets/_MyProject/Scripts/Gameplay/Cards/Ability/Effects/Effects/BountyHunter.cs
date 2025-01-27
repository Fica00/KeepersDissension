public class BountyHunter : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        ManageActiveDisplay(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}