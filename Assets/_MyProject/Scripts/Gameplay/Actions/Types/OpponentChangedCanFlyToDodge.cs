using System;

namespace GameplayActions
{
    [Serializable]
    public class OpponentChangedCanFlyToDodge
    {
        public int CardId;
        public bool Status;
        public bool IsEffectedCardMy;
    }
}