public class HighStakes : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        RemoveAction();
        MoveToActivationField();
        OnActivated?.Invoke();
        SetIsActive(true);
        GameplayManager.Instance.EndTurn();
        ManageActiveDisplay(true);
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
