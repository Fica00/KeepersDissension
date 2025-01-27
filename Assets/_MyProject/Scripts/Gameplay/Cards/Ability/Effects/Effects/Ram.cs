public class Ram : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        BlockaderRam _ram = _keeper.EffectsHolder.AddComponent<BlockaderRam>();
        _ram.IsBaseCardsEffect = false;
        _ram.Setup(false,null);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        BlockaderRam _ram = _keeper.EffectsHolder.GetComponent<BlockaderRam>();
        if (_ram==null)
        {
            return;
        }
        
        Destroy(_ram);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}