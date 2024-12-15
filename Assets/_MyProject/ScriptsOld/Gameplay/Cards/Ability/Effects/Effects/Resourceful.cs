public class Resourceful : AbilityEffect
{
    private int amount = 2;
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        GameplayManager.Instance.ChangeStrangeMatterCostChange(amount);
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
        GameplayManager.Instance.ChangeStrangeMatterCostChange(-amount);
        ManageActiveDisplay(false);
    }

    protected override void CancelEffect()
    {
        RemoveEffect();
    }
}
