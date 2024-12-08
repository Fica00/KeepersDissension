public class Tar : AbilityEffect
{
    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            RemoveAction();
            MoveToActivationField();
            OnActivated?.Invoke();
            return;
        }

        SetIsActive(true);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += TryEnd;
        MoveToActivationField();
        ManageActiveDisplay(true);
        RemoveAction();
        SetRemainingCooldown(1);
        OnActivated?.Invoke();
    }

    private void TryEnd()
    {
        if (RemainingCooldown>0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            return;
        }
        
        SetIsActive(false);
        ManageActiveDisplay(false);
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= TryEnd;
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        SetRemainingCooldown(0);
        TryEnd();
    }
}