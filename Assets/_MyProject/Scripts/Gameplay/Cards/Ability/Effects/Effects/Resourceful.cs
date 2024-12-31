public class Resourceful : AbilityEffect
{
    private int change = 2;
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        GameplayManager.Instance.ChangeStrangeMatterCostChange(change,true);
        GameplayManager.Instance.MyPlayer.OnStartedTurn += RemoveEffect;
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    private void RemoveEffect()
    {
        SetIsActive(false);
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= RemoveEffect;
        GameplayManager.Instance.ChangeStrangeMatterCostChange(-change, true);
        ManageActiveDisplay(false);
        RoomUpdater.Instance.ForceUpdate();
    }

    protected override void CancelEffect()
    {
        RemoveEffect();
    }
}
