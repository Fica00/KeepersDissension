using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hinder : AbilityEffect
{
    private List<CardBase> effectedCards = new ();
    private Keeper keeper;
    
    public override void ActivateForOwner()
    {
        RemoveAction();
        OnActivated?.Invoke();
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        CheckAllPlacesAround(keeper.GetTablePlace().Id);
        GameplayManager.OnCardMoved += CheckCard;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void CheckAllPlacesAround(int _placeId)
    {
        foreach (var _tablePlace in GameplayManager.Instance.TableHandler.GetPlacesAround(_placeId, 
                     CardMovementType.EightDirections))
        {
            if (!_tablePlace.IsOccupied)
            {
                continue;
            }
            foreach (var _card in _tablePlace.GetCards())
            {
                CheckCard(_card);
            }
        }
    }

    private void OnDisable()
    {
        if (keeper!=null)
        {
            GameplayManager.OnCardMoved -= CheckCard;
        }
    }

    private void CheckCard(CardBase _card, int _arg2, int _arg3)
    {
        CheckCard(_card);
    }

    private void CheckCard(CardBase _card)
    {
        if (!_card.IsWarrior())
        {
            return;
        }
        
        
        if (_card == keeper)
        {
            CheckAllPlacesAround(_card.GetTablePlace().Id);
            foreach (var _effectedCard in effectedCards.ToList())
            {
                CheckCard(_effectedCard);
            }
            return;
        }

        if (_card.My)
        {
            return;
        }
        
        TablePlaceHandler _tablePlaceHandler = _card.GetTablePlace();
        if (_tablePlaceHandler ==null)
        {
            return;
        }
        
        int _distance =
            GameplayManager.Instance.TableHandler.DistanceBetweenPlaces(keeper.GetTablePlace(),
                _tablePlaceHandler);
        if (_distance<=1)
        {
            GameplayManager.Instance.ChangeMovementForCard(_tablePlaceHandler.Id,false);
            effectedCards.Add(_card);
        }
        else
        {
            if (effectedCards.Contains(_card))
            {
                effectedCards.Remove(_card);
                GameplayManager.Instance.ChangeMovementForCard(_tablePlaceHandler.Id,true);
            }
        }
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        
        GameplayManager.OnCardMoved += CheckCard;
        keeper = null;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
