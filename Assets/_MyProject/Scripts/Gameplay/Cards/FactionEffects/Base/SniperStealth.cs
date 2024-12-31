public class SniperStealth : CardSpecialAbility
{
    private GameplayPlayer player;

    private void Start()
    {
        if (!GetPlayer().IsMy)
        {
            return;
        }
        
        player = GetPlayer();
        player.OnStartedTurn += ResetAbilities;
    }

    private void OnDisable()
    {
        if (player==null)
        {
            return; 
        }
        
        player.OnStartedTurn -= ResetAbilities;
    }

    private void ResetAbilities()
    {
        SetCanUseAbility(true);
    }

    public override void UseAbility()
    {
        if (Card.CardData.WarriorAbilityData.StealthData.StealthPlaceId != -1)
        {
            DialogsManager.Instance.ShowOkDialog("You can have maximum of 3 bombs");
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog("Are you sure that you want to use stealth ability?", YesUseStealth);
    }

    private void YesUseStealth()
    {
        GameplayManager.Instance.SelectPlaceForSpecialAbility(TablePlaceHandler.Id, 1, PlaceLookFor.Empty,
            CardMovementType.FourDirections, true, LookForCardOwner.My, GoStealth);
    }

    private void GoStealth(int _placeId)
    {
        
    }
}