using System.Linq;

public class Refresh : AbilityEffect
{
    private int amountToHeal=3;
    private Keeper keeper;
    
    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        MoveToActivationField();
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
        keeper.Heal(amountToHeal);
    }
}
