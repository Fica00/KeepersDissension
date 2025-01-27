public class Repel : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
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