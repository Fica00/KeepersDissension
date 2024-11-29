using System;

namespace GameplayActions
{
    [Serializable]
    public class OpponentWantsToDestroyBombWithoutActivatingIt
    {
        public int CardId;
        public bool IsMy;
    }
}