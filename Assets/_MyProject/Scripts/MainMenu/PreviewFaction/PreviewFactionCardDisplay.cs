using System;
using UnityEngine;
using UnityEngine.UI;

public class PreviewFactionCardDisplay : MonoBehaviour
{
    public static Action<Card> OnZoomCard;
    [SerializeField] private Image display;
    [SerializeField] private Button flipButton;
    [SerializeField] private Button zoomButton;

    private bool isFlipped;
    private Card showingCard;
    
    private void OnEnable()
    {
        flipButton.onClick.AddListener(Flip);
        zoomButton.onClick.AddListener(Zoom);
    }

    private void OnDisable()
    {
        flipButton.onClick.RemoveListener(Flip);
        zoomButton.onClick.RemoveListener(Zoom);
    }

    private void Flip()
    {
        display.sprite = isFlipped ? showingCard.Details.Background : showingCard.Details.Foreground;
        isFlipped = !isFlipped;
    }

    private void Zoom()
    {
        OnZoomCard?.Invoke(showingCard);   
    }

    public void Setup(Card _card)
    {
        showingCard = _card;
        display.sprite = showingCard.Details.Foreground;
    }
}