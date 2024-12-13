using System.Collections.Generic;
using System.Linq;

public class Tax : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        List<CardBase> _cards = new();
        List<AbilityCard> _availableCards = GameplayManager.Instance.OpponentPlayer.OwnedAbilities.ToList();
        foreach (var _availableCard in _availableCards.ToList())
        {
            if (_availableCard.Details.Type==AbilityCardType.CrowdControl)
            {
                continue;
            }

            _availableCards.Remove(_availableCard);
        }
        
        if (_availableCards.Count==0)
        {
            RemoveAction();
            OnActivated?.Invoke();
            DialogsManager.Instance.ShowOkDialog("Opponent doesn't own a CC ability card");
            return;
        }
        
        foreach (var _card in _availableCards)
        {
            _cards.Add(_card);
        }
        
        ChooseCardPanel.Instance.ShowCards(_cards,SetAsTax);

        void SetAsTax(CardBase _selectedCard)
        {
            AddEffectedCard((_selectedCard as Card)?.UniqueId);
            RemoveAction();
            OnActivated?.Invoke();
            GameplayManager.Instance.SetTaxCard(((AbilityCard)_selectedCard).Details.Id);
        }
        
        SetIsActive(true);
        ManageActiveDisplay(true);
    }

    protected override void CancelEffect()
    {
        SetIsActive(false);
        ManageActiveDisplay(false);
        RemoveEffectedCard(GetEffectedCards()[0].UniqueId);
    }
}
