using System;

namespace GameplayActions
{
    [Serializable]
    public class OpponentSaidToUseStealth
    {
        public int CardId;
        public int StealthFromPlace;
        public int PlaceMinionsFrom;
    }
}