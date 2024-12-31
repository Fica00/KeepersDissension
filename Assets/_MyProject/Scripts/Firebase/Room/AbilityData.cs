using System;
using System.Collections.Generic;

[Serializable]
public class AbilityData
{
    public string UniqueId;
    public string Owner;
    public bool IsVetoed;
    public int RemainingCooldown;
    public int PlaceId = -100;
    public int Cooldown;
    public bool IsActive;
    public AbilityCardType Type;
    public List<string> EffectedCards = new();
    public CardPlace CardPlace = CardPlace.Deck;
    public int CardId;
    public AbilityColor Color;
    public bool CanBeGivenToPlayer;
    
    //helpers
    public bool IsLightUp;
    public bool IsApplied;
    public int Multiplayer;
    public bool CanExecuteThisTurn;
    public int StartingRange;
    public int StartingDamage;
    public int StartingHealth;
    public bool HasMyRequiredCardDied;
    public bool HasOpponentsRequiredCardDied;
    public int OpponentsStartingHealth;
}
