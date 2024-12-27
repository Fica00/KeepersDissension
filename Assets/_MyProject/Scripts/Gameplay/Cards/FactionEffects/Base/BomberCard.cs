public class BomberCard : CardSpecialAbility
{
    private void OnEnable()
    {
        CardBase.OnGotDestroyed += CheckCard;
    }

    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= CheckCard;
    }

    private void CheckCard(CardBase _cardBase)
    {
        if (_cardBase is not Card _card)
        {
            return;
        }

        if (_card.UniqueId != Card.UniqueId)
        {
            return;
        }

        GameplayManager.Instance.BombExploded(_card.GetTablePlace().Id, false);
    }
}