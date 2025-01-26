using UnityEngine;

public class StrangeMatterOnTableHandler : MonoBehaviour
{
    private void OnEnable()
    {
        CardBase.OnGotDestroyed += PlaceStrangeMatter;
        GameplayManager.OnCardMoved += TryPickUpStrangeMatter;
    }

    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= PlaceStrangeMatter;
        GameplayManager.OnCardMoved -= TryPickUpStrangeMatter;
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
            TransferStrangeMatterToPlayer(_card);
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

        int _amountOfStrangeMatter = GetStrangeMatterForCard(_card) + _card.CardData.CarryingStrangeMatter;
        if (_amountOfStrangeMatter == 0)
        {
            return;
        }
        if (_card is Keeper)
        {
            if (_card.GetIsMy())
            {
                _amountOfStrangeMatter += GameplayManager.Instance.MyStrangeMatter();
                GameplayManager.Instance.ChangeMyStrangeMatter(-GameplayManager.Instance.MyStrangeMatter());
            }
            else
            {
                _amountOfStrangeMatter += GameplayManager.Instance.OpponentsStrangeMatter();
                GameplayManager.Instance.ChangeOpponentsStrangeMatter(-GameplayManager.Instance.OpponentsStrangeMatter());
            }
        }

        _card.CardData.CarryingStrangeMatter = 0;
        GameplayManager.Instance.AddStrangeMatterOnTable(_placeHandler.Id,_amountOfStrangeMatter);
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

    private void TransferStrangeMatterToPlayer(Card _card)
    {
        if (_card.GetIsMy())
        {
            GameplayManager.Instance.ChangeMyStrangeMatter(_card.CardData.CarryingStrangeMatter);
        }
        else
        {
            GameplayManager.Instance.ChangeOpponentsStrangeMatter(_card.CardData.CarryingStrangeMatter);
        }
        
        GameplayManager.Instance.NoteStrangeMatterAnimation(_card.CardData.CarryingStrangeMatter, _card.GetIsMy(), _card.GetTablePlace().Id);
        _card.CardData.CarryingStrangeMatter = 0;
    }

    public void TransferStrangeMatter(Card _firstCard, Card _secondCard)
    {
        _secondCard.CardData.CarryingStrangeMatter += _firstCard.CardData.CarryingStrangeMatter;
        _firstCard.CardData.CarryingStrangeMatter = 0;
        
        if (_secondCard is Keeper)
        {
            TransferStrangeMatterToPlayer(_secondCard);
        }
        
        GameplayManager.OnUpdatedStrangeMatterOnTable?.Invoke();
    }
}
