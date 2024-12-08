public class HighStakes : AbilityEffect
{
    public override void ActivateForOwner()
    {
        RemoveAction();
        MoveToActivationField();
        OnActivated?.Invoke();
        
        if (IsActive)
        {
            return;
        }
        
        RemoveAction();
        MoveToActivationField();
        OnActivated?.Invoke();
        SetIsActive(true);
        GameplayManager.Instance.EndTurn();
        ManageActiveDisplay(true);
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
