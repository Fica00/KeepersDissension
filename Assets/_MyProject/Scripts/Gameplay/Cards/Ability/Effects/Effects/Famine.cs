public class Famine : AbilityEffect
{
    public static bool IsActive;
    private int counter;
    private GameplayPlayer gameplayPlayer;

    private void Awake()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        gameplayPlayer = GameplayManager.Instance.MyPlayer;
        MoveToActivationField();
        Activate();
        RemoveAction();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        gameplayPlayer = GameplayManager.Instance.OpponentPlayer;
        Activate();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void Activate()
    {
        if (IsActive)
        {
            return;
        }
        counter = 1;
        IsActive = true;
        gameplayPlayer.OnEndedTurn += Deactivate;
    }

    private void Deactivate()
    {
        if (counter>0)
        {
            counter--;
            return;
        }
        
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        gameplayPlayer.OnEndedTurn -= Deactivate;
        IsActive = false;
    }

    public override void CancelEffect()
    {
        counter = 0;
        Deactivate();
    }
}
