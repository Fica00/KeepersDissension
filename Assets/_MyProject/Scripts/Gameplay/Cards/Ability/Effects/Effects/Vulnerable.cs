using System.Linq;
using UnityEngine;

public class Vulnerable : AbilityEffect
{
    private Keeper keeper;
    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
    }

    private void Activate()
    {
        keeper.PercentageOfHealthToRecover -= 50;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        keeper.PercentageOfHealthToRecover += 50;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
