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

        Card _attackingCard = GameplayManager.Instance.TableHandler.GetPlace(AttackerPlace).GetCardNoWall();

        if (!CardBase.My)
        {
            if (Collapse.IsActive)
            {
                DamageAttacker(_attackingCard.GetTablePlace().Id);
            }
            return;
        }

        if (Immunity.IsActiveForMe || Immunity.IsActiveForOpponent)
        {
            return;
        }

        if (_card.GetTablePlace().Id == AttackerPlace)
        {
            return;
        }

        GameplayManager.Instance.PushCardBack(AttackerPlace,_card.GetTablePlace().Id, 100);
        if (Collapse.IsActive)
        {
            DamageAttacker(_attackingCard.GetTablePlace().Id);
        }
    }
}
