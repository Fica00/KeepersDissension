public class Ram : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.EffectsHolder.AddComponent<BlockaderRam>();
        AddEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        BlockaderRam _blockader = _keeper.EffectsHolder.GetComponent<BlockaderRam>();
        SetIsActive(false);
        ManageActiveDisplay(false);
        if (_blockader==null)
        {
            return;
        }
        
        Destroy(_blockader);
    }
}