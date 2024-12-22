using System;

[Serializable]
public enum GameplaySubState
{
    None = 0,
    Player1PlacingLifeForce = 1,
    Player2PlacingLifeForce = 2,
    FinishedPlacingStartingLifeForce = 3,
    Player1SelectMinions = 4,
    Player2SelectMinions = 5,
    FinishedSelectingMinions = 6,
}
