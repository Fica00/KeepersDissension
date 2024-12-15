
public class Retaliate : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetRemainingCooldown(2);
        SetIsActive(true);
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
        GameplayManager.OnCardAttacked += CheckAttackingCard;
        GameplayManager.Instance.MyPlayer.OnEndedTurn += LowerCounter;
        ManageActiveDisplay(true);
    }

    private void LowerCounter()
    {
        if (RemainingCooldown>0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            return;
        }

        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        SetIsActive(false);
        GameplayManager.OnCardAttacked -= CheckAttackingCard;
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= LowerCounter;
        ManageActiveDisplay(false);
    }

    private void CheckAttackingCard(CardBase _attackingCard, CardBase _defendingCard, int _damage)
    {
        if (!IsActive)
        {
            return;
        }
        
        var _keeper = GetEffectedCards()[0];

        if (_attackingCard == _keeper)
        {
            return;
        }

        CardAction _returnDamage = new CardAction
        {
            StartingPlaceId = _attackingCard.GetTablePlace().Id,
            FirstCardId = ((Card)_attackingCard).UniqueId,
            FinishingPlaceId = _attackingCard.GetTablePlace().Id,
            SecondCardId = ((Card)_attackingCard).UniqueId,
            Type = CardActionType.Attack,
            Cost = 0,
            CanTransferLoot = false,
            Damage = _damage,
            CanCounter = false,
            GiveLoot = false
        };
        
        GameplayManager.Instance.ExecuteCardAction(_returnDamage);
    }

    protected override void CancelEffect()
    {
        SetRemainingCooldown(0);
        LowerCounter();
    }
}
