using System.Collections.Generic;

public class Reset : AbilityEffect
{
    public static bool IsActive;

    private void Awake()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        IsActive = true;
        MoveToActivationField();
        TablePlaceHandler _tablePlace = GameplayManager.Instance.MyPlayer.TableSideHandler.ActivationField;
        List<CardBase> _cards = _tablePlace.GetCards();

        if (_cards.Contains(AbilityCard))
        {
            _cards.Remove(AbilityCard);
        }
        
        if (_cards.Count==0)
        {
            UIManager.Instance.ShowOkDialog("You don't have any card on cooldown");
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        ChooseCardPanel.Instance.ShowCards(_cards,ReduceCooldown);

        void ReduceCooldown(CardBase _selectedCard)
        {
            AbilityCard _ability = _selectedCard as AbilityCard;
            AbilityEffect _effect = null;
            if (_ability != null)
            {
                _effect = _ability.GetEffect();
            }

            if (_effect ==null)
            {
                return;
            }

            _effect.SetCooldown(0);
            GameplayManager.Instance.PlaceAbilityOnTable(_ability.Details.Id);
            RemoveAction();
            OnActivated?.Invoke();
            IsActive = false;
        }
    }
}
