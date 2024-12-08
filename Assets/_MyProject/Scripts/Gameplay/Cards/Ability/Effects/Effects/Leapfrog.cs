public class Leapfrog : AbilityEffect
{
    public override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        _keeper.EffectsHolder.AddComponent<ScalerLeapfrog>();
        ManageActiveDisplay(true);
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

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
