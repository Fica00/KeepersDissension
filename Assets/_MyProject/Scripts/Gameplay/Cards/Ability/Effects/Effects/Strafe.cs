public class Strafe : AbilityEffect
{
    public override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.ChangeMovementType(CardMovementType.EightDirections);
        AddEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(true);
        RemoveAction();
        OnActivated?.Invoke();
    }
    
    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        RemoveEffectedCard(_keeper.UniqueId);
        SetIsActive(false);
        _keeper.ChangeMovementType(CardMovementType.FourDirections);
        ManageActiveDisplay(false);
    }
}
