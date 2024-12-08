public class Famine : AbilityEffect
{
    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            return;
        }
        SetIsActive(true);
        SetRemainingCooldown(1);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += Deactivate;
        MoveToActivationField();
        RemoveAction();
        ManageActiveDisplay(true);
        OnActivated?.Invoke();
    }

    private void Deactivate()
    {
        if (RemainingCooldown>0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            return;
        }
        
        ManageActiveDisplay(false);
        
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= Deactivate;
        SetIsActive(false);
    }

    public override void CancelEffect()
    {
        SetRemainingCooldown(0);
        Deactivate();
    }
}
