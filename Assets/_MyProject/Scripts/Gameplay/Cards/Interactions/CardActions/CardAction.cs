using System;

[Serializable]
public class CardAction
{
    public int StartingPlaceId;
    public string FirstCardId=string.Empty;
    public int FinishingPlaceId;
    public string SecondCardId=string.Empty;
    public CardActionType Type;
    public int Cost;
    public int Damage=-1;

    public bool CompareTo(CardAction _cardAction)
    {
        return _cardAction.StartingPlaceId == StartingPlaceId && _cardAction.FirstCardId == FirstCardId &&
               _cardAction.FinishingPlaceId == FinishingPlaceId && _cardAction.SecondCardId == SecondCardId && _cardAction.Type == Type;
    }
}
