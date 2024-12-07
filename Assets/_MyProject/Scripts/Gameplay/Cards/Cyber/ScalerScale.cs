public class ScalerScale : CardSpecialAbility
{
    private void Start()
    {
        Card.SetCanMoveOnWall(true);
    }

    private void OnDisable()
    {
        Card.SetCanMoveOnWall(false);
    }
}