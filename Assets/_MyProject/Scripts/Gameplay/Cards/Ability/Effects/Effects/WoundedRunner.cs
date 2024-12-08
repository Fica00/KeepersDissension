using System.Linq;
using UnityEngine;

public class WoundedRunner : AbilityEffect
{
    // private Keeper keeper;
    // private GameplayPlayer player;
    // private bool isActive;
    // private bool applied;
    public override void ActivateForOwner()
    {
        // keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        // player = GameplayManager.Instance.MyPlayer;
        // isActive = true;
        // Activate();
        // RemoveAction();
        // OnActivated?.Invoke();
    }

    private void Activate()
    {
        // player.OnEndedTurn += AddSpeed;
        // GameplayManager.OnCardAttacked += AddSpeed;
        // GameplayManager.OnCardMoved += AddSpeed;
        // GameplayManager.OnPlacedCard += AddSpeed;
        // GameplayManager.OnSwitchedPlace += AddSpeed;
        // keeper.UpdatedHealth += RemoveSpeed;
        // Debug.Log("Subscribed");
        // AddSpeed();
    }

    private void OnDisable()
    {
        // CancelEffect();
    }

    public override void CancelEffect()
    {
        // if (keeper==null)
        // {
        //     return;
        // }
        //
        // Debug.Log("canceled effect");
        // isActive = false;
        // keeper.SetSpeed(0);
        // player.OnEndedTurn -= AddSpeed;
        // GameplayManager.OnCardAttacked -= AddSpeed;
        // GameplayManager.OnCardMoved -= AddSpeed;
        // GameplayManager.OnPlacedCard -= AddSpeed;
        // GameplayManager.OnSwitchedPlace -= AddSpeed;
        // keeper.UpdatedHealth -= RemoveSpeed;
        // keeper = null;
        // AbilityCard.ActiveDisplay.SetActive(false);
    }

    private void AddSpeed(CardBase _arg1, CardBase _arg2)
    {
        // if (_arg1==keeper || _arg2==keeper)
        // {
        //     applied = false;
        // }
        // AddSpeed();
    }

    private void AddSpeed(CardBase _arg1, CardBase _arg2, int _arg3)
    {
        // if (_arg1==keeper || _arg2==keeper)
        // {
        //     applied = false;
        // }
        // AddSpeed();
    }

    private void AddSpeed(CardBase _arg1, int _arg2, int _arg3, bool _)
    {
        // Debug.Log("Card moved: "+_arg1);
        // if (_arg1==keeper)
        // {
        //     applied = false;
        // }
        // AddSpeed();
    }

    private void AddSpeed(CardBase _arg1)
    {
        // if (_arg1==keeper)
        // {
        //     applied = false;
        // }
        // AddSpeed();
    }
    
    private void AddSpeed()
    {
        // if (!isActive)
        // {
        //     return;
        // }
        // if (applied)
        // {
        //     return;
        // }
        //
        // if (keeper.Health==1)
        // {
        //     applied = true;
        //     keeper.ChangeSpeed(3);
        //     AbilityCard.ActiveDisplay.SetActive(true);
        // }
        // else
        // {
        //     AbilityCard.ActiveDisplay.SetActive(false);
        // }
    }

    private void RemoveSpeed()
    {
        // if (!isActive)
        // {
        //     return;
        // }
        //
        // if (!applied)
        // {
        //     return;
        // }
        //
        // keeper.ChangeSpeed(-3);
        // applied = false;
    }
}