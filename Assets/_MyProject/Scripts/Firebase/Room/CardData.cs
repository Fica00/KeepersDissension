using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class CardData
{
    public CardData()
    {
        PlaceId = -100;
    }
    
    private int placeId;

    public int PlaceRoomOwnerId;
    
    public string Owner;
    public string UniqueId;
    public int CardId;
    public bool IsVoid;
    public bool HasDelivery;
    public CardStats Stats;
    public CardMovementType MovementType;
    public bool HasDied;
    public bool HasSnowWallEffect;
    public bool HasSnowUltimateEffect;
    public int PercentageOfHealthToRecover=100;
    public CardPlace CardPlace = CardPlace.Deck;
    public WarriorAbilityData WarriorAbilityData = new ();

    public bool IsStunned;

    [JsonIgnore] public bool IsMy => Owner == FirebaseManager.Instance.PlayerId;
    
    public int PlaceId
    {
        get => placeId;
        set
        {
            placeId = value;
            Debug.Log("Setting place id: "+placeId);
            if (placeId<=0 || placeId>=64)
            {
                Debug.Log(placeId);
                PlaceRoomOwnerId = placeId;
            }
            else
            {
                PlaceRoomOwnerId = FirebaseManager.Instance.RoomHandler.IsOwner ? placeId : Utils.ConvertPosition(placeId);
                Debug.Log(PlaceRoomOwnerId);
            }
        }
    }
}