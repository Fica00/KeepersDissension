public class SnowWall : WallBase
{
    private int attackerPlace;

    private void OnEnable()
    {
        CardBase.OnGotDestroyed += CheckCard;
    }

    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= CheckCard;
    }

    private void CheckCard(CardBase _card)
    {
        if (CardBase != _card)
        {
            return;
        }

        if (AttackerPlace==-1)
        {
            return;
        }

        attackerPlace = AttackerPlace;

        if (!CardBase.My)
        {
            if (Collapse.IsActive)
            {
                DamageAttacker(attackerPlace);
            }
            return;
        }
        
        if (Immunity.IsActiveForMe || Immunity.IsActiveForOpponent)
        {
            return;
        }

        if (attackerPlace==TablePlaceHandler.Id)
        {
            return;
        }
        
        GameplayManager.Instance.ChangeMovementForCard(attackerPlace, false);
        GameplayManager.Instance.MyPlayer.OnStartedTurn += Unsubscribe;
        
        if (Collapse.IsActive)
        {
            DamageAttacker(attackerPlace);
        }
    }

    private void Unsubscribe()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= Unsubscribe;
        GameplayManager.Instance.ChangeMovementForCard(attackerPlace, true);
    }
}
