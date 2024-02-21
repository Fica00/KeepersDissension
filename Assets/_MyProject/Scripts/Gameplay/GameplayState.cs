using System;

[Serializable]
public enum GameplayState
{
    WaitingForPlayersToLoad=0,
    SettingUpTable=1,
    Playing=2,
    Waiting=3,
    AttackResponse=4,
    WaitingForAttackResponse = 5,
    PlacingKeeper=6,
    WaitingForOpponentToPlaceKeeper=7,
    BuyingMinion=8,
    BuildingWall = 9,
    UsingSpecialAbility=10,
    SelectingCardFromTable=11
}
