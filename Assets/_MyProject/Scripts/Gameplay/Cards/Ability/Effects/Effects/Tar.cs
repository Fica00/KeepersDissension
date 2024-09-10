public class Tar : AbilityEffect
{
    public static bool IsActiveForMe;
    public static bool IsActiveForOpponent;
    private GameplayPlayer player;

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

        player = GameplayManager.Instance.OpponentPlayer;
        IsActiveForMe = true;
        player.OnEndedTurn += ChangeEffect;
        MoveToActivationField();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        if (IsActiveForOpponent)
        {
            return;
        }

        player = GameplayManager.Instance.MyPlayer;
        IsActiveForOpponent = true;
        player.OnEndedTurn += ChangeEffect;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void ChangeEffect()
    {
        player.OnEndedTurn -= ChangeEffect;
        player.OnEndedTurn += RemoveEffect;
    }

    private void RemoveEffect()
    {
        player.OnEndedTurn -= RemoveEffect;        
        IsActiveForMe = false;
        IsActiveForOpponent = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    public override void CancelEffect()
    {
        if (!(IsActiveForMe&& IsActiveForOpponent))
        {
            return;
        }
        
        RemoveEffect();
    }
}
