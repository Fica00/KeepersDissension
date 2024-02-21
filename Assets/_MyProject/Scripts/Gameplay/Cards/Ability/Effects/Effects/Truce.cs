public class Truce : AbilityEffect
{
    private GameplayPlayer player;
    public static bool IsActive = false;
    private static int duration = 0;

    private void OnEnable()
    {
        IsActive = false;
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
        
        MoveToActivationField();
        duration = 3;
        player = GameplayManager.Instance.MyPlayer;
        IsActive = true;
        player.OnEndedTurn += CheckForEnd;
        RemoveAction();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        if (IsActive)
        {
            return;
        }
        duration = 3;
        player = GameplayManager.Instance.OpponentPlayer;
        IsActive = true;
        player.OnEndedTurn += CheckForEnd;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void CheckForEnd()
    {
        if (duration==0)
        {
            return;
        }
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        duration--;

        if (duration==0)
        {
            IsActive = false;
            player.OnEndedTurn -= CheckForEnd;
        }
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        duration = 1;
        CheckForEnd();
    }
}
