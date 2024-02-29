using System;

namespace GameplayActions
{
    [Serializable]
    public class Resign : GameplayActionBase
    {
        public override ActionType Type => ActionType.Resign;
    }
}