public class Invincible : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetRemainingCooldown(1);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += Deactivate;
        SetIsActive(true);
        MoveToActivationField();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
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
