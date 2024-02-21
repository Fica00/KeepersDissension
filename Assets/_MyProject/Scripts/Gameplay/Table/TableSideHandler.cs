using UnityEngine;
using UnityEngine.UI;

public class TableSideHandler : MonoBehaviour
{
    [SerializeField] private DeckDisplay minionDeck;
    [SerializeField] private DeckDisplay wallDeck;
    [SerializeField] private DeckDisplay abilityDeck;
    [SerializeField] private Transform cardsHolder;
    [SerializeField] private EconomyDisplay economyDisplay;
    [SerializeField] private TablePlaceHandler activationField;

    public TablePlaceHandler ActivationField => activationField;

    public Transform CardsHolder => cardsHolder;

    private GameplayPlayer player;

    public void Setup(GameplayPlayer _player)
    {
        player = _player;
        minionDeck.Setup(player);
        wallDeck.Setup(player);
        abilityDeck.Setup(player);
        economyDisplay.Setup(player);
    }
}
