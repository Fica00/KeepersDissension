public class ScalerScale : CardSpecialAbility
{
    private void Start()
    {
        Card.CanMoveOnWall = true;
    }

    private void OnDisable()
    {
        Card.CanMoveOnWall = false;
    }
}