using System.Linq;
using UnityEngine;

public class Dash : AbilityEffect
{
    private Keeper keeper;
    private GameplayPlayer player;
    private bool applied;
    
    public override void ActivateForOwner()
    {
        player = GameplayManager.Instance.MyPlayer;
        Activate(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        player = GameplayManager.Instance.OpponentPlayer;
        Activate(false);
    }

    private void Activate(bool _isMy)
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My==_isMy);
        player.OnEndedTurn += AddSpeed;
        GameplayManager.OnCardMoved += AddSpeed;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        AddSpeed();
    }

    private void AddSpeed(CardBase _card, int _starting, int _ending)
    {
        if (_card is not Keeper _keeper)
        {
            return;
        }
        if(_keeper != keeper)
        {
            return;
        }

        Debug.Log(111);
        applied = false;
        AddSpeed();
    }

    private void AddSpeed()
    {
        if (applied)
        {
            return;
        }
        applied = true;
        keeper.Speed+=2;
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        player.OnEndedTurn -= AddSpeed;
        GameplayManager.OnCardMoved -= AddSpeed;
        player = null;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (player!=null)
        {
            player.OnEndedTurn -= AddSpeed;
        }
    }
}
