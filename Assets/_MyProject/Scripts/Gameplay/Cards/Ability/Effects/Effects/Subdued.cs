public class Subdued : AbilityEffect
{
    public static bool IsActive;

    private void Awake()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        MoveToActivationField();
        IsActive = true;
        RemoveAction();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        IsActive = true;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        IsActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
