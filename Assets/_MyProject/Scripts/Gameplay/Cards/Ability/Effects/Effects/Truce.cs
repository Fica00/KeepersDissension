public class Truce : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        SetRemainingCooldown(3);
        SetIsActive(true);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += CheckForEnd;
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
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

    protected override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        SetRemainingCooldown(0);
        CheckForEnd();
    }
}
