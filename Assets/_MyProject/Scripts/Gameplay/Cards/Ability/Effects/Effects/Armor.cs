public class Armor : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        BlockaderCard _blockader = _keeper.EffectsHolder.AddComponent<BlockaderCard>();
        _blockader.Activate();
        ManageActiveDisplay(true);
        EffectedCards.Add(_keeper.UniqueId);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        Card _card = GameplayManager.Instance.GetCard(EffectedCards[0]);
        var _effect = _card.EffectsHolder.GetComponent<BlockaderCard>();
        if (_effect)
        {
            Destroy(_effect);
        }
        
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
