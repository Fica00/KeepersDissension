using System.Linq;

public class Persevere : AbilityEffect
{
    private bool isApplied;
    private int attackChange=1;
    private Keeper effectedKeeper;
    
    public override void ActivateForOwner()
    {
        effectedKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        effectedKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
    }

    private void Activate()
    {
        effectedKeeper.Stats.UpdatedHealth += CheckKeeper;
        CheckKeeper();
    }

    private void CheckKeeper()
    {
        if (isApplied&&effectedKeeper.Stats.Health>2)
        {
            isApplied = false;
            effectedKeeper.Stats.Damage -= attackChange;
            AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        }
        else if (!isApplied&& effectedKeeper.Stats.Health<=2)
        {
            isApplied = true;
            effectedKeeper.Stats.Damage += attackChange;
            AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        }
    }

    public override void CancelEffect()
    {
        effectedKeeper.Stats.UpdatedHealth -= CheckKeeper;
        if (isApplied)
        {
            isApplied = false;
            effectedKeeper.Stats.Damage -= attackChange;
            AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        }
    }
}
