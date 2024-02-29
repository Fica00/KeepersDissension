using System;

namespace GameplayActions
{
    [Serializable]
    public class FinishedPlacingStartingCards : GameplayActionBase
    {
        public override ActionType Type => ActionType.FinishedPlacingStartingCards;
    }
}