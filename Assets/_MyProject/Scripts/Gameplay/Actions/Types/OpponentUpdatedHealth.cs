using System;

namespace GameplayActions
{
    [Serializable]
    public class OpponentUpdatedHealth
    {
        public int CardId;
        public bool Status;
        public int Health;
    }
}