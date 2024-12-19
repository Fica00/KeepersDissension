using System.Collections;
using UnityEngine;

public class Void : AbilityEffect
{

    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        GameplayManager.Instance.SelectPlaceForSpecialAbility(PlaceId,20,PlaceLookFor.Empty,
        CardMovementType.EightDirections,false,LookForCardOwner.Both,PlaceVoid);

        void PlaceVoid(int _placeId)
        {
            if (_placeId==-1)
            {
                DialogsManager.Instance.ShowOkDialog("Looks like there is no empty space");
                RemoveAction();
                OnActivated?.Invoke();
                return;
            }

            GameplayPlayer _player = GameplayManager.Instance.MyPlayer;

            if (_player.Actions<=0)
            {
                DialogsManager.Instance.ShowOkDialog("You don't have anymore actions");
                RemoveAction();
                OnActivated?.Invoke();
                return;
            }

            _player.Actions--;
            GameplayManager.Instance.TellOpponentSomething("Opponent used Void");
            Card _marker = _player.GetCardOfType(CardType.Marker);
            if (_marker==null)
            {
                DialogsManager.Instance.ShowOkDialog("You don't have available marker");
                RemoveAction();
                OnActivated?.Invoke();
                return;
            }

            GameplayManager.Instance.PlaceCard(_marker,_placeId);
            AddEffectedCard(_marker.UniqueId);
            GameplayManager.Instance.ChangeSprite(_placeId,_marker.Details.Id,0);
            GameplayManager.OnCardMoved += CheckMovedCard;
            RemoveAction();
            OnActivated?.Invoke();
        }
    }

    private void CheckMovedCard(CardBase _movedCard, int _startingPlace, int _finishingPlace, bool _)
    {
        var _effectedCardBase = GetEffectedCards()[0];
        int _effectedPlaceId = _effectedCardBase.GetTablePlace().Id;
        if (_finishingPlace!=_effectedPlaceId)
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
                StartingPlaceId = _effectedPlaceId,
                FirstCardId = _effectedCard.UniqueId,
                FinishingPlaceId = _effectedPlaceId,
                SecondCardId = _effectedCard.UniqueId,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = false,
                Damage = 20,
                CanCounter = false,
                CanBeBlocked = false
            };
            GameplayManager.Instance.ExecuteCardAction(_attackAction);
        }
    }

    protected override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        GameplayPlayer _player = GameplayManager.Instance.MyPlayer;
        var _effectedCardBase = GetEffectedCards()[0];
        _player.DestroyCard(_effectedCardBase);
        GameplayManager.OnCardMoved -= CheckMovedCard;
        RemoveEffectedCard(_effectedCardBase.UniqueId);
        SetIsActive(false);
    }
}