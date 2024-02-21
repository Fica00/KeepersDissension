using System.Linq;

public class Snipe : AbilityEffect
{
    private GameplayPlayer player;
    private Keeper keeper;
    private int startingRange;
    private float startingDamage;
    
    public override void ActivateForOwner()
    {
        MoveToActivationField();
        player = GameplayManager.Instance.MyPlayer;
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        player = GameplayManager.Instance.OpponentPlayer;
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
    }

    private void Activate()
    {
        player.OnEndedTurn += Deactivate;
        startingDamage = keeper.Stats.Damage;
        startingRange = keeper.Stats.Range;
        keeper.Stats.Damage = 1;
        keeper.Stats.Range = 3;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void Deactivate()
    {
        player.OnEndedTurn -= Deactivate;
        keeper.Stats.Damage = startingDamage;
        keeper.Stats.Range = startingRange;
        keeper = null;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    public override void CancelEffect()
    {
        if (keeper == null)
        {
            return;
        }
        
        Deactivate();
    }
}
