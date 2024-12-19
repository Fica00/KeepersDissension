using System;
using System.Collections.Generic;

[Serializable]
public class RoomGameplayPlayer
{
    public string PlayerId;
    public int LootChange;
    public int StrangeMatter;
    public List<CardData> CardsInDeck = new();
    public List<AbilityData> AbilitiesInDeck = new();
    public int amountOfAbilitiesPlayerCanBuy = 7;

    public bool IsMy => PlayerId == FirebaseManager.Instance.PlayerId;

    public int AmountOfAbilitiesPlayerCanBuy
    {
        get => amountOfAbilitiesPlayerCanBuy;
        set => amountOfAbilitiesPlayerCanBuy = value;
    }
}
