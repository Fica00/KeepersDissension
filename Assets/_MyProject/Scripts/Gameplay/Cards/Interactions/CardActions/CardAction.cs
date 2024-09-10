using System;
using UnityEngine.Serialization;

[Serializable]
public class CardAction
{
    public int StartingPlaceId;
    public int FirstCardId=-1;
    public int FinishingPlaceId;
    public int SecondCardId=-1;
    public CardActionType Type;
    public int Cost;
    public bool IsMy;
    public bool CanTransferLoot = true;
    public int Damage=-1;
    public bool CanCounter = true;
    public bool GiveLoot = false;
    public bool CanBeBlocked = true;
    public bool ResetSpeed;
    [FormerlySerializedAs("AllowExplosion")] public bool AllowCardEffectOnDeath = true;
    
    public bool CompareTo(CardAction _other)
    {
        if (_other == null)
            return false;

        return StartingPlaceId == _other.StartingPlaceId &&
               FirstCardId == _other.FirstCardId &&
               FinishingPlaceId == _other.FinishingPlaceId &&
               SecondCardId == _other.SecondCardId &&
               Type == _other.Type &&
               Cost == _other.Cost &&
               IsMy == _other.IsMy &&
               CanTransferLoot == _other.CanTransferLoot &&
               Damage == _other.Damage &&
               CanCounter == _other.CanCounter &&
               GiveLoot == _other.GiveLoot &&
               CanBeBlocked == _other.CanBeBlocked &&
               AllowCardEffectOnDeath == _other.AllowCardEffectOnDeath;
    }
}
