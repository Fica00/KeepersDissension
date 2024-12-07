using System.Linq;

public class Strength : AbilityEffect
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
        keeper.ChangeDamage(1);
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }

        keeper.ChangeDamage(-1);
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
