using System.Linq;

public class Toughen : AbilityEffect
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
        keeper.Details.Stats.Health = 7;
        keeper.Heal();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        keeper.Details.Stats.Health = 5;
        keeper.Heal();
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
