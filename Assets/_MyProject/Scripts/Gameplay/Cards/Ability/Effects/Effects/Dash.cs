public class Dash : AbilityEffect
{
    public override void ActivateForOwner()
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

    private void AddSpeed(CardBase _card, int _starting, int _ending, bool _)
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
        _effectedKeeper.ChangeSpeed(2);
    }

    public override void CancelEffect()
    {
        if (GetEffectedCards().Count==0)
        {
            return;
        }
        
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= AddSpeed;
        GameplayManager.OnCardMoved -= AddSpeed;
        ManageActiveDisplay(false);
        SetIsActive(false);
    }

    private void OnDisable()
    {
        if (IsActive)
        {
            GameplayManager.Instance.MyPlayer.OnEndedTurn -= AddSpeed;
        }
    }
}
