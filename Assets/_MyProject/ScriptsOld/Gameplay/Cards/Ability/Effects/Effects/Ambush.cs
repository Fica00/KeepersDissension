public class Ambush : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        MoveToActivationField();
        ManageActiveDisplay(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
