public class Regeneration : AbilityEffect
{
    private int amountToHeal = 1;
    
    public override void ActivateForOwner()
    {
        if (Multiplayer==-1)
        {
            SetMultiplayer(1);
        }
        else
        {
            SetMultiplayer(Multiplayer+1);
        }
        
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            Activate();
            OnActivated?.Invoke();
            return;
        }
        
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
        if (!IsActive)
        {
            return;
        }
        
        Activate(Multiplayer);
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= ActivateAndUnsubscribe;
        SetMultiplayer(0);
        SetIsActive(false);
        ManageActiveDisplay(false);
    }

    private void Activate(int _amount=-1)
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.ChangeHealth(_amount == -1 ? amountToHeal : _amount);
    }

    public override void CancelEffect()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= ActivateAndUnsubscribe;
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
