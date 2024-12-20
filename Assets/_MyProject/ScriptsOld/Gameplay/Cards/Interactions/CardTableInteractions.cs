using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardTableInteractions : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public static Action<CardBase> OnCardPressed;
    public static Action<TablePlaceHandler> OnPlaceClicked;

    private bool isPointerDown;
    private bool longPressTriggered;
    private float pointerDownTimer;
    private const float LONG_PRESS_THRESHOLD = 0.5f;
    private CardBase CardBase => GetComponentInChildren<CardBase>();
    private TablePlaceHandler TablePlaceHandler => GetComponent<TablePlaceHandler>();

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
        SingleClick();
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

    private void SingleClick()
    {
        OnPlaceClicked?.Invoke(TablePlaceHandler);
        CardBase _cardBase = CardBase;
        if (_cardBase==null)
        {
            CardActionsDisplay.Instance.Close();
            return;
        }
        
        int _placeId = _cardBase.GetComponentInParent<TablePlaceHandler>().Id;
        GameplayPlayer _player = _cardBase.GetIsMy() ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;
        if (GameplayManager.Instance.GameState == GameplayState.UsingSpecialAbility)
        {
            return;
        }
        CardActionsDisplay.Instance.Show(_placeId, _player);
    }

    private void OnLongPress()
    {
        OnCardPressed?.Invoke(CardBase);
    }
}