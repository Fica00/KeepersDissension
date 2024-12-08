using UnityEngine;

public class BountyHunter : AbilityEffect
{
    [SerializeField] private int amount;
    
    public override void ActivateForOwner()
    {
        RemoveAction();
        SetIsActive(true);
        ManageActiveDisplay(true);
        OnActivated?.Invoke();
    }
    
    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        SetIsActive(false);
        ManageActiveDisplay(false);
    }
}