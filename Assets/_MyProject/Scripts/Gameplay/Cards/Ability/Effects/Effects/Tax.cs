using System.Collections.Generic;
using System.Linq;

public class Tax : AbilityEffect
{
    public static bool IsActiveForMe;
    public static CardBase SelectedCard;
    
    public override void ActivateForOwner()
    {
        IsActiveForMe = true;
        List<CardBase> _cards = new();
        GameplayState _state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        List<AbilityCard> _availableCards = GameplayManager.Instance.OpponentPlayer.OwnedAbilities;
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
            UIManager.Instance.ShowOkDialog("Opponent doesn't own a CC ability card");
            GameplayManager.Instance.GameState = _state;
            return;
        }
        
        foreach (var _card in _availableCards)
        {
            _cards.Add(_card);
        }
        
        ChooseCardPanel.Instance.ShowCards(_cards,SetAsTax);

        void SetAsTax(CardBase _selectedCard)
        {
            SelectedCard = _selectedCard;
            RemoveAction();
            OnActivated?.Invoke();
            GameplayManager.Instance.GameState = _state;
            GameplayManager.Instance.SetTaxCard((_selectedCard as AbilityCard).Details.Id);
        }
        
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public void SetTaxedCard(int _cardId)
    {
        SelectedCard = FindObjectsOfType<AbilityCard>().ToList().Find(_card => _card.Details.Id == _cardId);
    }

    public override void ActivateForOther()
    {
        IsActiveForMe = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        CancelEffect();
    }

    public override void CancelEffect()
    {
        if (SelectedCard== null)
        {
            return;
        }
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        SelectedCard = null;
    }
}
