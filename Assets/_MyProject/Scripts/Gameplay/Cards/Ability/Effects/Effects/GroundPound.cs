using System.Collections.Generic;

public class GroundPound : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        TablePlaceHandler _keeperPlace = _keeper.GetTablePlace();
        List<TablePlaceHandler> _availablePlaces = GameplayManager.Instance.TableHandler.GetPlacesAround(_keeperPlace.Id, CardMovementType.EightDirections);
        
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

                GameplayManager.Instance.DamageCardByAbility(_card.UniqueId, 1, null);
            }
        }

        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }
}