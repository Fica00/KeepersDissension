using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityShopDisplay : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
{
    public static Action<CardBase> OnCardPressed;
    public static Action<AbilityCard> OnAbilityClicked;
    [SerializeField] private Image foreground;
    
    private bool isPointerDown;
    private bool longPressTriggered;
    private float pointerDownTimer;
    private const float LONG_PRESS_THRESHOLD = 0.5f;

    public AbilityCard Ability { get; private set; }

    public void Setup(AbilityCard _card)
    {
        if (_card==null)
        {
            Empty();
            return;
        }
        Ability = _card;
        foreground.enabled = true;
        foreground.sprite = _card.Details.Foreground;
    }

    public void OnPointerClick(PointerEventData _eventData)
    {
        OnAbilityClicked?.Invoke(Ability);
    }


    public void Empty()
    {
        Ability = null;
        foreground.enabled = false;
    }
    
    public void OnPointerDown(PointerEventData _eventData)
    {
        isPointerDown = true;
        longPressTriggered = false;
        pointerDownTimer = Time.time;
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
    
    private void OnLongPress()
    {
        if (Ability==null)
        {
            return;
        }
        OnCardPressed?.Invoke(Ability);
    }
    
    public void OnPointerUp(PointerEventData _eventData)
    {
        isPointerDown = false;
    }
}
