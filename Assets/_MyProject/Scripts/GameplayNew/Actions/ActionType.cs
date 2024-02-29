using System;

namespace GameplayActions
{
    [Serializable]
    public enum ActionType
    {
        None = 0,
        PlaceCard = 1,
        Resign=2,
        FinishedPlacingLifeForce =3,
        FinishedPlacingStartingCards=4
    }
}

