using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardInHandDisplay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static Action<CardBase> OnClicked;
    public static Action<CardBase> OnCardHolded;
    
    private const float LONG_PRESS_THRESHOLD = 0.5f;
    

    [SerializeField] private Image image;
    [SerializeField] private Button button;

    public string UniqueId { get; private set; }
    private CardBase card;
    private bool isPointerDown;
    private bool longPressTriggered;
    private float pointerDownTimer;

    private void OnEnable()
    {
        button.onClick.AddListener(Select);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Select);
    }

    private void Select()
    {
        Debug.Log("Click detected");
        OnClicked?.Invoke(card);
    }

    public void Setup(string _uniqueId, bool _isAbility)
    {
        UniqueId = _uniqueId;
        card = _isAbility ? GameplayManager.Instance.GetAbilityCard(_uniqueId) : GameplayManager.Instance.GetCard(_uniqueId);
        if (_isAbility)
        {
            var _abilityCard = (AbilityCard)card;
            image.sprite = _abilityCard.Details.Foreground;
        }
        else
        {
            var _warriorCard = (Card)card;
            image.sprite = _warriorCard.Details.Foreground;
        }
        
        RectTransform _rect = GetComponent<RectTransform>();
        _rect.sizeDelta = _isAbility ? new Vector2(355,245):new Vector2(245,355);
        Vector3 _rotation = _rect.eulerAngles;
        _rotation.z = _isAbility ? _rotation.z: 180;
        _rect.eulerAngles = _rotation;
    }
    
    
    private void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            if ((Time.time - pointerDownTimer) > LONG_PRESS_THRESHOLD)
            {
                longPressTriggered = true;
                OnLongPress();
            }
        }
    }

    public void OnPointerDown(PointerEventData _eventData)
    {
        isPointerDown = true;
        longPressTriggered = false;
        pointerDownTimer = Time.time;
    }

    public void OnPointerUp(PointerEventData _eventData)
    {
        isPointerDown = false;
    }
    
    private void OnLongPress()
    {
        OnCardHolded?.Invoke(card);
    }
}