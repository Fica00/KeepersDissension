public class Resourceful : AbilityEffect
{
    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            return;
        }

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

    public override void CancelEffect()
    {
        RemoveEffect();
    }
}
