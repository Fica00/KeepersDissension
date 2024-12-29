public class Armor : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        BlockaderCard _blockader = _keeper.EffectsHolder.AddComponent<BlockaderCard>();
        _blockader.Activate();
        EffectedCards.Add(_keeper.UniqueId);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        var _effect = _keeper.EffectsHolder.GetComponent<BlockaderCard>();
        if (_effect)
        {
            Destroy(_effect);
        }
        
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
