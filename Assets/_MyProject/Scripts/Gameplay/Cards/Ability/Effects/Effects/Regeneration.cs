public class Regeneration : AbilityEffect
{
    private int amountToHeal = 1;

    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        Activate();
        MoveToActivationField();
        GameplayManager.Instance.MyPlayer.OnStartedTurn += ActivateAndUnsubscribe;
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    private void ActivateAndUnsubscribe()
    {
        Activate();
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= ActivateAndUnsubscribe;
        SetIsActive(false);
        ManageActiveDisplay(false);
    }

    private void Activate()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.ChangeHealth(amountToHeal);
    }

    protected override void CancelEffect()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= ActivateAndUnsubscribe;
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
