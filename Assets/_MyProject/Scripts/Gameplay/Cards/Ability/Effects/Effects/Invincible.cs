public class Invincible : AbilityEffect
{
    public static bool IsActive;
    public static bool IsActiveForOpponent;
    
    private GameplayPlayer player;

    private int counter;

    private void Awake()
    {
        IsActive = false;
        IsActiveForOpponent = false;
    }

    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        counter = 1;
        player = GameplayManager.Instance.MyPlayer;
        IsActive = true;
        player.OnEndedTurn += Deactivate;
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        if (IsActiveForOpponent)
        {
            return;
        }
        counter = 1;
        player = GameplayManager.Instance.OpponentPlayer;
        IsActiveForOpponent = true;
        player.OnEndedTurn += Deactivate;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void Deactivate()
    {
        if (counter>0)
        {
            counter--;
            return;
        }

        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        player.OnEndedTurn -= Deactivate;
        IsActiveForOpponent = false;
        IsActive = false;
    }

    public override void CancelEffect()
    {
        counter = 0;
        Deactivate();
    }
}
