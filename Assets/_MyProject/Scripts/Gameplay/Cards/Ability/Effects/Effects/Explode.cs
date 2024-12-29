public class Explode : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        var _bomberCard = _keeper.EffectsHolder.AddComponent<BomberCard>();
        _bomberCard.IsBaseCardsEffect = false;
        _bomberCard.Setup(false,null);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        BomberCard _bomberCard = _keeper.EffectsHolder.GetComponent<BomberCard>();
        if (_bomberCard==null)
        {
            return;
        }
        
        Destroy(_bomberCard);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
