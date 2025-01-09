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
    [SerializeField] private Button flip2Button;
    [SerializeField] private Image cardDisplay;
    [SerializeField] private Image abilityDisplay;
    
    private bool isShowingFrontSide;
    private Sprite frontImage;
    private Sprite backImage;
    private bool isSelectedCardGuardian;
    private bool isGuardianChained;
    
    private void OnEnable()
    {
        CardTableInteractions.OnCardPressed += Show;
        CardHandInteractions.OnCardPressed += Show;
        AbilityShopDisplay.OnCardPressed += Show;
        flipButton.onClick.AddListener(Flip);
        flip2Button.onClick.AddListener(Flip);
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        CardTableInteractions.OnCardPressed -= Show;
        CardHandInteractions.OnCardPressed -= Show;
        AbilityShopDisplay.OnCardPressed -= Show;
        flipButton.onClick.RemoveListener(Flip);
        flip2Button.onClick.RemoveListener(Flip);
        closeButton.onClick.RemoveListener(Close);
    }

    private void Show(AbilityData _abilityData)
    {
        if (_abilityData == null)
        {
            return;
        }
        
        isSelectedCardGuardian = false;
        isGuardianChained = false;
        isShowingFrontSide = true;
        
        cardDisplay.gameObject.SetActive(false);
        abilityDisplay.gameObject.SetActive(true);
        if (isShowingFrontSide)
        {
            abilityDisplay.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        
        float _screenWidth = Screen.width;
        float _screenHeight = Screen.height;

        float _targetSize = Mathf.Min(_screenWidth, _screenHeight) * 0.9f;

        RectTransform _rectTransform = abilityDisplay.GetComponent<RectTransform>();
        Sprite _sprite = CardsManager.Instance.GetAbilityImage(_abilityData.CardId);
        float _aspectRatio = _sprite.rect.width / _sprite.rect.height;
        _rectTransform.sizeDelta = new Vector2(_targetSize, _targetSize / _aspectRatio);
        frontImage = _sprite;
        backImage = CardsManager.Instance.GetAbilityBackground(_abilityData.CardId);
        abilityDisplay.sprite = frontImage;
        
        animationHolder.transform.localScale=Vector3.zero;
        animationHolder.transform.DOScale(Vector3.one, 0.5f);
        holder.SetActive(true);
        OnShowed?.Invoke();
    }

    private void Show(CardBase _card)
    {
        if (_card==null)
        {
            return;
        }

        isSelectedCardGuardian = _card is Guardian;
        if (isSelectedCardGuardian)
        {
            isGuardianChained = (_card as Guardian).IsChained;
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

    private void ShowCard(Card _card)
    {
        cardDisplay.gameObject.SetActive(true);
        abilityDisplay.gameObject.SetActive(false);
        frontImage = _card.Details.Foreground;
        backImage = _card.Details.Background;
        cardDisplay.sprite = frontImage;
        if (_card is Guardian _guardian && !_guardian.IsChained)
        {
            cardDisplay.sprite = backImage;
        }
        cardDisplay.transform.eulerAngles = new Vector3(0, 0, 90);
    }

    private void ShowAbility(AbilityCard _abilityCard)
    {
        cardDisplay.gameObject.SetActive(false);
        abilityDisplay.gameObject.SetActive(true);
        if (isShowingFrontSide)
        {
            abilityDisplay.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        
        float _screenWidth = Screen.width;
        float _screenHeight = Screen.height;

        float _targetSize = Mathf.Min(_screenWidth, _screenHeight) * 0.9f;

        RectTransform _rectTransform = abilityDisplay.GetComponent<RectTransform>();
        float _aspectRatio = _abilityCard.Details.Foreground.rect.width / _abilityCard.Details.Foreground.rect.height;
        _rectTransform.sizeDelta = new Vector2(_targetSize, _targetSize / _aspectRatio);
        frontImage = _abilityCard.Details.Foreground;
        backImage = _abilityCard.Details.Background;
        abilityDisplay.sprite = frontImage;
    }

    private void Flip()
    {
        Debug.Log(1111);
        if (isShowingFrontSide && !isGuardianChained)
        {
            Debug.Log(22222);
            return;
        }
        
        RectTransform _cardBaseTransform = cardDisplay.GetComponent<RectTransform>();
        _cardBaseTransform.DOScale(new Vector3(1,0,1), 0.5f).OnComplete(() =>
        {
            isShowingFrontSide = !isShowingFrontSide;
            _cardBaseTransform.DOScale(Vector3.one,0.5f);
            cardDisplay.sprite = isShowingFrontSide ? frontImage : backImage;
            abilityDisplay.sprite = isShowingFrontSide ? frontImage : backImage;
        });
    }
    
    private void Close()
    {
        holder.SetActive(false);
        OnHided?.Invoke();
    }
}