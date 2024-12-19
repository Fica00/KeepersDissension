using System;
using System.Collections.Generic;

[Serializable]
public class RoomGameplayPlayer
{
    public string PlayerId;
    public int LootChange;
    public int StrangeMatter;
    public int AmountOfAbilitiesPlayerCanBuy;
    public List<CardData> CardsInDeck = new();
    public List<AbilityData> AbilitiesInDeck = new();

    public bool IsMy => PlayerId == FirebaseManager.Instance.PlayerId;

    public RoomGameplayPlayer()
    {
        if (FirebaseManager.Instance.RoomHandler.IsTestingRoom)
        {
            AmountOfAbilitiesPlayerCanBuy = 1000;
        }
        else
        {
            AmountOfAbilitiesPlayerCanBuy = 7;
        }
    }
}
