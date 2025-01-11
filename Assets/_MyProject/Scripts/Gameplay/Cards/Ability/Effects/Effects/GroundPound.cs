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
            if (_cardsOnPlace.Count == 0)
            {
                continue;
            }

            if (_cardsOnPlace.Count>1)
            {
                Card _card = _availablePlace.GetMarker();
                if (_card)
                {
                    GameplayManager.Instance.DamageCardByAbility(_card.UniqueId, 1, null);
                }
            }
            else
            {
                Card _card = _availablePlace.GetCard();
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