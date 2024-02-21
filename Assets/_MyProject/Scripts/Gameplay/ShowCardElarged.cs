using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShowCardElarged : MonoBehaviour
{
    public static Action OnShowed;
    public static Action OnHided;
    [SerializeField] private GameObject holder;
    [SerializeField] private GameObject animationHolder;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button flipButton;
    [SerializeField] private Image cardDisplay;
    
    private bool isShowingFrontSide;
    private Sprite frontImage;
    private Sprite backImage;
    private CardBase shownCard;
    
    private void OnEnable()
    {
        CardTableInteractions.OnCardPressed += Show;
        CardHandInteractions.OnCardPressed += Show;
        AbilityShopDisplay.OnCardPressed += Show;
        flipButton.onClick.AddListener(Flip);
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        CardTableInteractions.OnCardPressed -= Show;
        CardHandInteractions.OnCardPressed -= Show;
        AbilityShopDisplay.OnCardPressed -= Show;
        flipButton.onClick.RemoveListener(Flip);
        closeButton.onClick.RemoveListener(Close);
    }

    private void Show(CardBase _card)
    {
        if (_card==null)
        {
            return;
        }
        
        isShowingFrontSide = true;

        TablePlaceHandler _tablePlace = _card.GetTablePlace();
        if (_tablePlace==null)
        {
            ShowSelectedCard(_card);
            return;
        }

        List<CardBase> _availableCards = _tablePlace.GetCards();

        if (_availableCards.Count==1)
        {
            ShowSelectedCard(_availableCards[0]);
        }
        else if(_availableCards.Count>1)
        {
            ResolveMultipleActions.Instance.Show(_availableCards,ShowSelectedCard);
        }
    }

    private void ShowSelectedCard(CardBase _card)
    {
        shownCard = _card;
        if (_card is Card)
        {
            ShowCard(_card as Card);
        }
        else
        {
            ShowAbility(_card as AbilityCard);
        }
        
        animationHolder.transform.localScale=Vector3.zero;
        animationHolder.transform.DOScale(Vector3.one, 0.5f);
        holder.SetActive(true);
        OnShowed?.Invoke();
    }

    private void ShowCard(CardBase _card)
    {
        cardDisplay.transform.eulerAngles = new Vector3(0, 0, 90);
        TablePlaceHandler _tablePlace = _card.GetTablePlace();
        if (_tablePlace==null)
        {
            Close();
            return;
        }
        List<CardBase> _cardsOnField = _tablePlace.GetCards();
        if (_cardsOnField == null||_cardsOnField.Count==0)
        {
            Close();
            return;
        }

        if (_cardsOnField.Count==1)
        {
            ShowCard(_cardsOnField[0] as Card);
        }
        else
        {
            ResolveMultipleActions.Instance.Show(_cardsOnField,ShowCard);
        }
    }

    private void ShowCard(Card _card)
    {
        frontImage = _card.Details.Foreground;
        backImage = _card.Details.Background;
        cardDisplay.sprite = frontImage;
        if (_card is Guardian _guardian && !_guardian.IsChained)
        {
            cardDisplay.sprite = backImage;
        }
    }

    private void ShowAbility(AbilityCard _abilityCard)
    {
        if (isShowingFrontSide)
        {
            cardDisplay.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        frontImage = _abilityCard.Details.Foreground;
        backImage = _abilityCard.Details.Background;
        cardDisplay.sprite = frontImage;
    }

    private void Flip()
    {
        if (shownCard is Card _card && _card is Guardian _guardian && !_guardian.IsChained)
        {
            return;
        }
        RectTransform _cardBaseTransform = cardDisplay.GetComponent<RectTransform>();
        _cardBaseTransform.DOScale(new Vector3(1,0,1), 0.5f).OnComplete(() =>
        {
            isShowingFrontSide = !isShowingFrontSide;
            _cardBaseTransform.DOScale(Vector3.one,0.5f);
            cardDisplay.sprite = isShowingFrontSide ? frontImage : backImage;
        });
    }
    
    private void Close()
    {
        holder.SetActive(false);
        OnHided?.Invoke();
    }
}