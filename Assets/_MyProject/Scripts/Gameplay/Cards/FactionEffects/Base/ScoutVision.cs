using System.Collections.Generic;

public class ScoutVision : CardSpecialAbility
{
    private void OnEnable()
    {
        GameplayManager.OnCardMoved += CheckCardThatMoved;
    }

    private void OnDisable()
    {
        GameplayManager.OnCardMoved -= CheckCardThatMoved;
    }

    private void CheckCardThatMoved(CardBase _cardThatMoved, int _startingPlace, int _finishingPlace)
    {
        if (_cardThatMoved != CardBase)
        {
            return;
        }

        List<Card> _cardsAround = GameplayManager.Instance.TableHandler.GetAttackableCards(_finishingPlace, CardMovementType.EightDirections);
        foreach (var _cardAround in _cardsAround)
        {
            if (_cardAround is not Marker _marker)
            {
                continue;
            }

            if (_marker.IsVoid)
            {
                continue;
            }

            bool _skip = false;
            foreach (var _card in FirebaseManager.Instance.RoomHandler.BoardData.Cards)
            {
                if (_card.WarriorAbilityData.BomberData == null)
                {
                    continue;
                }

                foreach (var _bomberData in _card.WarriorAbilityData.BomberData)
                {
                    if (_bomberData.BombPlace == _cardAround.GetTablePlace().Id)
                    {
                        GameplayManager.Instance.MarkMarkerAsBomb(_cardAround.UniqueId);
                        _skip = true;
                        break;
                    }
                }
            }

            if (_skip)
            {
                continue;
            }
            
            GameplayManager.Instance.DamageCardByAbility(_marker.UniqueId, 1,null);
        }
    }
}