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

        if (!CardBase.GetIsMy())
        {
            return;
        }
        
        CardBase _attacker = GameplayManager.Instance.TableHandler.GetPlace(attackerPlace).GetCard();
        if (GameplayManager.Instance.IsAbilityActiveForMe<Collapse>())
        {
            if (!_attacker.GetIsMy())
            {
                DamageAttacker(attackerPlace);
            }
        }
        else if (GameplayManager.Instance.IsAbilityActiveForOpponent<Collapse>())
        {
            if (_attacker.GetIsMy())
            {
                DamageAttacker(attackerPlace);
            }
        }

        if (_attacker.GetIsMy())
        {
            if (GameplayManager.Instance.IsAbilityActiveForMe<Immunity>())
            {
                return;
            }
            if (GameplayManager.Instance.IsAbilityActiveForMe<Tar>())
            {
                return;   
            }
        }
        else
        {
            if (GameplayManager.Instance.IsAbilityActiveForOpponent<Immunity>())
            {
                return;
            }   
        }

        if (attackerPlace==TablePlaceHandler.Id)
        {
            return;
        }

        // GameplayManager.Instance.ChangeMovementForCard(attackerPlace, false);
        GameplayManager.Instance.MyPlayer.OnStartedTurn += Unsubscribe;
    }

    private void Unsubscribe()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= Unsubscribe;
        // GameplayManager.Instance.ChangeMovementForCard(attackerPlace, true);
    }
}
