using System;

namespace GameplayActions
{
    [Serializable]
    public class OpponentBoughtMinion
    {
        public int CardId;
        public int Cost; 
        public int PositionId;
        public bool PlaceMinion;
    }
}