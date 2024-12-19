using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivationFieldHandler : MonoBehaviour
{
    public static Action OnShowed;
    public static Action OnHided;
    [SerializeField] private GameObject holder;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform cardsHolder;
    
    private Vector3 sizeOfCards = new (2, 2, 1);
    private List<CardBase> shownCards = new ();
    private int placeId;


    
    private void OnEnable()
    {
        TablePlaceHandler.OnPlaceClicked -= CheckPlace;
        CardTableInteractions.OnPlaceClicked += CheckPlace;
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        TablePlaceHandler.OnPlaceClicked -= CheckPlace;
        CardTableInteractions.OnPlaceClicked -= CheckPlace;
        closeButton.onClick.RemoveListener(Close);
    }

    private void CheckPlace(TablePlaceHandler _clickedPlace)
    {
        if (_clickedPlace.Id != 65 && _clickedPlace.Id != -1)
        {
            return;
        }

        placeId = _clickedPlace.Id;
        ShowCards(_clickedPlace.GetCards());
    }

    private void ShowCards(List<CardBase> _cardBase)
    {
        if (_cardBase==null || _cardBase.Count == 0)
        {
            return;
        }
        
        foreach (var _card in _cardBase)
        {
            _card.transform.SetParent(cardsHolder);
            _card.PositionInHand();
            _card.transform.localScale = sizeOfCards;
            _card.gameObject.AddComponent<CardHandInteractions>().Setup(_card);
            shownCards.Add(_card);
        }
        
        holder.SetActive(true);
        CardHandInteractions.OnCardClicked += CheckCard;
        OnShowed?.Invoke();
    }

    private void Close()
    {
        ClearShownCards();
        holder.SetActive(false);
        CardHandInteractions.OnCardClicked -= CheckCard;
        OnHided?.Invoke();
    }

    private void ClearShownCards()
    {
        TablePlaceHandler _activationField = GameplayManager.Instance.TableHandler.GetPlace(placeId);
        foreach (var _shownCard in shownCards)
        {
            CardHandInteractions _cardHandler = _shownCard.GetComponent<CardHandInteractions>();
            if (_cardHandler!=null)
            {
                Destroy(_cardHandler);
            }
            _shownCard.PositionOnTable(_activationField);
        }
        
        shownCards.Clear();
    }
    
    private void CheckCard(CardBase _cardBase)
    {
        if (GameplayManager.Instance.GameState!=GameplayState.Playing && !GameplayManager.Instance.IsKeeperResponseAction)
        {
            return;
        }
        
        if (!shownCards.Contains(_cardBase))
        {
            return;
        }
        
        if (GameplayManager.Instance.IsAbilityActive<Subdued>() && GameplayCheats.CheckForCd)
        {
            DialogsManager.Instance.ShowOkDialog("Activation of the ability is blocked by Subdued ability");
            return;
        }

        int _indexOfCard = 1;
        foreach (var _shownCard in shownCards)
        {
            if (_shownCard==_cardBase)
            {
                break;
            }

            _indexOfCard++;
        }

        AbilityEffect _effect = (_cardBase as AbilityCard)?.Effect;
        int _amountOfCardsOnTop = shownCards.Count - _indexOfCard;
        bool _canReturn = _effect.Cooldown <= _amountOfCardsOnTop;
        if (!GameplayCheats.CheckForCd)
        {
            _canReturn = true;
        }
        if (_canReturn)
        {
            CardTableInteractions _tableInteractions = _cardBase.GetComponent<CardTableInteractions>();
            if (_tableInteractions!=null)
            {
                Destroy(_tableInteractions);
            }

            CardHandInteractions _handInteractions = _cardBase.GetComponent<CardHandInteractions>();
            if (_handInteractions!=null)
            {
                Destroy(_handInteractions);
            }
            shownCards.Remove(_cardBase);
            _cardBase.transform.SetParent(null);
            _cardBase.PositionInHand();
            Close();
            GameplayManager.Instance.ReturnAbilityFromActivationField(((AbilityCard)_cardBase).UniqueId);
        }
        else
        {
            DialogsManager.Instance.ShowOkDialog($"This card needs {_effect.Cooldown} on top of it but there is only " +
                                            $"{_amountOfCardsOnTop}");
        }
    }
}
