using System.Collections.Generic;
using System.Linq;

public class GroundPound : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        Keeper _keeper = FindObjectsOfType<Keeper>().ToList().Find(_element => _element.My);
        TablePlaceHandler _keeperPlace = _keeper.GetTablePlace();
        List<TablePlaceHandler> _availablePlaces =
            GameplayManager.Instance.TableHandler.GetPlacesAround(_keeperPlace.Id, CardMovementType.EightDirections);
        foreach (var _availablePlace in _availablePlaces)
        {
            if (!_availablePlace.IsOccupied)
            {
                continue;
            }

            List<CardBase> _cardsOnPlace = _availablePlace.GetCards();
            foreach (var _cardOnPlace in _cardsOnPlace)
            {
                Card _card = ((Card)_cardOnPlace);
                if (!_card.IsAttackable())
                {
                    continue;
                }

                CardAction _action = new CardAction
                {
                    StartingPlaceId = _card.GetTablePlace().Id,
                    FirstCardId = _card.UniqueId,
                    FinishingPlaceId = _card.GetTablePlace().Id,
                    SecondCardId = _card.UniqueId,
                    Type = CardActionType.Attack,
                    Cost = 0,
                    CanTransferLoot = false,
                    Damage = _keeper.Damage,
                    CanCounter = false
                };
                
                GameplayManager.Instance.ExecuteCardAction(_action);
            }
        }

        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
    
}
