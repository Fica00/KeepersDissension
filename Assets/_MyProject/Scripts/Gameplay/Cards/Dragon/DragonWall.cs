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
            if (AttackerPlace == -1)
            {
                return;
            }
            if (Collapse.IsActive)
            {
                DamageAttacker(AttackerPlace);
            }
        }
        
        GameplayManager.Instance.PlayAudioOnBoth("FireCrackle", CardBase);

        if (Immunity.IsActiveForMe || Immunity.IsActiveForOpponent)
        {
            return;
        }

        if (Card.My)
        {
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
        

        if (Collapse.IsActive)
        {
            if (AttackerPlace == -1)
            {
                return;
            }
            DamageAttacker(AttackerPlace);
        }
    }
}