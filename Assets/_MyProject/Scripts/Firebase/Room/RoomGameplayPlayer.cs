using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class RoomGameplayPlayer
{
    public string PlayerId;
    public int LootChange;
    public int StrangeMatter;
    public List<CardData> CardsInDeck = new();
    public List<AbilityData> AbilitiesInDeck = new();
    public int AmountOfAbilitiesPlayerCanBuy;

    [JsonIgnore] public bool IsMy => PlayerId == FirebaseManager.Instance.PlayerId;
}
