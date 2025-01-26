using UnityEngine;

public class StrangeMatterOnTableHandler : MonoBehaviour
{
    private void OnEnable()
    {
        CardBase.OnGotDestroyed += PlaceStrangeMatter;
        GameplayManager.OnCardMoved += TryPickUpStrangeMatter;
        GameplayManager.OnCardMoved += TryAddStrangeMatterToPlayer;
    }

    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= PlaceStrangeMatter;
        GameplayManager.OnCardMoved -= TryPickUpStrangeMatter;
        GameplayManager.OnCardMoved -= TryAddStrangeMatterToPlayer;
    }

    private void TryPickUpStrangeMatter(CardBase _cardThatMoved, int _startingPlace, int _finishingPlace)
    {
        if (!ShouldIHandleStrangeMatter())
        {
            return;
        }
        
        int _amount = 0;
        var _strangeMatter = GameplayManager.Instance.GetStrangeMatterOnPlace(_finishingPlace);
        foreach (var _strangeMatterData in _strangeMatter)
        {
            _amount += _strangeMatterData.Amount;
        }

        if (_amount==0)
        {
            return;
        }

        Card _card = _cardThatMoved as Card;
        CarryStrangeMatter(_card, _amount);
        if (_card is Keeper)
        {
            PickUpStrangeMatter(_card);
        }
        
        GameplayManager.Instance.RemoveStrangeMatterFromTable(_strangeMatter);
    }

    private void CarryStrangeMatter(Card _card, int _amount)
    {
        _card.CardData.CarryingStrangeMatter += _amount;
    }

    private void PlaceStrangeMatter(CardBase _cardBase)
    {
        if (!ShouldIHandleStrangeMatter())
        {
            return;
        }

        if (_cardBase is not Card _card)
        {
            return;
        }

        TablePlaceHandler _placeHandler = _card.GetTablePlace();
        if (_placeHandler==null)
        {
            return;
        }

        GameplayManager.Instance.AddStrangeMatterOnTable(_placeHandler.Id,GetStrangeMatterForCard(_card));
    }
    
    private int GetStrangeMatterForCard(Card _card)
    {
        if (_card is Minion)
        {
            return 2;
        }

        if (_card is Guardian)
        {
            return 10;
        }

        if (_card is Keeper)
        {
            return 5;
        }

        return 0;
    }

    private bool ShouldIHandleStrangeMatter()
    {
        if (!GameplayManager.Instance.IsMyTurn())
        {
            if (!GameplayManager.Instance.IsMyResponseAction())
            {
                return false;
            }
        }

        return true;
    }
    
    private void TryAddStrangeMatterToPlayer(CardBase _cardThatMoved, int _startingPlace, int _finishingPlace)
    {
        if (!ShouldIHandleStrangeMatter())
        {
            return;
        }

        if (_cardThatMoved is not Card _card)
        {
            return;
        }

        if (_card is Keeper)
        {
            return;
        }

        if (_card.CardData.CarryingStrangeMatter==0)
        {
            return;
        }

        if (!IsKeeperInRange(_finishingPlace, _card))
        {
            return;
        }

        PickUpStrangeMatter(_card);
        GameplayManager.OnUpdatedStrangeMatterOnTable?.Invoke();
    }

    private bool IsKeeperInRange(int _place, Card _card)
    {
        foreach (var _placeAround in GameplayManager.Instance.TableHandler.GetPlacesAround(_place, _card.MovementType, _card.Range, false))
        {
            if (!_placeAround.IsOccupied)
            {
                continue;
            }

            foreach (var _cardOnPlace in _placeAround.GetCards())
            {
                if (_cardOnPlace is not Keeper)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    private void PickUpStrangeMatter(Card _card)
    {
        if (_card.GetIsMy())
        {
            GameplayManager.Instance.ChangeMyStrangeMatter(_card.CardData.CarryingStrangeMatter);
        }
        else
        {
            GameplayManager.Instance.ChangeOpponentsStrangeMatter(_card.CardData.CarryingStrangeMatter);
        }
        
        _card.CardData.CarryingStrangeMatter = 0;
    }
}
