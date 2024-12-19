using System;
using UnityEngine;

public class Depelete : AbilityEffect
{
    [SerializeField] private int amount;

    protected override void ActivateForOwner()
    {
        int _amount = Math.Min(amount,GameplayManager.Instance.OpponentsStrangeMatter());
        GameplayManager.Instance.ChangeOpponentsStrangeMatter(-_amount);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}
