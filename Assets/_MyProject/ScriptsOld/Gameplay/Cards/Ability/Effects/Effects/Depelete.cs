using System;
using UnityEngine;

public class Depelete : AbilityEffect
{
    [SerializeField] private int amount;

    protected override void ActivateForOwner()
    {
        GameplayPlayer _player = GameplayManager.Instance.OpponentPlayer;
        int _amount = Math.Min(amount,_player.StrangeMatter);
        _player.RemoveStrangeMatter(_amount);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}
