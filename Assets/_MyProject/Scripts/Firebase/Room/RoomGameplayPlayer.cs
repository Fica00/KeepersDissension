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

    public bool IsMy => PlayerId == FirebaseManager.Instance.PlayerId;
    public int AmountOfAbilitiesPlayerCanBuy => FirebaseManager.Instance.RoomHandler.IsTestingRoom ? 1000 : 7;
}
