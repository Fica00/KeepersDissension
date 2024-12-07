using System;

[Serializable]
public class CardData
{
    public string Owner;
    public string CardUniqueId;
    public int CardId;
    public bool IsVoid;
    public bool CanFlyToDodgeAttack;
    public bool CanMoveOnWall;
    public CardStats Stats;
    public CardMovementType MovementType;
    public bool HasDied;
    public bool CanBeUsed = true;
    public bool CanMove = true;
}
