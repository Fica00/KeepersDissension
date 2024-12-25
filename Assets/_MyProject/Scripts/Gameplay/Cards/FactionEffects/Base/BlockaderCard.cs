public class BlockaderCard : CardSpecialAbility
{
    public bool CanBlock => Card.CardData.WarriorAbilityData.CanBlock;
    private GameplayPlayer player;
    
    private void Start()
    {
        if (!Player.IsMy)
        {
            return;
        }
        player = Player;
        player.OnStartedTurn += ResetCanBlock;
    }

    private void OnDisable()
    {
        if (player==null)
        {
            return;
        }
        player.OnStartedTurn -= ResetCanBlock;
    }

    private void ResetCanBlock()
    {
         SetCanBlock(true);
    }

    public void MarkAsUsed()
    {
        SetCanBlock(false);
    }

    private void SetCanBlock(bool _value)
    {
        Card.CardData.WarriorAbilityData.CanBlock = _value;
    }
}