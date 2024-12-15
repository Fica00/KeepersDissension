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

        if (!CardBase.GetIsMy())
        {
            return;
        }
        
        GameplayManager.Instance.PlayAudioOnBoth("FireCrackle", CardBase);

        CardBase _attacker = GameplayManager.Instance.TableHandler.GetPlace(AttackerPlace).GetCard();
        if (GameplayManager.Instance.IsAbilityActiveForMe<Collapse>())
        {
            if (!_attacker.GetIsMy())
            {
                DamageAttacker(AttackerPlace);
            }
        }
        else if (GameplayManager.Instance.IsAbilityActiveForOpponent<Collapse>())
        {
            if (_attacker.GetIsMy())
            {
                DamageAttacker(AttackerPlace);
            }
        }
        
        if (GameplayManager.Instance.IsAbilityActiveForMe<Immunity>() && _attacker.GetIsMy())
        {
            return;
        }
        if (GameplayManager.Instance.IsAbilityActiveForOpponent<Immunity>() && !_attacker.GetIsMy())
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
                FirstCardId = _cardThatCanBeAttacked.UniqueId,
                FinishingPlaceId = _placeAround.Id,
                SecondCardId = _cardThatCanBeAttacked.UniqueId,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = false,
                Damage = 1,
                CanCounter = false,
                GiveLoot = false
            };
            
            GameplayManager.Instance.ExecuteCardAction(_attackAction);
        }
    }
}