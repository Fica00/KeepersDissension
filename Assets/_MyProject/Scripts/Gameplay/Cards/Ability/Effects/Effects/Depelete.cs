using System;

public class Depelete : AbilityEffect
{
    private int amount = 2;

    protected override void ActivateForOwner()
    {
        int _amount = Math.Min(amount,GameplayManager.Instance.OpponentsStrangeMatter());
        GameplayManager.Instance.ChangeOpponentsStrangeMatter(-_amount);
        GameplayManager.Instance.ChangeStrangeMaterInEconomy(_amount);
        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }
}
