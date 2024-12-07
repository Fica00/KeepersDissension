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

        if (!CardBase.GetIsMy())
        {
            return;
        }
        
        CardBase _attacker = GameplayManager.Instance.TableHandler.GetPlace(attackerPlace).GetCard();
        if (Collapse.IsActiveForMe)
        {
            if (!_attacker.GetIsMy())
            {
                DamageAttacker(attackerPlace);
            }
        }
        else if (Collapse.IsActiveForOpponent)
        {
            if (_attacker.GetIsMy())
            {
                DamageAttacker(attackerPlace);
            }
        }

        Debug.Log("Checking effect");
        if (_attacker.GetIsMy())
        {
            Debug.Log(1111);
            if (Immunity.IsActiveForMe)
            {
                Debug.Log(22222);
                return;
            }
            if (Tar.IsActiveForOpponent)
            {
                Debug.Log(33333);
                return;   
            }
        }
        else
        {
                Debug.Log(444444);
            if (Immunity.IsActiveForOpponent)
            {
                Debug.Log(555555);
                return;
            }   
            
            
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
