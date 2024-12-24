public class Repel : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
        GameplayManager.OnCardAttacked += CheckAttackingCard;
        ManageActiveDisplay(true);
    }

    private void CheckAttackingCard(CardBase _attackingCard, CardBase _defendingCard, int _damage)
    {
        if (_attackingCard is not Keeper)
        {
            return;
        }

        if (!_attackingCard.GetIsMy())
        {
            return;
        }
        
        GameplayManager.Instance.PushCardBack(_defendingCard.GetTablePlace().Id,_attackingCard.GetTablePlace().Id);
    }

    protected override void CancelEffect()
    {
        GameplayManager.OnCardAttacked -= CheckAttackingCard;
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}