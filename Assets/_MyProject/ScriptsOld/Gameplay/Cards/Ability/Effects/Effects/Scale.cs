
public class Scale : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        ManageActiveDisplay(true);
        _keeper.EffectsHolder.AddComponent<ScalerScale>();
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        ScalerScale _scale = _keeper.EffectsHolder.GetComponent<ScalerScale>();
        ManageActiveDisplay(false);
        SetIsActive(false);
        if (_scale==null)
        {
            return;
        }
        
        Destroy(_scale);
    }
}
