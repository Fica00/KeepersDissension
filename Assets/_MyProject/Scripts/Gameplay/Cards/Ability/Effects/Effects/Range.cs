using System.Linq;

public class Range : AbilityEffect
{
    private Keeper keeper;
    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }
    
    public override void ActivateForOther()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void Activate()
    {
        keeper.Stats.Range++;
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        
        keeper.Stats.Range--;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}