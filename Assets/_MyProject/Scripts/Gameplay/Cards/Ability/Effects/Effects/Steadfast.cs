using System.Linq;

public class Steadfast : AbilityEffect
{
    public static bool IsActive;
    public static bool IsActiveForOpponent;
    private Keeper keeper;

    private void OnEnable()
    {
        IsActive = false;
        IsActiveForOpponent = false;
    }

    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        MoveToActivationField();
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        IsActive = true;
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        if (IsActiveForOpponent)
        {
            return;
        }
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        IsActiveForOpponent = true;
        Activate();
    }

    private void Activate()
    {
        GameplayManager.OnCardMoved += CheckMovedCard;
        GameplayManager.OnSwitchedPlace += CheckSwitchedCard;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void CheckSwitchedCard(CardBase _firstCard, CardBase _secondCard)
    {
        if(_firstCard != keeper && _secondCard != keeper)
        {
            return;
        }

        Deactivate();
    }

    private void CheckMovedCard(CardBase _movedCard, int _startPlace, int _endPLace)
    {
        if (_movedCard != keeper)
        {
            return;
        }

        Deactivate();
    }

    private void Deactivate()
    {
        GameplayManager.OnCardMoved -= CheckMovedCard;
        GameplayManager.OnSwitchedPlace -= CheckSwitchedCard;
        IsActive = false;
        IsActiveForOpponent = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        GameplayManager.OnCardMoved -= CheckMovedCard;
        GameplayManager.OnSwitchedPlace -= CheckSwitchedCard;
        IsActive = false;
        IsActiveForOpponent = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
