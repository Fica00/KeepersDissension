using UnityEngine;

public class DragonWall : WallBase
{
    private void OnEnable()
    {
        CardBase.OnGotDestroyed += CheckCard;
    }

    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= CheckCard;
    }

    private void CheckCard(CardBase _card)
    {
        if (CardBase != _card)
        {
            return;
        }

        if (!CardBase.My)
        {
            return;
        }
        
        GameplayManager.Instance.PlayAudioOnBoth("FireCrackle", CardBase);

        CardBase _attacker = GameplayManager.Instance.TableHandler.GetPlace(AttackerPlace).GetCard();
        if (Collapse.IsActiveForMe)
        {
            if (!_attacker.My)
            {
                DamageAttacker(AttackerPlace);
            }
        }
        else if (Collapse.IsActiveForOpponent)
        {
            if (_attacker.My)
            {
                DamageAttacker(AttackerPlace);
            }
        }
        
        if (Immunity.IsActiveForMe && _attacker.My)
        {
            return;
        }
        if (Immunity.IsActiveForOpponent && !_attacker.My)
        {
            return;
        }
        
        foreach (var _placeAround in 
                 GameplayManager.Instance.TableHandler.GetPlacesAround(
                     Card.GetTablePlace().Id,
                     CardMovementType.EightDirections,_includeCenter:false))
        {
            if (!_placeAround.IsOccupied)
            {
                continue;
            }

            if (!_placeAround.ContainsWarrior())
            {
                continue;
            }

            Card _cardThatCanBeAttacked = _placeAround.GetCard();
                
            Debug.Log("Damagging",_placeAround.gameObject);

            CardAction _attackAction = new CardAction
            {
                StartingPlaceId = _placeAround.Id,
                FirstCardId = _cardThatCanBeAttacked.Details.Id,
                FinishingPlaceId = _placeAround.Id,
                SecondCardId = _cardThatCanBeAttacked.Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = 1,
                CanCounter = false,
                GiveLoot = false
            };
            
            GameplayManager.Instance.ExecuteCardAction(_attackAction);
        }
    }
}