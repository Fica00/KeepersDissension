using System;
using System.Collections.Generic;

[Serializable]
public class AbilityData
{
    private int placeId = -100;

    public int PlaceRoomOwnerId;
    public string UniqueId;
    public string Owner;
    public int RemainingCooldown;
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

    public int PlaceId
    {
        get => placeId;
        set
        {
            placeId = value;
            PlaceRoomOwnerId = FirebaseManager.Instance.RoomHandler.IsOwner ? placeId : Utils.ConvertPosition(placeId);
        }
    }
}