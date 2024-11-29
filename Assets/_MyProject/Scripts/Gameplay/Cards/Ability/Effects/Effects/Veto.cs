using System.Collections.Generic;
using System.Linq;

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
        List<AbilityCard> _availableCards = new List<AbilityCard>();
        
        if (_optionId==1)
        {
            _availableCards = GameplayManager.Instance.OpponentPlayer.OwnedAbilities.ToList();
        }
        else
        {
            _availableCards = GameplayManager.Instance.OpponentPlayer.GetAbilities().ToList();
        }

        if (_availableCards.Count==0)
        {
            if (_optionId==1)
            {
                DialogsManager.Instance.ShowOkDialog("Opponent doesnt have ability in his hand, choose from field");
                OptionSelected(0);
                return;
            }
            DialogsManager.Instance.ShowOkDialog("Opponent doesnt have ability cards on field, choose from hand");
            OptionSelected(1);
            return;
        }
        
        GameplayState _state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        List<CardBase> _cards = new List<CardBase>();
        foreach (var _availableCard in _availableCards)
        {
            _cards.Add(_availableCard);
        }
        
        ChooseCardPanel.Instance.ShowCards(_cards,VetoCard,_hideCards:_optionId==0);
        
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