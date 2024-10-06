public class HighStakes : AbilityEffect
{
    public static bool IsActive;

    private void Awake()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            RemoveAction();
            MoveToActivationField();
            OnActivated?.Invoke();
            return;
        }
        RemoveAction();
        MoveToActivationField();
        OnActivated?.Invoke();
        IsActive = true;
        GameplayManager.Instance.EndTurn();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        if (IsActive)
        {
            return;
        }
        IsActive = true;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        IsActive = false;
    }
}
