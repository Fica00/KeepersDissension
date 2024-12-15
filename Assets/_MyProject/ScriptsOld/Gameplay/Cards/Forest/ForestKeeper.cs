using System;
using System.Collections.Generic;
using System.Linq;

public class ForestKeeper : CardSpecialAbility
{
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
            // GameplayManager.Instance.TellOpponentThatIUsedUltimate();
            CanUseAbility = false;
            Card.ChangeHealth(2);
            GameplayManager.Instance.MyPlayer.Actions--;
        }
    }
}
