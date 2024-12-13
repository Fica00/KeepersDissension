public class Invincible : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        SetRemainingCooldown(1);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += Deactivate;
        SetIsActive(true);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    private void Deactivate()
    {
        if (RemainingCooldown>0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            return;
        }

        GameplayManager.Instance.MyPlayer.OnEndedTurn -= Deactivate;
        ManageActiveDisplay(false);
        SetIsActive(false);
    }

    protected override void CancelEffect()
    {
        SetRemainingCooldown(0);
        Deactivate();
    }
}
