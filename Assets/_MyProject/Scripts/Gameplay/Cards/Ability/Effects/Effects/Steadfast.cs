public class Steadfast : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        SetIsActive(true);
        GameplayManager.OnCardMoved += CheckMovedCard;
        GameplayManager.OnSwitchedPlace += CheckSwitchedCard;
        OnActivated?.Invoke();
        RemoveAction();
    }

    private void CheckSwitchedCard(CardBase _firstCard, CardBase _secondCard)
    {
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        if(_firstCard != _keeper && _secondCard != _keeper)
        {
            return;
        }
        
        Deactivate();
    }

    private void CheckMovedCard(CardBase _cardThatMoved, int _arg2, int _arg3)
    {
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        if (_cardThatMoved != _keeper)
        {
            return;   
        }
        
        Deactivate();
    }

    private void Deactivate()
    {
        GameplayManager.OnCardMoved -= CheckMovedCard;
        GameplayManager.OnSwitchedPlace -= CheckSwitchedCard;
        SetIsActive(false);
        ManageActiveDisplay(false);
    }

    protected override void CancelEffect()
    {
       Deactivate();
    }
}
