public class Retaliate : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetRemainingCooldown(2);
        SetIsActive(true);
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        MoveToActivationField();
        OnActivated?.Invoke();
        GameplayManager.Instance.MyPlayer.OnEndedTurn += LowerCounter;
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        SetRemainingCooldown(0);
        LowerCounter();
    }
    
    private void LowerCounter()
    {
        if (RemainingCooldown>0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            return;
        }

        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        SetIsActive(false);
        ManageActiveDisplay(false);
        RoomUpdater.Instance.ForceUpdate();
    }
}
