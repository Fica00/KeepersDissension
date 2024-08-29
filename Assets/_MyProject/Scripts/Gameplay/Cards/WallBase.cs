using UnityEngine;

public class WallBase : CardSpecialAbility
{
    public static int AttackerPlace;

    protected void DamageAttacker(int _placeId)
    {
        TablePlaceHandler _tablePlace = GameplayManager.Instance.TableHandler.GetPlace(_placeId);
        Card _card= _tablePlace.GetCardNoWall();
        if (_card==null)
        {
            Debug.Log(66666);
            return;
        }
        if (!_card.My)
        {
            Debug.Log(7777);
            return;
        }
            Debug.Log(8888);
        int _cardId =_card.Details.Id;
        CardAction _attackAction = new CardAction
        {
            StartingPlaceId = _placeId,
            FirstCardId = _cardId,
            FinishingPlaceId = _placeId,
            SecondCardId = _cardId,
            Type = CardActionType.Attack,
            Cost = 0,
            IsMy = true,
            CanTransferLoot = false,
            Damage = 1,
            GiveLoot = true,
            CanCounter = false
        };
        
        GameplayManager.Instance.ExecuteCardAction(_attackAction);
            Debug.Log(9999);
    }
}
