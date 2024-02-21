using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Veto : AbilityEffect
{
    public override void ActivateForOwner()
    {
        List<string> _availableOptions = new List<string>();
        
        _availableOptions.Add("Hand");
        _availableOptions.Add("Field");
        ResolveMultipleActions.Instance.Show(_availableOptions,OptionSelected, "Do you want to Veto card from Hand or from Ability Field");
    }

    private void OptionSelected(int _optionId)
    {
        Debug.Log("selected option: "+_optionId);
        List<AbilityCard> _availableCards = new List<AbilityCard>();
        
        if (_optionId==1)
        {
            Debug.Log(GameplayManager.Instance.OpponentPlayer.OwnedAbilities.Count);
            _availableCards = GameplayManager.Instance.OpponentPlayer.OwnedAbilities.ToList();
            Debug.Log(_availableCards.Count);
        }
        else
        {
            _availableCards = GameplayManager.Instance.OpponentPlayer.GetAbilities().ToList();
        }

        foreach (var _available in _availableCards)
        {
            Debug.Log(_available.name,_available.gameObject);
        }

        if (_availableCards.Count==0)
        {
            if (_optionId==1)
            {
                UIManager.Instance.ShowOkDialog("Opponent doesnt have ability in his hand, choose from field");
                OptionSelected(0);
                return;
            }
            UIManager.Instance.ShowOkDialog("Opponent doesnt have ability cards on field, choose from hand");
            OptionSelected(1);
            
            return;
        }
        
        GameplayState _state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        
        ChooseCardPanel.Instance.ShowCards(_availableCards.Cast<CardBase>().ToList(),VetoCard,_hideCards:_optionId==0);
        
        void VetoCard(CardBase _card)
        {
            GameplayManager.Instance.VetoCard(_card as AbilityCard);
            GameplayManager.Instance.GameState = _state;
            RemoveAction();
            OnActivated?.Invoke();
            AbilityCard.RotateToBeVertical();
        }
    }
}