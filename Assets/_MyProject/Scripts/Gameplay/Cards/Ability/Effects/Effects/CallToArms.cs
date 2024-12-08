using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CallToArms : AbilityEffect
{
    [SerializeField] private int powerChange;
    
    public override void ActivateForOwner()
    {
        Guardian _guardian = GameplayManager.Instance.GetMyGuardian() as Guardian;
        if (_guardian.IsChained)
        {
            _guardian.OnUnchained += () => { ApplyEffect(false);};
        }
        else
        {
            ApplyEffect(false);
        }
       

        SetIsActive(true);
        RemoveAction();
    }

    private void ApplyEffect(bool _applyToMyMinions)
    {
        if (GameplayManager.Instance.IsCardVetoed(UniqueId))
        {
            return;
        }
        List<Card> _minions = FindObjectsOfType<Card>().ToList().FindAll(_card => _card.Details.Type == CardType
            .Minion && _card.My == _applyToMyMinions).ToList();

        foreach (var _minion in _minions)
        {
            _minion.ChangeDamage(powerChange);
        }
        SetIsActive(true);
        ManageActiveDisplay(true);
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        List<Card> _minions = FindObjectsOfType<Card>().ToList().FindAll(_card => _card.Details.Type == CardType
            .Minion && _card.My == true).ToList();

        foreach (var _minion in _minions)
        {
            _minion.ChangeDamage(-powerChange);
        }
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
