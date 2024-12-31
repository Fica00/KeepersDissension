public class Dash : AbilityEffect
{
    private int speedChange = 1; 
    
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        Card _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.ChangeSpeed(speedChange);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        Card _keeper = GameplayManager.Instance.GetCard(EffectedCards[0]);
        _keeper.ChangeSpeed(speedChange);
        SetIsActive(false);
    }
}
