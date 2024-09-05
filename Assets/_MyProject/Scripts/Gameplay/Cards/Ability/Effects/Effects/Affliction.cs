using System;
using System.Linq;
using UnityEngine;

public class Affliction : AbilityEffect
{
    public override void ActivateForOwner()
    {
        Guardian _opponentsGuardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My == false);
        RemoveAction();
        int _damage = Convert.ToInt32(_opponentsGuardian.Stats.Health / 2);
        _damage = Math.Clamp(_damage, 1, int.MaxValue);
        CardAction _damageAction = new CardAction
        {
            StartingPlaceId = _opponentsGuardian.GetTablePlace().Id,
            FirstCardId = _opponentsGuardian.Details.Id,
            SecondCardId = _opponentsGuardian.Details.Id,
            FinishingPlaceId = _opponentsGuardian.GetTablePlace().Id,
            Type = CardActionType.Attack,
            Cost = 0,
            IsMy = true,
            CanTransferLoot = false,
            Damage = _damage,
            CanCounter = false,
            GiveLoot = false,
        };
        GameplayManager.Instance.ExecuteCardAction(_damageAction);

        OnActivated?.Invoke();
    }
}
