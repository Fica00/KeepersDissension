using System.Collections;
using UnityEngine;

public class Void : AbilityEffect
{
    private Card placedMarker;
    private int effectedPlace;
    
    public override void ActivateForOwner()
    {
        GameplayManager.Instance.SelectPlaceForSpecialAbility(AbilityCard.GetTablePlace().Id,20,PlaceLookFor.Empty,
        CardMovementType.EightDirections,false,LookForCardOwner.Both,PlaceVoid);

        void PlaceVoid(int _placeId)
        {
            if (_placeId==-1)
            {
                UIManager.Instance.ShowOkDialog("Looks like there is no empty space");
                RemoveAction();
                OnActivated?.Invoke();
                return;
            }

            GameplayPlayer _player = GameplayManager.Instance.MyPlayer;

            if (_player.Actions<=0)
            {
                UIManager.Instance.ShowOkDialog("You don't have anymore actions");
                RemoveAction();
                OnActivated?.Invoke();
                return;
            }

            _player.Actions--;
            GameplayManager.Instance.TellOpponentSomething("Opponent used Void");
            Card _marker = _player.GetCard(CardType.Marker);
            if (_marker==null)
            {
                UIManager.Instance.ShowOkDialog("You don't have available marker");
                RemoveAction();
                OnActivated?.Invoke();
                return;
            }

            GameplayManager.Instance.PlaceCard(_marker,_placeId);
            placedMarker = _marker;
            effectedPlace = _placeId;
            GameplayManager.Instance.ChangeSprite(_placeId,_marker.Details.Id,0);
            GameplayManager.OnCardMoved += CheckMovedCard;
            RemoveAction();
            OnActivated?.Invoke();
        }
    }

    private void CheckMovedCard(CardBase _movedCard, int _startingPlace, int _finishingPlace, bool _)
    {
        if (_finishingPlace!=effectedPlace)
        {
            return;
        }
        StartCoroutine(Activate());

        IEnumerator Activate()
        {
            yield return new WaitForSeconds(1);
            Card _effectedCard = _movedCard as Card;
            if (_effectedCard==null)
            {
                yield break;
            }

            if (_effectedCard.SpecialAbilities.Find(_effect => _effect is BomberCard))
            {
                GameplayManager.Instance.DestroyBombWithoutActivatingIt(_effectedCard.Details.Id, _effectedCard.My);
                yield break;
            }
            
            CardAction _attackAction = new CardAction
            {
                StartingPlaceId = effectedPlace,
                FirstCardId = _effectedCard.Details.Id,
                FinishingPlaceId = effectedPlace,
                SecondCardId = _effectedCard.Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = 20,
                CanCounter = false,
                CanBeBlocked = false
            };
            GameplayManager.Instance.ExecuteCardAction(_attackAction);
        }
    }

    public override void CancelEffect()
    {
        if (placedMarker==null)
        {
            return;
        }

        GameplayPlayer _player =
            placedMarker.My ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;
        _player.DestroyCard(placedMarker);
        GameplayManager.OnCardMoved -= CheckMovedCard;
        placedMarker = null;
    }
}
