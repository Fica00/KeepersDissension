public class Repel : AbilityEffect
{
    public override void ActivateForOwner()
    {
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
        GameplayManager.OnCardAttacked += CheckAttackingCard;
        ManageActiveDisplay(true);
    }

    private void OnDisable()
    {
        if (!IsActive)
        {
            return;
        }
        
        GameplayManager.OnCardAttacked -= CheckAttackingCard; 
    }

    private void CheckAttackingCard(CardBase _attackingCard, CardBase _defendingCard, int _damage)
    {
        if (_attackingCard is not Keeper)
        {
            return;
        }
        
        GameplayManager.Instance.PushCardBack(_defendingCard.GetTablePlace().Id,_attackingCard.GetTablePlace().Id);
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        GameplayManager.OnCardAttacked -= CheckAttackingCard;
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}