public class Vision : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        ScoutVision _vision = _keeper.EffectsHolder.AddComponent<ScoutVision>();
        _vision.IsBaseCardsEffect = false;
        _vision.Setup(true,null);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        ScoutVision _vision = _keeper.EffectsHolder.GetComponent<ScoutVision>();
        if (_vision==null)
        {
            return;
        }
        
        Destroy(_vision);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}