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

    private void Awake()
    {
        Instance = this;
    }

    public void ShowCards(List<CardBase> _cardBase, Action<CardBase> _callBack,bool _enableCancel = false, bool 
    _hideCards=false)
    {
        cancelButton.gameObject.SetActive(false);
        callBack = _callBack;
        if (_cardBase==null || _cardBase.Count == 0)
        {
            _callBack?.Invoke(null);
            return;
        }
        
        foreach (var _card in _cardBase)
        {
            _card.transform.SetParent(cardsHolder);
            _card.PositionInHand();
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
        foreach (var _shownCard in shownCards)
        {
            _shownCard.Display.UnHide();
            CardHandInteractions _cardHandler = _shownCard.GetComponent<CardHandInteractions>();
            if (_cardHandler!=null)
            {
                Destroy(_cardHandler);
            }
            
            if (!(_shownCard.CardPlace == CardPlace.Hand || _shownCard.CardPlace == CardPlace.Graveyard))
            {
                continue;
            }

            _shownCard.ReturnFromHand();
        }
        
        shownCards.Clear();
    }
}
