using UnityEngine;

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
            Debug.Log("1111111");
            return;
        }

        if (_card.UniqueId != Card.UniqueId)
        {
            Debug.Log("2222222");
            return;
        }

        Debug.Log("Detected that Card holding bomber died");
        GameplayManager.Instance.BombExploded(_card.GetTablePlace().Id, false);
    }
}