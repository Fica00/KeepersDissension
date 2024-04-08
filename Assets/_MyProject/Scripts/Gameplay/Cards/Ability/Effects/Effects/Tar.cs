using System.Linq;

public class Tar : AbilityEffect
{
    private int counter;
    public static bool IsActive;
    private bool isActiveForOpponent;

    private void OnEnable()
    {
        isActiveForOpponent = false;
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

        IsActive = false;
        isActiveForOpponent = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    public override void ActivateForOther()
    {
        if (isActiveForOpponent)
        {
            return;
        }

        isActiveForOpponent = true;
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
        IsActive = false;
        isActiveForOpponent = false;
    }

    public override void CancelEffect()
    {
        RemoveEffect();
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        IsActive = false;
        isActiveForOpponent = false;
    }
}
