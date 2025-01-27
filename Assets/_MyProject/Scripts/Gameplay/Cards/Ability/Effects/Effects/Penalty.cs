public class Penalty : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn += EndEffect;
        SetIsActive(true);
        ManageActiveDisplay(true);
        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }

    private void EndEffect()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= EndEffect;
        CancelEffect();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        SetIsActive(false);
        RoomUpdater.Instance.ForceUpdate();
    }
}