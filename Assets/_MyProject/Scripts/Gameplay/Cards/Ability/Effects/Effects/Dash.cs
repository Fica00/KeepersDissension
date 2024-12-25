using UnityEngine;

public class Dash : AbilityEffect
{
    [SerializeField] private int speedChange; 
    
    protected override void ActivateForOwner()
    {
        Card _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += AddSpeed;
        GameplayManager.OnCardMoved += AddSpeed;
        SetIsActive(true);
        AddSpeed();
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void AddSpeed(CardBase _card, int _starting, int _ending)
    {
        var _effectedKeeper = GetEffectedCards()[0];
        if (_card is not Keeper _keeper)
        {
            return;
        }
        if(_keeper != _effectedKeeper)
        {
            return;
        }

        SetIsApplied(false);
        AddSpeed();
    }

    private void AddSpeed()
    {
        if (IsApplied)
        {
            return;
        }
        var _effectedKeeper = GetEffectedCards()[0];
        SetIsApplied(true);
        _effectedKeeper.ChangeSpeed(speedChange);
    }

    protected override void CancelEffect()
    {
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= AddSpeed;
        GameplayManager.OnCardMoved -= AddSpeed;
        ManageActiveDisplay(false);
        var _effectedKeeper = GetEffectedCards()[0];
        RemoveEffectedCard(GetEffectedCards()[0].UniqueId);
        if (IsApplied)
        {
            _effectedKeeper.ChangeSpeed(-speedChange);
        }
        SetIsApplied(false);
        SetIsActive(false);
    }
}
