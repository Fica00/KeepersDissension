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
    [FormerlySerializedAs("AllowExplosion")] public bool AllowCardEffectOnDeath = true;
}
