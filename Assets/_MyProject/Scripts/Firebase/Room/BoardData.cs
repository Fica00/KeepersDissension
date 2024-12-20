using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace FirebaseMultiplayer.Room
{
    [Serializable]
    public class BoardData
    {
        public int StrangeMaterInEconomy;
        public int StrangeMatterCostChange;
        public string IdOfCardWithResponseAction;
        public List<RoomGameplayPlayer> PlayersData = new ();
        public List<AbilityData> AbilitiesInShop = new();
        public List<AbilityData> AvailableAbilities = new();
        public List<CardData> Cards = new();
        public List<AbilityData> Abilities = new();
        
        [JsonIgnore] public RoomGameplayPlayer MyPlayer => PlayersData.First(_player => _player.IsMy);
        [JsonIgnore] public RoomGameplayPlayer OpponentPlayer => PlayersData.First(_player => !_player.IsMy);
        [JsonIgnore] public int AmountOfStartingAbilities => FirebaseManager.Instance.RoomHandler.IsTestingRoom ? 0 : 7;
        [JsonIgnore] public int AmountOfCardsInShop => FirebaseManager.Instance.RoomHandler.IsTestingRoom ? UnityEngine.Resources.LoadAll("Abilities").Length : 3;
        
        public int AbilityCardPrice => 5;

    }
}