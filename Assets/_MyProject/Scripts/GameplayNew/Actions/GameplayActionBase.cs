using System;

namespace GameplayActions
{
    [Serializable]
    public class GameplayActionBase
    {
        public string Owner;
        public virtual ActionType Type => ActionType.None;
    }
}