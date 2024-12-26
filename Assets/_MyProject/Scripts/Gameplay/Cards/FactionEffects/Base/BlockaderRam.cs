using System;
using UnityEngine;

public class BlockaderRam : CardSpecialAbility
{
    public void TryAndPush(string _firstCardId, string _secondCardId)
    {
        TryToPlaySoundEffect();
        int _secondCardsPlace = GameplayManager.Instance.GetCard(_secondCardId).GetTablePlace().Id;

        if (CanPushCard(_firstCardId, _secondCardId))
        {
            PushCard(_firstCardId, _secondCardId, () =>
            {
                MoveSelf(_firstCardId, _secondCardsPlace, ClearActions);
            });
        }
        else
        {
            GameplayManager.Instance.DamageCardByAbility(_secondCardId, 1, _didKillCard =>
            {
                if (!_didKillCard)
                {
                    ClearActions();
                    ReduceActions();
                    return;
                }

                MoveSelf(_firstCardId, _secondCardsPlace, ClearActions);
            });
        }
    }

    private void ClearActions()
    {
        GameplayManager.Instance.HideCardActions();
    }

    private void TryToPlaySoundEffect()
    {
        if (Card is Keeper) return;

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

    private bool CanPushCard(string _firstCardId, string _secondCardId)
    {
        Card _secondCard = GameplayManager.Instance.GetCard(_secondCardId);

        if (!_secondCard.CheckCanMove())
        {
            return false;
        }

        int _firstCardPlace = GameplayManager.Instance.GetCard(_firstCardId).GetTablePlace().Id;
        int _secondCardPlace = _secondCard.GetTablePlace().Id;

        TablePlaceHandler _placeInFront = GameplayManager.Instance.TableHandler.CheckForPlaceInFront(_firstCardPlace, _secondCardPlace);

        if (_placeInFront == null)
        {
            return false;
        }

        if (_placeInFront.IsAbility)
        {
            return false;
        }

        if (_placeInFront.IsOccupied)
        {
            return false;
        }

        Debug.Log(_placeInFront.Id, _placeInFront.gameObject);
        return true;
    }

    private void PushCard(string _firstCardId, string _secondCardId, Action _callBack)
    {
        Card _secondCard = GameplayManager.Instance.GetCard(_secondCardId);
        int _firstCardPlace = GameplayManager.Instance.GetCard(_firstCardId).GetTablePlace().Id;
        int _secondCardPlace = _secondCard.GetTablePlace().Id;

        TablePlaceHandler _placeInFront = GameplayManager.Instance.TableHandler.CheckForPlaceInFront(_firstCardPlace, _secondCardPlace);
        GameplayManager.Instance.ExecuteMove(_secondCardPlace, _placeInFront.Id, _secondCardId, _callBack);
    }

    private void MoveSelf(string _cardId, int _newPlaceId, Action _callBack)
    {
        int _currentPlaceId = GameplayManager.Instance.GetCard(_cardId).GetTablePlace().Id;
        GameplayManager.Instance.ExecuteMove(_currentPlaceId, _newPlaceId, _cardId, () => { HandleMoveOutcome(_callBack); });
    }
    
    private void HandleMoveOutcome(Action _callBack)
    {
        ReduceActions();
        _callBack?.Invoke();
    }

    private void ReduceActions()
    {
        GameplayManager.Instance.MyPlayer.Actions--;
        if (GameplayManager.Instance.MyPlayer.Actions > 0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }
}