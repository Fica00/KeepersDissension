public class BlockaderCard : CardSpecialAbility
{
    public bool CanBlock => Card.CardData.WarriorAbilityData.CanBlock;
    private GameplayPlayer player;
    
    private void Start()
    {
        if (!GetPlayer().IsMy)
        {
            return;
        }
        player = GetPlayer();
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

    public void Activate()
    {
        ResetCanBlock();
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