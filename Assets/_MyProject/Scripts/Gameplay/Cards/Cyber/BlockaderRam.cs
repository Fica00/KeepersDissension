using System.Collections;
using UnityEngine;

public class BlockaderRam : CardSpecialAbility
{
    private int chanceForPush = 100;
    
    public void TryAndPush(int _placeIdOfFirstCard, int _placeIdOfSecondCard, int _firstCardId, int _secondCardId)
    {
        Card _cardInFrontOfSecondCard = GameplayManager.Instance.TableHandler.CheckForCardInFront(_placeIdOfFirstCard, _placeIdOfSecondCard);
        if (_cardInFrontOfSecondCard==null)
        {
            //push second card
            if (Card is not Keeper)
            {
                if (Card.Details.Faction.IsCyber)
                {
                    GameplayManager.Instance.PlayAudioOnBoth("DontMindMe", Card);
                }
                else if (Card.Details.Faction.IsDragon)
                {
                    GameplayManager.Instance.PlayAudioOnBoth("YouAppearToBeInMyWay", Card);
                }
                else if (Card.Details.Faction.IsForest)
                {
                    GameplayManager.Instance.PlayAudioOnBoth("IAskedNicely", Card);
                }
                else if (Card.Details.Faction.IsSnow)
                {
                    GameplayManager.Instance.PlayAudioOnBoth("OutOfMyWay", Card);
                }
            }
            
            int _pushedCardPlaceId = GameplayManager.Instance.PushCardForward(_placeIdOfFirstCard, _placeIdOfSecondCard, 
                chanceForPush);
            if (_pushedCardPlaceId!=-1)
            {
                StartCoroutine(MoveSelfRoutine());
            }

            GameplayManager.Instance.MyPlayer.Actions--;
        }
        else
        {
            //damage second card
            CardAction _damageAction = new CardAction
            {
                StartingPlaceId = _placeIdOfFirstCard,
                FirstCardId = _firstCardId,
                FinishingPlaceId = _placeIdOfSecondCard,
                SecondCardId = _secondCardId,
                Type = CardActionType.Attack,
                Cost = 1,
                IsMy = true,
                CanTransferLoot = false,
                Damage = 1,
                CanCounter = false,
                GiveLoot = false
            };
            
            GameplayManager.Instance.ExecuteCardAction(_damageAction);
        }
        
        
        IEnumerator MoveSelfRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            CardAction _moveSelf = new CardAction()
            {
                FirstCardId = Card.Details.Id,
                StartingPlaceId = _placeIdOfFirstCard,
                FinishingPlaceId = _placeIdOfSecondCard,
                Type = CardActionType.Move,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false,
            };

            GameplayManager.Instance.ExecuteCardAction(_moveSelf);
        }
    }
}
