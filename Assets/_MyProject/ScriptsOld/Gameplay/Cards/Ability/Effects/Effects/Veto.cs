using System.Collections.Generic;
using System.Linq;

public class Veto : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        List<string> _availableOptions = new List<string> { "Hand", "Field" };
        ResolveMultipleActions.Instance.Show(_availableOptions,OptionSelected, "Do you want to Veto card from Hand or from Ability Field");
    }

    private void OptionSelected(int _optionId)
    {
        List<AbilityData> _availableCards = GameplayManager.Instance.GetOwnedAbilities(false);

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
        
        GameplayState _state = GameplayManager.Instance.GameState();
        GameplayManager.Instance.SetGameState(GameplayState.UsingSpecialAbility);
        List<CardBase> _cards = new List<CardBase>();
        foreach (var _availableCard in _availableCards)
        {
            _cards.Add(GameplayManager.Instance.GetAbility(_availableCard.UniqueId));
        }
        
        ChooseCardPanel.Instance.ShowCards(_cards,VetoCard,_hideCards:_optionId==0);
        
        void VetoCard(CardBase _card)
        {
            SetIsActive(true);
            AddEffectedCard((_card as AbilityCard)?.UniqueId);
            GameplayManager.Instance.SetGameState(_state);
            RemoveAction();
            OnActivated?.Invoke();
            _card.RotateToBeVertical();
        }
    }

    protected override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        SetIsActive(false);
        ClearEffectedCards();
    }
}