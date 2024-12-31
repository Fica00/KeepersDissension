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

    public void MarkAsUsed()
    {
        SetIsActive(false);
        ManageActiveDisplay(false);
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
