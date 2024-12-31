public class ScoutStrafe : CardSpecialAbility
{
    private GameplayPlayer player;
    
    private void Start()
    {
        GameplayPlayer _player = GetPlayer();
        if (!_player.IsMy)
        {
            return;
        }

        player = _player;
        player.OnStartedTurn += Activate;
    }
    
    private void OnDisable()
    {
        if (player==null)
        {
            return;
        }
        
        player.OnStartedTurn -= Activate;
    }

    private void Activate()
    {
        player.OnStartedTurn -= Activate;
        player = null;
        Card.ChangeMovementType(CardMovementType.EightDirections);
    }
}
