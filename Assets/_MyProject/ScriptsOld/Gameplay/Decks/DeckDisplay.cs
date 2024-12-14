using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckDisplay : MonoBehaviour, IPointerClickHandler
{
    public static Action<GameplayPlayer,CardType> OnDeckClicked;
    [SerializeField] private CardType type;
    [SerializeField] private Image display;

    private GameplayPlayer player;
    
    public void Setup(GameplayPlayer _player)
    {
        player = _player;
        if (type== CardType.Ability)
        {
            return;
        }

        ShowFirstCard();
        
        player.UpdatedDeck += ShowFirstCard;
        GameplayManager.FinishedSetup += ShowFirstCard;
    }

    private void OnDisable()
    {
        if (player==null)
        {
            return;
        }
        player.UpdatedDeck -= ShowFirstCard;
        GameplayManager.FinishedSetup -= ShowFirstCard;
    }

    public void OnPointerClick(PointerEventData _eventData)
    {
        if (GameplayManager.Instance.GameState is not (GameplayState.Playing or GameplayState.Waiting or GameplayState.AttackResponse))
        {
            return;
        }
        OnDeckClicked?.Invoke(player,type);
    }

    private void ShowFirstCard()
    {
        if (player==null)
        {
            return;
        }
        Card _card = player.PeakNextCard(type);
        display.sprite = _card==null ? player.FactionSo.CardBackground : _card.Details.Foreground;
    }
}
