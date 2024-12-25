public class BlockaderCard : CardSpecialAbility
{
    public bool CanBlock => Card.CardData.WarriorAbilityData.CanBlock;
    
    private void Start()
    {
        Player.OnStartedTurn += ResetCanBlock;
    }

    private void OnDisable()
    {
        Player.OnStartedTurn -= ResetCanBlock;
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