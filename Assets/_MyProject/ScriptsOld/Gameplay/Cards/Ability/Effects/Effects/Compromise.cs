using System.Collections.Generic;
using UnityEngine;

public class Compromise : AbilityEffect
{
    [SerializeField] private int amount;

    protected override void ActivateForOwner()
    {
        List<Card> _cards = GameplayManager.Instance.GetAllCards();
        foreach (var _card in _cards)
        {
            _card?.ChangeHealth(amount);
        }
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}
