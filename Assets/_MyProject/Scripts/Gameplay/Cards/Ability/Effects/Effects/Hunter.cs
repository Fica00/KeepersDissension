public class Hunter : AbilityEffect
{
    public static bool IsActive;
    public static bool IsActiveForOpponent;

    private void OnEnable()
    {
        IsActive = false;
        IsActiveForOpponent = false;
    }

    public override void ActivateForOwner()
    {
        IsActive = true;
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        IsActiveForOpponent = true;
    }

    public override void CancelEffect()
    {
        IsActiveForOpponent = false;
    }
}
