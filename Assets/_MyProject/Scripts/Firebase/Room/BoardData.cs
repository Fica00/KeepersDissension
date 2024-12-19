using System;
using System.Collections.Generic;
using System.Linq;

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
        
        public RoomGameplayPlayer MyPlayer => PlayersData.First(_player => _player.IsMy);
        public RoomGameplayPlayer OpponentPlayer => PlayersData.First(_player => !_player.IsMy);
        
        public BoardData()
        {
            PlayersData.Add(new RoomGameplayPlayer { PlayerId = FirebaseManager.Instance.PlayerId, LootChange = 0, StrangeMatter = 0 });
            PlayersData.Add(new RoomGameplayPlayer { PlayerId = FirebaseManager.Instance.OpponentId, LootChange = 0, StrangeMatter = 0 });
        }


        public int AmountOfStartingAbilities => FirebaseManager.Instance.RoomHandler.IsTestingRoom ? 0 : 7;
        public int AmountOfCardsInShop => FirebaseManager.Instance.RoomHandler.IsTestingRoom ? UnityEngine.Resources.LoadAll("Abilities").Length : 3;
        
        public int AbilityCardPrice => 5;

    }
}