
public class Scale : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.CardData.HasScaler = true;
        SetIsActive(true);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        _keeper.CardData.HasScaler = false;
        RemoveEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
