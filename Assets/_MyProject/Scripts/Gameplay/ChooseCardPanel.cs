using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCardPanel : MonoBehaviour
{
    public static ChooseCardPanel Instance;
    public static Action OnShowed;
    public static Action OnClosed;
    [SerializeField] private GameObject holder;
    [SerializeField] private Transform cardsHolder;
    [SerializeField] private Button cancelButton;
    
    private Vector3 sizeOfCards = new Vector3(2, 2, 1);
    private List<CardBase> shownCards = new ();
    private Action<CardBase> callBack;
    private Dictionary<CardBase, TablePlaceHandler> cardsDict = new();

    private void Awake()
    {
        Instance = this;
    }

    public void ShowCards(List<CardBase> _cardBase, Action<CardBase> _callBack,bool _enableCancel = false, bool 
    _hideCards=false)
    {
        cardsDict = new();
        cancelButton.gameObject.SetActive(false);
        callBack = _callBack;
        if (_cardBase==null || _cardBase.Count == 0)
        {
            _callBack?.Invoke(null);
            return;
        }
        
        foreach (var _card in _cardBase)
        {
            cardsDict.Add(_card, _card.GetTablePlace());
            _card.transform.SetParent(cardsHolder);
            _card.RotateCard();
            _card.transform.localScale = sizeOfCards;
            _card.gameObject.AddComponent<CardHandInteractions>().Setup(_card);
            shownCards.Add(_card);
            if (_hideCards)
            {
                _card.Display.Hide();
            }
        }

        if (_enableCancel)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() =>
            {
                ReturnCheckCard(null);
            });
            cancelButton.gameObject.SetActive(true);
        }
        
        holder.SetActive(true);
        OnShowed?.Invoke();
        CardHandInteractions.OnCardClicked += ReturnCheckCard;
    }

    private void Close()
    {
        OnClosed?.Invoke();
        ClearShownCards();
        holder.SetActive(false);
        CardHandInteractions.OnCardClicked -= ReturnCheckCard;
    }
    
    private void ReturnCheckCard(CardBase _clickedCard)
    {
        Close();
        callBack?.Invoke(_clickedCard);
    }

    private void ClearShownCards()
    {
        foreach (CardBase _shownCard in shownCards)
        {
            Debug.Log($"{_shownCard.gameObject.name} -> {_shownCard.CardPlace}");
            _shownCard.Display.UnHide();
            CardHandInteractions _cardHandler = _shownCard.GetComponent<CardHandInteractions>();
            if (_cardHandler != null)
            {
                Debug.Log(1111);
                Destroy(_cardHandler);
            }

            if (!(_shownCard.CardPlace == CardPlace.Hand || _shownCard.CardPlace == CardPlace.Graveyard))
            {
                var _place = cardsDict[_shownCard];
                GameplayManager.Instance.PlaceAbilityOnTable((_shownCard as AbilityCard).Details.Id,_place.Id,false);
                continue;
            }

            Debug.Log(3333);
            _shownCard.ReturnFromHand();
        }

        shownCards.Clear();
    }
}
