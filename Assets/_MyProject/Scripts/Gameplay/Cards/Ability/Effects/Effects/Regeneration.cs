using System;
using System.Linq;

public class Regeneration : AbilityEffect
{
    private int amountToHeal=1;
    private GameplayPlayer player;
    private Keeper keeper;
    private bool isActive;
    private int counter=-1;
    
    public override void ActivateForOwner()
    {
        if (counter==-1)
        {
            counter = 1;
        }
        else
        {
            counter++;
        }
        if (isActive)
        {
            MoveToActivationField();
            RemoveAction();
            Activate();
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
        if (counter==-1)
        {
            counter = 1;
        }
        else
        {
            counter++;
        }
        if (isActive)
        {
            Activate();
            return;
        }
        player = GameplayManager.Instance.OpponentPlayer;
        player.OnStartedTurn += ActivateAndUnsubscribe;
        isActive = true;
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void ActivateAndUnsubscribe()
    {
        player.OnStartedTurn -= ActivateAndUnsubscribe;
        if (isActive)
        {
            Activate(counter);
        }

        counter = 0;
        isActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    private void Activate(int _amount=-1)
    {
        keeper.Heal(_amount == -1 ? amountToHeal : _amount);
    }

    public override void CancelEffect()
    {
        isActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
