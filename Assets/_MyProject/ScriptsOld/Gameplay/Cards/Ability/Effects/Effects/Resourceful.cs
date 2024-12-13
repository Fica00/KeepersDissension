public class Resourceful : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        GameplayManager.Instance.StrangeMatterCostChange += 2;
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
        GameplayManager.Instance.StrangeMatterCostChange -= 2;
        ManageActiveDisplay(false);
    }

    protected override void CancelEffect()
    {
        RemoveEffect();
    }
}
