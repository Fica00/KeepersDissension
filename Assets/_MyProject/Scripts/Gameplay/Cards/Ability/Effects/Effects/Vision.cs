public class Vision : AbilityEffect
{
    public override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        ScoutVision _vision = _keeper.EffectsHolder.AddComponent<ScoutVision>();
        _vision.IsBaseCardsEffect = false;
        _vision.Setup(false,null);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    public override void CancelEffect()
    {
        ManageActiveDisplay(false);
        if (GetEffectedCards().Count==0)
        {
            return;
        }

        Card _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        ScoutVision _vision = _keeper.EffectsHolder.GetComponent<ScoutVision>();
        if (_vision==null)
        {
            return;
        }
        
        Destroy(_vision);
    }
}