public class Truce : AbilityEffect
{
    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        
        MoveToActivationField();
        SetRemainingCooldown(3);
        SetIsActive(true);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += CheckForEnd;
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    private void CheckForEnd()
    {
        if (RemainingCooldown > 0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            return;
        }
        
        ManageActiveDisplay(false);
        SetIsActive(false);
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= CheckForEnd;
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        SetRemainingCooldown(0);
        CheckForEnd();
    }
}
