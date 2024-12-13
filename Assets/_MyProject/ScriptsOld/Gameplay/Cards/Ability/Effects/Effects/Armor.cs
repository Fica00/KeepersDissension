public class Armor : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        GameplayManager.Instance.MyPlayer.OnStartedTurn += Apply;
        GameplayManager.Instance.MyPlayer.OnEndedTurn += Apply;
        Apply();
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void Apply()
    {
        ManageActiveDisplay(true);
        SetCanExecuteThisTurn(true);
    }

    public void MarkAsUsed()
    {
        ManageActiveDisplay(false);
        SetCanExecuteThisTurn(false);
    }

    protected override void CancelEffect()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= Apply;
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= Apply;
        SetIsActive(false);
        SetCanExecuteThisTurn(false);
        ManageActiveDisplay(false);
    }
}
