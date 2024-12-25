public class Leapfrog : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.EffectsHolder.AddComponent<ScalerLeapfrog>();
        ManageActiveDisplay(true);
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        ManageActiveDisplay(false);
        var _keeper = GetEffectedCards()[0];
        ScalerLeapfrog _leapfrog = _keeper.EffectsHolder.GetComponent<ScalerLeapfrog>();
        if (_leapfrog==null)
        {
            return;
        }
        
        Destroy(_leapfrog);
    }
}
