using System.Linq;
using UnityEngine;

public class Leapfrog : AbilityEffect
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
        Debug.Log(keeper.name,keeper.gameObject);
        keeper.EffectsHolder.AddComponent<ScalerLeapfrog>();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }

        Debug.Log("Canceling effect");
        ScalerLeapfrog _leapfrog = keeper.EffectsHolder.GetComponent<ScalerLeapfrog>();
        if (_leapfrog==null)
        {
            return;
        }
        
        Destroy(_leapfrog);
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
