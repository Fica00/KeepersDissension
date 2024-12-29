public class Immunity : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        SetIsActive(true);
        OnActivated?.Invoke();
        GameplayManager.Instance.MyPlayer.OnEndedTurn += DisableEffect;
        ManageActiveDisplay(true);
        RemoveAction();
    }

    private void DisableEffect()
    {
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= DisableEffect;
        SetIsActive(false);
        ManageActiveDisplay(false);
        RoomUpdater.Instance.ForceUpdate();
    }

    protected override void CancelEffect()
    {
        DisableEffect();
    }
}
