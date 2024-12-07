using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CallToArms : AbilityEffect
{
    [SerializeField] private int powerChange;
    private bool isActive;
    private bool isActiveForMe;
    
    public override void ActivateForOwner()
    {
        Guardian _guardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => !_guardian.My);
        if (_guardian.IsChained)
        {
            _guardian.OnUnchained += () => { ApplyEffect(true);};
        }
        else
        {
            ApplyEffect(true);
        }

        isActiveForMe = true;
        RemoveAction();
    }

    public override void ActivateForOther()
    {
        Guardian _guardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My);
        if (!_guardian.IsChained)
        {
            _guardian.OnUnchained += () => { ApplyEffect(false);};
        }
        else
        {
            ApplyEffect(false);
        }
    }

    private void ApplyEffect(bool _applyToMyMinions)
    {
        if (AbilityCard.IsVetoed)
        {
            return;
        }
        List<Card> _minions = FindObjectsOfType<Card>().ToList().FindAll(_card => _card.Details.Type == CardType
            .Minion && _card.My == _applyToMyMinions).ToList();

        foreach (var _minion in _minions)
        {
            _minion.ChangeDamage(powerChange);
        }
        isActive = true;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (!isActive) return;
        List<Card> _minions = FindObjectsOfType<Card>().ToList().FindAll(_card => _card.Details.Type == CardType
            .Minion && _card.My == isActiveForMe).ToList();

        foreach (var _minion in _minions)
        {
            _minion.ChangeDamage(-powerChange);
        }
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
