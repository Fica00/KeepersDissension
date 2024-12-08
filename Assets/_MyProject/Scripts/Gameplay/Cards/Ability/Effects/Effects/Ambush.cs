public class Ambush : AbilityEffect
{
    public override void ActivateForOwner()
    {
        SetIsActive(true);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void CancelEffect()
    {
        SetIsActive(false);
    }
}
