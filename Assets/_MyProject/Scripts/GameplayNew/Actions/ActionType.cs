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
        FinishedPlacingStartingCards=4,
        AddAbilityToPlayer=5,
        AddAbilityToShop=6,
        ExecuteCardAction=7,
        OpponentTookLoot=8,
        GiveLoot=9,
        OpponentFinishedAttackResponse=10,
        OpponentEndedTurn=11,
        OpponentUpdatedHisStrangeMatter=12,
        OpponentUpdatedStrangeMatterInReserve=13,
        ForceUpdateOpponentAction=14,
        OpponentBoughtMinion=15,
        OpponentBuiltWall=16,
        OpponentUnchainedGuardian = 17,
        OpponentsBlockaderPassive = 18,
        TellOpponentSomething = 19,
        ChangeOwner=20,
        MyCardDiedInOpponentPossession=21,
        OpponentChangedMovement=22,
        OpponentChangedCanFlyToDodge=23,
        OpponentWantsToTryAndDestroyMarkers=24,
        OpponentMarkedBomb=25,
        OpponentFinishedReductionAction=26,
        OpponentSaidThatBombExploded=27,
        OpponentUsedSnowUltimate=28,
        OpponentActivatedAbility=29,
        OpponentBoughtAbilityFromShop=30,
        OpponentBoughtAbilityFromHand=31,
        
    }
}