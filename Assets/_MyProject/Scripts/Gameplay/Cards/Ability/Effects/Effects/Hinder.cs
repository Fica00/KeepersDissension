public class Hinder : AbilityEffect
{
    public static bool IsActive;

    private void OnEnable()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        RemoveAction();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        IsActive = true;
    }

    public override void CancelEffect()
    {
        IsActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
