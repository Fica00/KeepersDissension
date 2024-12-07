using System.Linq;

public class Minionized : AbilityEffect
{
    public static bool IsActive;
    private int counter;
    private GameplayPlayer player;
    private int startingHealth;
    private bool hasMyKeeperDied;
    private bool hasOpponentKeeperDied;
    
    private void Awake()
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
        
    }

    private void Activate()
    {
        hasMyKeeperDied = false;
        hasOpponentKeeperDied = false;
        counter = 2;
        IsActive = true;
        Keeper _myKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Keeper _opponentKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        startingHealth = _myKeeper.Health;
        _myKeeper.SetMaxHealth(1);
        _opponentKeeper.SetMaxHealth(1);
        _myKeeper.SetHealth(1);
        _opponentKeeper.SetHealth(1);

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
        Keeper _myKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Keeper _opponentKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        _myKeeper.SetMaxHealth(-1);
        _opponentKeeper.SetMaxHealth(-1);
        _myKeeper.SetHealth(hasMyKeeperDied ? 5 : startingHealth);
        _opponentKeeper.SetHealth(hasOpponentKeeperDied ? 5 : startingHealth);
    }

    private void CheckKeeper(Keeper _keeper)
    {
        Keeper _myKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Keeper _opponentKeeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        
        if (_keeper == _myKeeper)
        {
            hasMyKeeperDied = true;
        }
        else if (_keeper == _opponentKeeper)
        {
            hasOpponentKeeperDied = true;
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
