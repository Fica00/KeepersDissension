public class Explode : AbilityEffect
{
    public static bool IsActive;
    
    private void Awake()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        IsActive = true;
        RemoveAction();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        IsActive = false;
    }
}
