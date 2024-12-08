public class Minionized : AbilityEffect
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
        Activate();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    private void Activate()
    {
        SetHasMyRequiredCardDied(false);
        SetHasOpponentsRequiredCardDied(false);
        SetRemainingCooldown(2);
        SetIsActive(true);
        Card _myKeeper = GameplayManager.Instance.GetMyKeeper();
        Card _opponentKeeper = GameplayManager.Instance.GetOpponentKeeper();
        SetStartingHealth(_myKeeper.Health);
        SetOpponentsStartingHealth(_opponentKeeper.Health);
        
        _myKeeper.SetMaxHealth(1);
        _opponentKeeper.SetMaxHealth(1);
        _myKeeper.SetHealth(1);
        _opponentKeeper.SetHealth(1);

        GameplayManager.Instance.MyPlayer.OnEndedTurn += LowerCounter;
        GameplayManager.OnKeeperDied += CheckKeeper;
        RemoveAction();
    }

    private void LowerCounter()
    {
        if (RemainingCooldown>0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            return;
        }

        ManageActiveDisplay(false);
        SetIsActive(false);
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= LowerCounter;
        GameplayManager.OnKeeperDied -= CheckKeeper;
        Card _myKeeper = GameplayManager.Instance.GetMyKeeper();
        Card _opponentKeeper = GameplayManager.Instance.GetOpponentKeeper();
        _myKeeper.SetMaxHealth(-1);
        _opponentKeeper.SetMaxHealth(-1);
        _myKeeper.SetHealth(HasMyRequiredCardDied ? 5 : StartingHealth);
        _opponentKeeper.SetHealth(HasOpponentsRequiredCardDied ? 5 : OpponentsStartingHealth);
    }

    private void CheckKeeper(Keeper _keeper)
    {
        Card _myKeeper = GameplayManager.Instance.GetMyKeeper();
        Card _opponentKeeper = GameplayManager.Instance.GetOpponentKeeper();
        
        if (_keeper == _myKeeper)
        {
            SetHasMyRequiredCardDied(true);
        }
        else if (_keeper == _opponentKeeper)
        {
            SetHasOpponentsRequiredCardDied(true);
        }
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        SetRemainingCooldown(0);
        LowerCounter();
    }
}
