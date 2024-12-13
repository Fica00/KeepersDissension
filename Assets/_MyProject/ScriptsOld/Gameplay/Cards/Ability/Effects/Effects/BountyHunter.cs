using UnityEngine;

public class BountyHunter : AbilityEffect
{
    [SerializeField] private int amount;

    protected override void ActivateForOwner()
    {
        GameplayManager.Instance.ChangeLootAmountForMe(amount);
        SetIsActive(true);
        ManageActiveDisplay(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        GameplayManager.Instance.ChangeLootAmountForMe(-amount);
        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}