using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shockwave : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        
        int _damage = FindObjectsOfType<AbilityCard>().ToList().FindAll(_ability => _ability.My &&_ability.Details
        .Type == AbilityCardType.CrowdControl && _ability.Details.Color == AbilityColor.Red && _ability.GetTablePlace() != null).Count;
        
        if (_damage==0)
        {
            Finish();
            DialogsManager.Instance.ShowOkDialog("You don't have any red ability card");
            return;
        }

        List<Card> _killedCards = new List<Card>();
        foreach (var _card in FindObjectsOfType<Card>().ToList())
        {
            if (!_card.IsAttackable())
            {
                continue;
            }

            if (_card.GetTablePlace()==null)
            {
                continue;
            }
            
            GameplayManager.Instance.DamageCardByAbility(_card.UniqueId, _damage, _didKill =>
            {
                if (_didKill)
                {
                    _killedCards.Add(_card);
                }
            });
        }
    }

    private void Finish()
    {
        OnActivated?.Invoke();
        RemoveAction();
    }
}
