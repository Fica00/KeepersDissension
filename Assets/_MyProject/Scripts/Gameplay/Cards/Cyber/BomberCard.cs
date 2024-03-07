using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BomberCard : CardSpecialAbility
{
    private int bombDamage = 3;
    [HideInInspector]public bool ExplodeOnDeath;


    private void OnEnable()
    {
        CardBase.OnGotDestroyed += CheckForCard;
        ExplodeOnDeath = true;
    }
    
    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= CheckForCard;
    }

    private void CheckForCard(CardBase _cardBase)
    {
        if (!ExplodeOnDeath)
        {
            return;
        }
        
        if (_cardBase != CardBase)
        {
            return;
        }

        List<TablePlaceHandler> _availablePlaces =
            GameplayManager.Instance.TableHandler.GetPlacesAround(TablePlaceHandler.Id,
                CardMovementType.EightDirections);

        foreach (var _availablePlace in _availablePlaces.ToList())
        {
            if (!_availablePlace.IsOccupied)
            {
                _availablePlaces.Remove(_availablePlace);
                continue;
            }

            CardBase _cardBaseOnPlace = _availablePlace.GetCard();
            Card _card = (Card)_cardBaseOnPlace;
            if (_card==null)
            {
                _availablePlaces.Remove(_availablePlace);
                continue;
            }

            CardAction _action = new CardAction()
            {
                FirstCardId = Card.Details.Id,
                SecondCardId = _card.Details.Id,
                StartingPlaceId = TablePlaceHandler.Id,
                FinishingPlaceId = _availablePlace.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = false,
                IsMy = CardBase.My,
                Damage = bombDamage
            };

            GameplayManager.Instance.ExecuteCardAction(_action, false);
            GameplayManager.Instance.SpawnBombEffect(TablePlaceHandler.Id);
        }
    }
}
