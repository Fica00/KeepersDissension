using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Compromise : AbilityEffect
{
    [SerializeField] private int amount;
    
    public override void ActivateForOwner()
    {
        RemoveAction();
        Activate();
        MoveToActivationField();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        Activate();
    }

    private void Activate()
    {
        List<CardBase> _cards = FindObjectsOfType<CardBase>().ToList();
        foreach (var _card in _cards)
        {
            _card.Heal(amount);
        }
    }
}
