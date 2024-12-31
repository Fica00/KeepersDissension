public class HighStakes : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        SetIsActive(true);
        ManageActiveDisplay(true);
        OnActivated?.Invoke();
        GameplayManager.Instance.EndTurn();
        RoomUpdater.Instance.ForceUpdate();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
