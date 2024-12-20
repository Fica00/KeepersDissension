using System;
using System.Collections;
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

        StartCoroutine(ShowFirstCard());
    }

    private void OnDisable()
    {
        if (player==null)
        {
            return;
        }
        
        StopAllCoroutines();
    }

    public void OnPointerClick(PointerEventData _eventData)
    {
        if (GameplayManager.Instance.GameState is not (GameplayState.Playing or GameplayState.Waiting or GameplayState.AttackResponse))
        {
            return;
        }
        OnDeckClicked?.Invoke(player,type);
    }

    private IEnumerator ShowFirstCard()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(5);
            if (player==null)
            {
                continue;
            }
            
            Card _card = GameplayManager.Instance.GetCardOfType(type,player.IsMy);
            display.sprite = _card==null ? player.FactionSo.CardBackground : _card.Details.Foreground;
        }
    }
}
