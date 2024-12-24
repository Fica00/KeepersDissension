public class Subdued : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        MoveToActivationField();
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