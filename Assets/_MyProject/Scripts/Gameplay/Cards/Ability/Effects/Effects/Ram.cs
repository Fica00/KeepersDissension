using System.Linq;

public class Ram : AbilityEffect
{
    private Keeper keeper;
    
    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
    }

    private void Activate()
    {
        keeper.EffectsHolder.AddComponent<BlockaderRam>();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }

        BlockaderRam _blockader = keeper.EffectsHolder.GetComponent<BlockaderRam>();
        if (_blockader==null)
        {
            return;
        }
        
        Destroy(_blockader);
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
