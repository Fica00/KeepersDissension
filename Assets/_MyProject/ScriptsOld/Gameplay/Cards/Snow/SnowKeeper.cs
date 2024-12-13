public class SnowKeeper : CardSpecialAbility
{
    private CardBase effectedCard;

    public override void UseAbility()
    {
        if (!CanUseAbility)
        {
            return;
        }
        if (Player.Actions <= 0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have enough actions");
            return;
        }

        if (!GameplayManager.Instance.MyTurn)
        {
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog("Use keepers ultimate?", Use);
        
        void Use()
        {
            GameplayManager.Instance.TellOpponentThatIUsedUltimate();
            Player.Actions--;
            GameplayManager.Instance.TellOpponentSomething("Opponent used his Ultimate!");
            GameplayManager.Instance.HandleSnowUltimate(false);
            Player.OnStartedTurn += EndEffect;
            CanUseAbility = false;
        }
    }

    private void EndEffect()
    {
        Player.OnStartedTurn -= EndEffect;
        GameplayManager.Instance.HandleSnowUltimate(true);
    }
}
