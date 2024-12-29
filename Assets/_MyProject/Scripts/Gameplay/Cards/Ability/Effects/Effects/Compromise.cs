using System.Collections.Generic;

public class Compromise : AbilityEffect
{
    private int amount = 3;

    protected override void ActivateForOwner()
    {
        List<Card> _cards = GameplayManager.Instance.GetAllCards();
        foreach (var _card in _cards)
        {
            _card?.ChangeHealth(amount);
        }
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}
