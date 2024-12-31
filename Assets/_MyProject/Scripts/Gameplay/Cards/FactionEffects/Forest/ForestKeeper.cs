public class ForestKeeper : CardSpecialAbility
{
    private GameplayPlayer player;
    
    private void Start()
    {
        if (!GetPlayer().IsMy)
        {
            return;
        }
        
        player = GetPlayer();
        player.OnStartedTurn += EnableAbility;
    }

    private void EnableAbility()
    {
        player.OnStartedTurn -= EnableAbility;
        SetCanUseAbility(true);
    }
    
    public override void UseAbility()
    {
        DialogsManager.Instance.ShowYesNoDialog("Use keepers ultimate?", Use);
    }
    
    private void Use()
    {
        SetCanUseAbility(false);
        Card.ChangeHealth(2);
        GameplayManager.Instance.MyPlayer.Actions--;
        if (GameplayManager.Instance.MyPlayer.Actions>0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }
}
