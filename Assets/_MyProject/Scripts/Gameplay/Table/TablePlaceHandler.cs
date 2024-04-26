using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TablePlaceHandler : MonoBehaviour,IPointerClickHandler
{
    public static Action<TablePlaceHandler> OnPlaceClicked;
    [SerializeField] private int id;
    [SerializeField] private bool isAbility;
    [SerializeField] private Image imageDisplay;

    public int Id => id;
    public bool IsAbility => isAbility;
    public bool IsOccupied => GetComponentInChildren<CardBase>()!=null;
    
    public bool HasLifeForce => GetComponentInChildren<LifeForce>()!=null;

    public bool ContainsMarker
    {
        get
        {
            if (!IsOccupied)
            {
                return false;
            }

            foreach (var _card in GetCards())
            {
                if (_card is Marker)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool IsActivationField => id == 65 || id == -1;

    public bool ContainsPortal
    {
        get
        {
            if (!IsOccupied)
            {
                return false;
            }

            foreach (var _card in GetCards())
            {
                if (_card is PortalCard)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    public bool ContainsVoid
    {
        get
        {
            if (!IsOccupied)
            {
                return false;
            }

            foreach (var _card in GetCards())
            {
                if (_card is Card _cardObj && _cardObj.IsVoid)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public Marker GetMarker()
    {
        if (!IsOccupied)
        {
            return null;
        }

        foreach (var _card in GetCards())
        {
            if (_card is Marker _marker)
            {
                return _marker;
            }
        }

        return null;
    }

    public bool ContainsWall
    {
        get
        {
            if (!IsOccupied)
            {
                return false;
            }

            foreach (var _card in GetCards())
            {
                if (_card is Wall)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool ContainsWarrior()
    {
        if (!IsOccupied)
        {
            return false;
        }

        foreach (var _card in GetCards())
        {
            if (_card.IsWarrior())
            {
                return true;
            }
        }

        return false;
    }
    
    public CardBase GetWarrior()
    {
        if (!IsOccupied)
        {
            return null;
        }

        foreach (var _card in GetCards())
        {
            if (_card.IsWarrior())
            {
                return _card;
            }
        }

        return null;
    }

    public List<CardBase> GetCards()
    {
        if (!IsOccupied)
        {
            return null;
        }

        return GetComponentsInChildren<CardBase>().ToList();
    }
    
    public CardBase GetWall()
    {
        if (!IsOccupied)
        {
            return null;
        }

        foreach (var _card in GetCards())
        {
            if (_card is Wall)
            {
                return _card;
            }
        }

        return null;
    }

    public Card GetCard(bool _prioritizeNonWall=true)
    {
        if (!IsOccupied)
        {
            return null;
        }

        if (_prioritizeNonWall)
        {
            foreach (var _cardBase in GetCards())
            {
                if (_cardBase is Card _card and not Wall)
                {
                    return _card;
                }
            }
        }
       
        foreach (var _cardBase in GetCards())
        {
            if (_cardBase is Card _card)
            {
                return _card;
            }
        }
        
        return null;
    }

    public Card GetCardNoWall()
    {
        if (!IsOccupied)
        {
            return null;
        }

        foreach (var _cardBase in GetCards())
        {
            Card _card = _cardBase as Card;
            if (_card is not Wall)
            {
                return _card;
            }
        }

        return null;
    }

    public void OnPointerClick(PointerEventData _eventData)
    {
        OnPlaceClicked?.Invoke(this);
    }
    
    public void SetColor(Color _color)
    {
        imageDisplay.color = _color;
    }

}
