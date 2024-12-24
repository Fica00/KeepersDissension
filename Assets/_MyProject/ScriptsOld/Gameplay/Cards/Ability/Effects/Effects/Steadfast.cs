using System.Linq;

public class Steadfast : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        // if (IsActive)
        // {
        //     MoveToActivationField();
        //     RemoveAction();
        //     OnActivated?.Invoke();
        //     return;
        // }
        // MoveToActivationField();
        // var keeper = GameplayManager.Instance.GetMyKeeper();
        // SetIsActive(true);
        // Activate();
        // RemoveAction();
        // OnActivated?.Invoke();
    }

    private void Activate()
    {
        // GameplayManager.OnCardMoved += CheckMovedCard;
        // GameplayManager.OnSwitchedPlace += CheckSwitchedCard;
        // AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void CheckSwitchedCard(CardBase _firstCard, CardBase _secondCard)
    {
        // if(_firstCard != keeper && _secondCard != keeper)
        // {
        //     return;
        // }
        //
        // Deactivate();
    }

    private void CheckMovedCard(CardBase _movedCard, int _startPlace, int _endPLace, bool _)
    {
        // if (_movedCard != keeper)
        // {
        //     return;
        // }
        //
        // Deactivate();
    }

    private void Deactivate()
    {
        // GameplayManager.OnCardMoved -= CheckMovedCard;
        // GameplayManager.OnSwitchedPlace -= CheckSwitchedCard;
        // IsActive = false;
        // IsActiveForOpponent = false;
        // AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    protected override void CancelEffect()
    {
        // if (keeper==null)
        // {
        //     return;
        // }
        // GameplayManager.OnCardMoved -= CheckMovedCard;
        // GameplayManager.OnSwitchedPlace -= CheckSwitchedCard;
        // IsActive = false;
        // IsActiveForOpponent = false;
        // AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
