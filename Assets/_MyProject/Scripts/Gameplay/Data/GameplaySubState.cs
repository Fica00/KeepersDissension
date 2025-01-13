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
    Player1ResponseAction=7,
    Player2ResponseAction=8,
    Playing=9,
    Player1DeliveryReposition=10,
    Player2DeliveryReposition=11,
    Player1UseComrade=12,
    Player2UseComrade=13,
    Player1UseReduction=14,
    Player2UseReduction=15,
}
