using System.Collections.Generic;

public class Veto : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        List<AbilityData> _availableCards = GameplayManager.Instance.GetOwnedAbilities(false);

        if (_availableCards.Count==0)
        {
            DialogsManager.Instance.ShowOkDialog("Opponent doesnt have available abilities");
            OnActivated?.Invoke();
            RemoveAction();
            return;
        }
        
        List<CardBase> _cards = new List<CardBase>();
        foreach (var _availableCard in _availableCards)
        {
            _cards.Add(GameplayManager.Instance.GetAbility(_availableCard.UniqueId));
        }
        
        ChooseCardImagePanel.Instance.Show(_cards,VetoCard);
        
        void VetoCard(CardBase _card)
        {
            SetIsActive(true);
            ManageActiveDisplay(true);
            AddEffectedCard(((AbilityCard)_card).UniqueId);
            GameplayManager.Instance.NoteVetoAnimation(EffectedCards[0],true);
            OnActivated?.Invoke();
            RemoveAction();
        }
    }

    protected override void CancelEffect()
    {
        GameplayManager.Instance.NoteVetoAnimation(EffectedCards[0],false);
        ClearEffectedCards();
        SetIsActive(false);
        ManageActiveDisplay(false);
    }

    public bool IsCardEffected(string _uniqueId)
    {
        return EffectedCards[0] == _uniqueId;
    }
}