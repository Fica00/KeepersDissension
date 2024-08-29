public class Tar : AbilityEffect
{
    private int counter;
    public static bool IsActiveForMe;
    public static bool IsActiveForOpponent;

    private void OnEnable()
    {
        IsActiveForOpponent = false;
        IsActiveForMe = false;
    }

    public override void ActivateForOwner()
    {
        if (IsActiveForMe)
        {
            RemoveAction();
            MoveToActivationField();
            OnActivated?.Invoke();
            return;
        }

        IsActiveForMe = true;
        counter = 1;
        GameplayManager.Instance.OpponentPlayer.OnEndedTurn += DisableActiveDisplay;
        MoveToActivationField();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void DisableActiveDisplay()
    {
        if (counter>0)
        {
            counter--;
        }

        IsActiveForMe = false;
        IsActiveForOpponent = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    public override void ActivateForOther()
    {
        if (IsActiveForOpponent)
        {
            return;
        }

        IsActiveForOpponent = true;
        GameplayManager.Instance.MyPlayer.OnEndedTurn += ChangeEffect;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void ChangeEffect()
    {
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= ChangeEffect;
        GameplayManager.Instance.MyPlayer.OnEndedTurn += RemoveEffect;
    }

    private void RemoveEffect()
    {
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= RemoveEffect;        
        IsActiveForMe = false;
        IsActiveForOpponent = false;
    }

    public override void CancelEffect()
    {
        RemoveEffect();
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        IsActiveForMe = false;
        IsActiveForOpponent = false;
    }
}
