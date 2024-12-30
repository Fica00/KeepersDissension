
public class Scale : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        ScalerScale _scale = _keeper.EffectsHolder.AddComponent<ScalerScale>();
        _scale.IsBaseCardsEffect = false;
        _scale.Setup(true,null);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        ScalerScale _scale = _keeper.EffectsHolder.GetComponent<ScalerScale>();
        if (_scale==null)
        {
            return;
        }
        
        Destroy(_scale);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
