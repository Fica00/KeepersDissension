using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Compromise : AbilityEffect
{
    [SerializeField] private int amount;
    
    public override void ActivateForOwner()
    {
        RemoveAction();
        List<CardBase> _cards = FindObjectsOfType<CardBase>().ToList();
        foreach (var _card in _cards)
        {
            (_card as Card)?.ChangeHealth(amount);
        }
        MoveToActivationField();
        OnActivated?.Invoke();
    }
}
