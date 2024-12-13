public class SlowDown : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        ClearList();
        SetRemainingCooldown(2);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += ClearList;
        GameplayManager.OnCardMoved += AddCard;
        GameplayManager.Instance.MyPlayer.OnEndedTurn += LowerCounter;
        ManageActiveDisplay(true);
        
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void AddCard(CardBase _cardThatMoved, int _arg2, int _arg3, bool _)
    {
        AddEffectedCard((_cardThatMoved as Card)?.UniqueId);
    }

    private void ClearList()
    {
        ClearEffectedCards();
    }

    public bool CanMoveCard(Card _card)
    {
        if (!IsActive)
        {
            return true;
        }

        return !GetEffectedCards().Find(_cardSavedCard =>_cardSavedCard.UniqueId == _card.UniqueId);
    }

    protected override void CancelEffect()
    {
        SetRemainingCooldown(0);
        LowerCounter();
    }
    
    private void LowerCounter()
    {
        if (RemainingCooldown>0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            return;
        }
        
        ManageActiveDisplay(false);
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= ClearList;
        GameplayManager.OnCardMoved -= AddCard;
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= LowerCounter;
        SetIsActive(false);
        ClearEffectedCards();
    }
}