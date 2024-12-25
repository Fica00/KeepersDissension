public class ScalerScale : CardSpecialAbility
{
    protected override void Awake()
    {
        base.Awake();
        Card.CardData.CanMoveOnWall = true;
    }
}