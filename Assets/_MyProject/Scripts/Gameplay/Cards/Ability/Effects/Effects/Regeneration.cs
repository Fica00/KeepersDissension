using System.Linq;

public class Regeneration : AbilityEffect
{
    private int amountToHeal=1;
    private GameplayPlayer player;
    private Keeper keeper;
    private bool isActive;
    
    public override void ActivateForOwner()
    {
        if (isActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        isActive = true;
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        MoveToActivationField();
        player = GameplayManager.Instance.MyPlayer;
        player.OnStartedTurn += ActivateAndUnsubscribe;
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
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
        player = GameplayManager.Instance.OpponentPlayer;
        player.OnStartedTurn += ActivateAndUnsubscribe;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void ActivateAndUnsubscribe()
    {
        player.OnStartedTurn -= ActivateAndUnsubscribe;
        if (isActive)
        {
            Activate();
        }
        isActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    private void Activate()
    {
        keeper.Heal(amountToHeal);
    }

    public override void CancelEffect()
    {
        isActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
