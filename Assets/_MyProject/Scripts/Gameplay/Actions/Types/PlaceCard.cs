using System;

namespace GameplayActions
{
    [Serializable]
    public class PlaceCard
    {
        public int CardId;
        public int PositionId;
        public bool DontCheckIfPlayerHasIt;
    }
}