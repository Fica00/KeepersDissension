public class Leapfrog : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        ScalerLeapfrog _delivery = _keeper.EffectsHolder.AddComponent<ScalerLeapfrog>();
        _delivery.IsBaseCardsEffect = false;
        _delivery.Setup(false,null);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        ScalerLeapfrog _leapfrog = _keeper.EffectsHolder.GetComponent<ScalerLeapfrog>();
        if (_leapfrog==null)
        {
            return;
        }
        
        Destroy(_leapfrog);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
