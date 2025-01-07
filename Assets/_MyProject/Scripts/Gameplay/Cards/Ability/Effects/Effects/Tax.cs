using System.Collections.Generic;
using System.Linq;

public class Tax : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        ManageActiveDisplay(true);
        Activate();
    }

    private void Activate()
    {
        List<CardBase> _cards = new();
        var _availableCards = GameplayManager.Instance.GetOwnedAbilities(false);
        foreach (var _availableCard in _availableCards.ToList())
        {
            AbilityCard _card = GameplayManager.Instance.GetAbilityCard(_availableCard.UniqueId);
            bool _shouldSkip = false;
            foreach (var _effect in _card.EffectsHolder.GetComponents<AbilityEffect>())
            {
                if (_effect.Cooldown <= 0)
                {
                    _shouldSkip = true;
                    break;
                }
            }

            if (_shouldSkip)
            {
                continue;
            }
            
            _availableCards.Remove(_availableCard);
        }
        
        if (_availableCards.Count==0)
        {
            OnActivated?.Invoke();
            RemoveAction();
            DialogsManager.Instance.ShowOkDialog("Opponent doesn't own a CC ability card");
            return;
        }
        
        foreach (var _cardData in _availableCards)
        {
            AbilityCard _card = GameplayManager.Instance.GetAbilityCard(_cardData.UniqueId);
            _cards.Add(_card);
        }
        
        ChooseCardPanel.Instance.ShowCards(_cards,SetAsTax);
    }
    
    void SetAsTax(CardBase _selectedCard)
    {
        AddEffectedCard((_selectedCard as Card)?.UniqueId);
        OnActivated?.Invoke();
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        ManageActiveDisplay(false);
        RemoveEffectedCard(GetEffectedCards()[0].UniqueId);
    }
}
