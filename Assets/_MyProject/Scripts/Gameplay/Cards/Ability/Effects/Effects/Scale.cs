using System.Linq;

public class Scale : AbilityEffect
{
    public override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        ManageActiveDisplay(true);
        _keeper.EffectsHolder.AddComponent<ScalerScale>();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        ScalerScale _scale = _keeper.EffectsHolder.GetComponent<ScalerScale>();
        if (_scale==null)
        {
            return;
        }
        
        Destroy(_scale);
        ManageActiveDisplay(false);
    }
}
