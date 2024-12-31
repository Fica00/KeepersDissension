public class Subdued : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        MoveToActivationField();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}