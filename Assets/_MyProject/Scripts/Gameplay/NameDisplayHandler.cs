using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameDisplayHandler : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private Sprite on;
    [SerializeField] private Sprite off;

    private bool isOn;

    private void OnEnable()
    {
        button.onClick.AddListener(Toggle);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        isOn = !isOn;
        image.sprite = isOn ? on : off;
        var _cards = GetCards();
        foreach (var _card in _cards)
        {
            _card.Display.ManageNameDisplay(isOn);
        }
    }

    private List<Card> GetCards()
    {
        List<Card> _availableCards = new ();
        foreach (var _card in GameplayManager.Instance.GetAllCards())
        {
            if (!_card.IsAttackable())
            {
                continue;
            }

            if (_card is Wall)
            {
                continue;
            }

            if (_card is Marker)
            {
                continue;
            }
            
            _availableCards.Add(_card);
        }

        return _availableCards;
    }
}
