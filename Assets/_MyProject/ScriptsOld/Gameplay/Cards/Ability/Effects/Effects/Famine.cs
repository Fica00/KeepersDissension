public class Famine : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetRemainingCooldown(2);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += Deactivate;
        MoveToActivationField();
        ManageActiveDisplay(true);
        SetIsActive(true);
        RemoveAction();
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

    protected override void CancelEffect()
    {
        SetRemainingCooldown(0);
        Deactivate();
    }
}
