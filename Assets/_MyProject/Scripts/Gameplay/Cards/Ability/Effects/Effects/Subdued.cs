public class Subdued : AbilityEffect
{
    public static bool IsActive;

    private void OnEnable()
    {
        IsActive = false;
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOwner()
    {
        MoveToActivationField();
        IsActive = true;
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
