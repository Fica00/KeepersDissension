using System;
using Newtonsoft.Json;

[Serializable]
public class CardData
{
    public string Owner;
    public string UniqueId;
    public int CardId;
    public bool IsVoid;
    public bool HasDelivery;
    public CardStats Stats;
    public CardMovementType MovementType;
    public bool HasDied;
    public bool CanBeUsed = true;
    public int PercentageOfHealthToRecover=100;
    public int PlaceId = -100;
    public CardPlace CardPlace = CardPlace.Deck;
    public WarriorAbilityData WarriorAbilityData = new ();

    public bool IsStunned;

    [JsonIgnore] public bool IsMy => Owner == FirebaseManager.Instance.PlayerId;
}