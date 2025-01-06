public class Penalty : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        ManageActiveDisplay(true);
        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
        RoomUpdater.Instance.ForceUpdate();
    }
}