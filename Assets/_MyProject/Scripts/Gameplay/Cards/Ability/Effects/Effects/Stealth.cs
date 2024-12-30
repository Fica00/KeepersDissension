public class Stealth : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        SniperStealth _stealth = _keeper.EffectsHolder.AddComponent<SniperStealth>();
        _stealth.IsBaseCardsEffect = false;
        _stealth.Setup(true,null);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        SniperStealth _stealth = _keeper.EffectsHolder.GetComponent<SniperStealth>();
        ManageActiveDisplay(false);
        SetIsActive(false);
        
        if (_stealth==null)
        {
            return;
        }
        
        Destroy(_stealth);
    }
}
