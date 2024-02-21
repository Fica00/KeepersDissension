using System.Linq;

public class Minionized : AbilityEffect
{
    public static bool IsActive;
    private int counter;
    private GameplayPlayer player;
    private float startingHealth;
    private bool hasDied;
    
    private void OnEnable()
    {
        IsActive = false;
    }

    public override void ActivateForOther()
    {
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        player = GameplayManager.Instance.OpponentPlayer;
        MoveToActivationField();
        Activate();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            return;
        }
        player = GameplayManager.Instance.MyPlayer;
        Activate();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void Activate()
    {
        hasDied = false;
        counter = 2;
        IsActive = true;
        Keeper _myKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Keeper _opponentKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        startingHealth = _myKeeper.Stats.Health;
        _myKeeper.Stats.Health = 1;
        _opponentKeeper.Stats.Health = 1;

        player.OnEndedTurn += LowerCounter;
        GameplayManager.OnKeeperDied += CheckKeeper;
        RemoveAction();
    }

    private void LowerCounter()
    {
        if (counter>0)
        {
            counter--;
            return;
        }

        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        IsActive = false;
        player.OnEndedTurn -= LowerCounter;
        GameplayManager.OnKeeperDied -= CheckKeeper;
        Keeper _keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        if (!hasDied)
        {
            _keeper.Stats.Health = startingHealth;
        }
        else
        {
            _keeper.Stats.Health = 5;
        }
        
        GameplayManager.Instance.UpdateHealth(_keeper.Details.Id,true,(int)_keeper.Stats.Health);
    }

    private void CheckKeeper(Keeper _keeper)
    {
        if (_keeper == FindObjectsOfType<Keeper>().ToList().Find(_myKeeper => _myKeeper.My))
        {
            hasDied = true;
        }
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        counter = 0;
        LowerCounter();
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
