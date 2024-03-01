using System;

namespace GameplayActions
{
    [Serializable]
    public class GameplayActionBase
    {
        public string Owner;
        public ActionType Type;
    }
}