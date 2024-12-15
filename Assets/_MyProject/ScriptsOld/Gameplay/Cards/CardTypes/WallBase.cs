using System.Collections;
using UnityEngine;

public class WallBase : CardSpecialAbility
{
    public static int AttackerPlace;

    protected void DamageAttacker(int _place,float _delay=0)
    {
        StartCoroutine(DamageAttackerRoutine(_place, _delay));
    }
    
    private IEnumerator DamageAttackerRoutine(int _place,float _delay)
    {
        TablePlaceHandler _tablePlace = GameplayManager.Instance.TableHandler.GetPlace(_place);
        Card _card= _tablePlace.GetCardNoWall();
        
        if (_delay!=0)
        {
            yield return new WaitForSeconds(_delay);
        }
        
        if (_card==null)
        {
            yield break;
        }
        
        if (!_card.My)
        {
            yield break;
        }
        
        string _cardId =_card.UniqueId;
        CardAction _attackAction = new CardAction
        {
            StartingPlaceId = _card.GetTablePlace().Id,
            FirstCardId = _cardId,
            FinishingPlaceId = _card.GetTablePlace().Id,
            SecondCardId = _cardId,
            Type = CardActionType.Attack,
            Cost = 0,
            CanTransferLoot = false,
            Damage = 1,
            GiveLoot = true,
            CanCounter = false
        };
        
        GameplayManager.Instance.ExecuteCardAction(_attackAction);
    }
}
