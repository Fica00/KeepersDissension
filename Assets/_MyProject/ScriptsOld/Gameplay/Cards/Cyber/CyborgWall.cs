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

        if (CardBase.GetIsMy())
        {
            GameplayManager.Instance.PlayAudioOnBoth("CyborgWall",CardBase);
        }
        else
        {
            return;
        }

        CardBase _attacker = GameplayManager.Instance.TableHandler.GetPlace(AttackerPlace).GetCard();
        if (GameplayManager.Instance.IsAbilityActiveForMe<Collapse>())
        {
            if (!_attacker.GetIsMy())
            {
                DamageAttacker(AttackerPlace,2);
            }
        }
        else if (GameplayManager.Instance.IsAbilityActiveForOpponent<Collapse>())
        {
            if (_attacker.GetIsMy())
            {
                DamageAttacker(AttackerPlace,2);
            }
        }
        
        if (GameplayManager.Instance.IsAbilityActiveForMe<Immunity>() && _attacker.GetIsMy())
        {
            return;
        }
        if (GameplayManager.Instance.IsAbilityActiveForOpponent<Immunity>() && !_attacker.GetIsMy())
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
