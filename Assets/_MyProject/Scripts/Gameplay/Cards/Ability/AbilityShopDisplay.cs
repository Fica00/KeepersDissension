using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityShopDisplay : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler
{
    public static Action<AbilityData> OnCardPressed;
    public static Action<AbilityData> OnAbilityClicked;
    [SerializeField] private Image foreground;
    
    private bool isPointerDown;
    private bool longPressTriggered;
    private float pointerDownTimer;
    private const float LONG_PRESS_THRESHOLD = 0.5f;

    public AbilityData Ability { get; private set; }

    public void Setup(AbilityData _card)
    {
        Debug.Log("Showing");
        if (_card==null)
        {
            Debug.Log(11111111111);
            Empty();
            return;
        }

        Debug.Log(22222222);
        gameObject.name = CardsManager.Instance.GetAbilityName(_card.CardId);
        Ability = _card;
        foreground.enabled = true;
        foreground.sprite = CardsManager.Instance.GetAbilityImage(_card.CardId);
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
