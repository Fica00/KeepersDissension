public class ScalerScale : CardSpecialAbility
{
    protected override void Awake()
    {
        Card.CardData.CanMoveOnWall = true;
    }
}