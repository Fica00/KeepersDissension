public class Strafe : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.ChangeMovementType(CardMovementType.EightDirections);
        AddEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(true);
        OnActivated?.Invoke();
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        _keeper.ChangeMovementType(CardMovementType.FourDirections);
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}