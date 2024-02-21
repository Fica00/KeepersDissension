public class Resourceful : AbilityEffect
{
    private bool isActive;
    
    public override void ActivateForOwner()
    {
        if (isActive)
        {
            return;
        }

        isActive = true;
        GameplayManager.Instance.StrangeMatterCostChange += 2;
        GameplayManager.Instance.MyPlayer.OnStartedTurn += RemoveEffect;
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        if (isActive)
        {
            return;
        }

        isActive = true;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        GameplayManager.Instance.OpponentPlayer.OnStartedTurn += DisableActive;
    }

    private void DisableActive()
    {
        isActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        GameplayManager.Instance.OpponentPlayer.OnStartedTurn -= DisableActive;
    }

    private void RemoveEffect()
    {
        isActive = false;
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= RemoveEffect;
        GameplayManager.Instance.StrangeMatterCostChange -= 2;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    public override void CancelEffect()
    {
        RemoveEffect();
    }
}
