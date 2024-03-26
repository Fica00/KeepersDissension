using UnityEngine;

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
            return;
        }
        
        CardBase _attacker = GameplayManager.Instance.TableHandler.GetPlace(attackerPlace).GetCard();
        if (Collapse.IsActiveForMe)
        {
            if (!_attacker.My)
            {
                DamageAttacker(attackerPlace);
            }
        }
        else if (Collapse.IsActiveForOpponent)
        {
            if (_attacker.My)
            {
                DamageAttacker(attackerPlace);
            }
        }
        
        if (Immunity.IsActiveForMe && _attacker.My)
        {
            return;
        }
        if (Immunity.IsActiveForOpponent && !_attacker.My)
        {
            return;
        }
        

        if (attackerPlace==TablePlaceHandler.Id)
        {
            return;
        }
        
        GameplayManager.Instance.ChangeMovementForCard(attackerPlace, false);
        GameplayManager.Instance.MyPlayer.OnStartedTurn += Unsubscribe;
    }

    private void Unsubscribe()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= Unsubscribe;
        GameplayManager.Instance.ChangeMovementForCard(attackerPlace, true);
    }
}
