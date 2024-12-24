using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CallToArms : AbilityEffect
{
    [SerializeField] private int powerChange;

    protected override void ActivateForOwner()
    {
        Guardian _guardian = GameplayManager.Instance.GetOpponentGuardian() as Guardian;
        if (_guardian==null)
        {
            return;
        }
        
        if (_guardian.IsChained)
        {
            _guardian.OnUnchained += ApplyEffect;
        }
        else
        {
            ApplyEffect();
        }
        
        RemoveAction();
        SetIsActive(true);
        OnActivated?.Invoke();
    }

    private void ApplyEffect()
    {
        if (GameplayManager.Instance.IsCardVetoed(UniqueId))
        {
            return;
        }
        List<Card> _minions = GameplayManager.Instance.GetAllCards().FindAll(_card => _card.Details.Type == CardType
            .Minion && _card.My).ToList();

        foreach (var _minion in _minions)
        {
            _minion.ChangeDamage(powerChange);
        }
        ManageActiveDisplay(true);
    }

    protected override void CancelEffect()
    {
        List<Card> _minions = GameplayManager.Instance.GetAllCards().FindAll(_card => _card.Details.Type == CardType
            .Minion && _card.My).ToList();

        foreach (var _minion in _minions)
        {
            _minion.ChangeDamage(-powerChange);
        }
        
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}
