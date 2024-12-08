using System.Linq;

public class Ram : AbilityEffect
{
    
    public override void ActivateForOwner()
    {
        var _keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        _keeper.EffectsHolder.AddComponent<BlockaderRam>();
        AddEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(true);
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
        BlockaderRam _blockader = _keeper.EffectsHolder.GetComponent<BlockaderRam>();
        if (_blockader==null)
        {
            return;
        }
        
        Destroy(_blockader);
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}