using System.Collections.Generic;

public class CallToArms : AbilityEffect
{
    private int powerChange = 1;

    protected override void ActivateForOwner()
    {
        Guardian _guardian = GameplayManager.Instance.GetOpponentGuardian();
        if (_guardian==null)
        {
            return;
        }
        
        if (_guardian.IsChained)
        {
            _guardian.OnUnchained += ApplyEffectAndUpdateRoom;
        }
        else
        {
            ApplyEffect();
        }
        
        SetIsActive(true);
        OnActivated?.Invoke();
        RemoveAction();
    }

    private void ApplyEffectAndUpdateRoom()
    {
        ApplyEffect();
        RoomUpdater.Instance.ForceUpdate();
    }

    private void ApplyEffect()
    {
        if (GameplayManager.Instance.IsCardVetoed(UniqueId))
        {
            return;
        }
        
        List<Card> _minions = GameplayManager.Instance.GetAllCardsOfType(CardType.Minion, true);
        
        foreach (var _minion in _minions)
        {
            _minion.ChangeDamage(powerChange);
        }
        ManageActiveDisplay(true);
    }

    protected override void CancelEffect()
    {
        Guardian _guardian = GameplayManager.Instance.GetOpponentGuardian();
        _guardian.OnUnchained -= ApplyEffectAndUpdateRoom;

        List<Card> _minions = GameplayManager.Instance.GetAllCardsOfType(CardType.Minion, true);

        foreach (var _minion in _minions)
        {
            _minion.ChangeDamage(-powerChange);
        }
        
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}