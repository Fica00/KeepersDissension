using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tax : AbilityEffect
{
    private CardBase selectedCard;
    
    public override void ActivateForOwner()
    {
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
            Debug.Log(_selectedCard,_selectedCard.gameObject);
            selectedCard = _selectedCard;
            RemoveAction();
            OnActivated?.Invoke();
            GameplayManager.OnActivatedAbility += CheckAbility;
            GameplayManager.Instance.GameState = _state;
        }
        
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void CheckAbility(CardBase _playedAbility)
    {
        if (_playedAbility==selectedCard)
        {
            if (GameplayManager.Instance.OpponentPlayer.StrangeMatter<=0)
            {
                return;
            }
            GameplayManager.Instance.MyPlayer.StrangeMatter++;
            GameplayManager.Instance.TellOpponentToRemoveStrangeMatter(1);
        }
    }

    public override void CancelEffect()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        if (selectedCard==null)
        {
            return;
        }

        selectedCard = null;
        GameplayManager.OnActivatedAbility -= CheckAbility;
    }
}
