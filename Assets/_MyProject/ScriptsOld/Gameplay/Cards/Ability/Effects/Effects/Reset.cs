using System.Collections.Generic;

public class Reset : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        // SetIsActive(true);
        // MoveToActivationField();
        // TablePlaceHandler _tablePlace = GameplayManager.Instance.MyPlayer.TableSideHandler.ActivationField;
        // List<CardBase> _cards = _tablePlace.GetCards();
        //
        // if (_cards.Contains(AbilityCard))
        // {
        //     _cards.Remove(AbilityCard);
        // }
        //
        // if (_cards.Count==0)
        // {
        //     DialogsManager.Instance.ShowOkDialog("You don't have any card on cooldown");
        //     RemoveAction();
        //     OnActivated?.Invoke();
        //     return;
        // }
        // ChooseCardPanel.Instance.ShowCards(_cards,ReduceCooldown);
        //
        // void ReduceCooldown(CardBase _selectedCard)
        // {
        //     AbilityCard _ability = _selectedCard as AbilityCard;
        //     AbilityEffect _effect = null;
        //     if (_ability != null)
        //     {
        //         _effect = _ability.Effect;
        //     }
        //
        //     if (_effect ==null)
        //     {
        //         return;
        //     }
        //
        //     _effect.SetRemainingCooldown(0);
        //     GameplayManager.Instance.PlaceAbilityOnTable(_ability.Details.Id);
        //     RemoveAction();
        //     OnActivated?.Invoke();
        //     IsActive = false;
        // }
    }
}