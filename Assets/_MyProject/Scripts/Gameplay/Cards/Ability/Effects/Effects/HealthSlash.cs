using System;
using System.Linq;
using UnityEngine;

public class HealthSlash : AbilityEffect
{
    public override void ActivateForOwner()
    {
        ApplyEffect(FindObjectsOfType<Keeper>().ToList().Find(_keeper=>!_keeper.My));
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        
    }

    private void ApplyEffect(Keeper _keeper)
    {
        int _newHealth = (int)Math.Floor(_keeper.Stats.Health / 2.0);
        int _damage = (int)_keeper.Stats.Health - _newHealth;

        if (_damage<1)
        {
            _damage = 1;
        }
        
        TablePlaceHandler _tablePlace = _keeper.GetTablePlace();
        CardAction _attackAction = new CardAction
        {
            StartingPlaceId = _tablePlace.Id,
            FirstCardId = _keeper.Details.Id,
            FinishingPlaceId = _tablePlace.Id,
            SecondCardId = _keeper.Details.Id,
            Type = CardActionType.Attack,
            Cost = 0,
            IsMy = true,
            CanTransferLoot = false,
            Damage = _damage,
            CanCounter = false,
            GiveLoot = false,
            CanBeBlocked = false
        };
        
        GameplayManager.Instance.ExecuteCardAction(_attackAction);
    }
}
