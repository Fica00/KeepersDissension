public class Persevere : AbilityEffect
{
    private int attackChange=1;

    protected override void ActivateForOwner()
    {
        Card _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.UpdatedHealth += CheckKeeper;
        CheckKeeper(_keeper);
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void CheckKeeper()
    {
        Card _keeper = GetEffectedCards()[0];
        CheckKeeper(_keeper);
    }

    private void CheckKeeper(Card _keeper)
    {
        if (!GameplayManager.Instance.IsMyTurn())
        {
            return;
        }
        
        if (IsApplied&&_keeper.Health>2)
        {
            SetIsApplied(false);
            _keeper.ChangeDamage(-attackChange);
            ManageActiveDisplay(false);
        }
        else if (!IsApplied&& _keeper.Health<=2)
        {
            SetIsApplied(true);
            _keeper.ChangeDamage(attackChange);
            ManageActiveDisplay(true);
        }
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        _keeper.UpdatedHealth -= CheckKeeper;
        if (!IsApplied)
        {
            return;
        }
        
        SetIsApplied(false);
        _keeper.ChangeDamage(-attackChange);
        ManageActiveDisplay(false);
    }
}
