using System.Linq;

public class WoundedRunner : AbilityEffect
{
    private Keeper keeper;
    private GameplayPlayer player;
    private bool isActive;
    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        player = GameplayManager.Instance.MyPlayer;
        isActive = true;
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void Activate()
    {
        player.OnEndedTurn += AddSpeed;
        GameplayManager.OnCardAttacked += AddSpeed;
        GameplayManager.OnCardMoved += AddSpeed;
        GameplayManager.OnPlacedCard += AddSpeed;
        GameplayManager.OnSwitchedPlace += AddSpeed;
        AddSpeed();
    }

    private void AddSpeed()
    {
        if (keeper.Stats.Health==1)
        {
            keeper.Speed = 3;
            AbilityCard.ActiveDisplay.SetActive(true);
        }
        else
        {
            AbilityCard.ActiveDisplay.SetActive(false);
        }
    }

    private void Update()
    {
        if (isActive)
        {
            if (keeper.Stats.Health!=1)
            {
                CancelEffect();
            }
        }
    }

    private void OnDisable()
    {
        CancelEffect();
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }

        isActive = false;
        keeper.Speed = 0;
        player.OnEndedTurn -= AddSpeed;
        GameplayManager.OnCardAttacked -= AddSpeed;
        GameplayManager.OnCardMoved -= AddSpeed;
        GameplayManager.OnPlacedCard -= AddSpeed;
        GameplayManager.OnSwitchedPlace -= AddSpeed;
        keeper = null;
        AbilityCard.ActiveDisplay.SetActive(false);
    }

    private void AddSpeed(CardBase _arg1, CardBase _arg2)
    {
        AddSpeed();
    }

    private void AddSpeed(CardBase _arg1, CardBase _arg2, int _arg3)
    {
        AddSpeed();
    }

    private void AddSpeed(CardBase _arg1, int _arg2, int _arg3)
    {
        AddSpeed();
    }

    private void AddSpeed(CardBase _obj)
    {
        AddSpeed();
    }
}