using System;

namespace FirebaseMultiplayer.Room
{
    [Serializable]
    public class BoardData
    {
        public int StrangeMaterInEconomy;
        public int StrangeMatterCostChange;
        public string IdOfCardWithResponseAction;


        public int[] LootChanges = { 0, 0 };
        public int[] StrangeMatter = { 0, 0 };
    }
}