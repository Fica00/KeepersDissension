using UnityEngine;

public class CyborgWall : WallBase
{
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

        if (CardBase.My)
        {
            GameplayManager.Instance.PlayAudioOnBoth("CyborgWall",CardBase);
        }
        else
        {
            return;
        }

        CardBase _attacker = GameplayManager.Instance.TableHandler.GetPlace(AttackerPlace).GetCard();
        if (Collapse.IsActiveForMe)
        {
            if (!_attacker.My)
            {
                DamageAttacker(AttackerPlace);
            }
        }
        else if (Collapse.IsActiveForOpponent)
        {
            if (_attacker.My)
            {
                DamageAttacker(AttackerPlace);
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

        if (_card.GetTablePlace().Id == AttackerPlace)
        {
            return;
        }

        int _distance = GameplayManager.Instance.TableHandler.DistanceBetweenPlaces(GameplayManager.Instance.TableHandler.GetPlace(AttackerPlace), CardBase.GetTablePlace());
        if (_distance>1)
        {
            return;
        }

        GameplayManager.Instance.PushCardBack(AttackerPlace,_card.GetTablePlace().Id, 100);
    }
}
