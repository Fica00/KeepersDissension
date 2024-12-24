using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHandInteractions : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public static Action<CardBase> OnCardClicked;
    public static Action<CardBase> OnCardPressed;

    private CardBase cardBase;
    private bool isPointerDown;
    private bool longPressTriggered;
    private float pointerDownTimer;
    private const float LONG_PRESS_THRESHOLD = 0.5f;
    
    public void Setup(CardBase _cardBase)
    {
        cardBase = _cardBase;
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

    public void OnPointerClick(PointerEventData _eventData)
    {
        OnCardClicked?.Invoke(cardBase);
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
        OnCardPressed?.Invoke(cardBase);
    }
}
