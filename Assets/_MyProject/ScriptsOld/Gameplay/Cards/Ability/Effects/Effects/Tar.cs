public class Tar : AbilityEffect
{
    protected override void ActivateForOwner()
    {
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

    protected override void CancelEffect()
    {
        SetRemainingCooldown(0);
        TryEnd();
    }
}