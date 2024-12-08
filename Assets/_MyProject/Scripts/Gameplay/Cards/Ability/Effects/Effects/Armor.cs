public class Armor : AbilityEffect
{
    public override void ActivateForOwner()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn += Activate;
        GameplayManager.Instance.OpponentPlayer.OnStartedTurn += Activate;
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void OnDisable()
    {
        if (!IsActive)
        {
            return;
        }

        GameplayManager.Instance.MyPlayer.OnStartedTurn -= Activate;
        GameplayManager.Instance.OpponentPlayer.OnStartedTurn -= Activate;
    }
    
    private void Activate()
    {
        SetIsActive(true);
        ManageActiveDisplay(true);
        SetCanExecuteThisTurn(true);
    }

    public void MarkAsUsed()
    {
        ManageActiveDisplay(false);
        SetCanExecuteThisTurn(false);
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        OnDisable();
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
