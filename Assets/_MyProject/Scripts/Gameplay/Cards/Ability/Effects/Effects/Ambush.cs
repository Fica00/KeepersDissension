using UnityEngine;

public class Ambush : AbilityEffect
{
    public static bool IsActiveForMe;
    public static bool IsActiveForOpponent;

    private void Awake()
    {
        IsActiveForMe = false;
        IsActiveForOpponent = false;
    }

    public override void ActivateForOwner()
    {
        IsActiveForMe = true;
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        IsActiveForOpponent = true;
    }

    public override void CancelEffect()
    {
        Debug.Log("----- Resting");
        IsActiveForMe = false;
        IsActiveForOpponent = false;
    }
}
