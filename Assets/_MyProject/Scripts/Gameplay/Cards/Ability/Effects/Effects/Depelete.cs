using System;

public class Depelete : AbilityEffect
{
    private int amount = 2;

    public override void ActivateForOwner()
    {
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        GameplayPlayer _player = GameplayManager.Instance.MyPlayer;
        int _amount = Math.Min(amount,_player.StrangeMatter);
        _player.RemoveStrangeMatter(_amount);
    }
}
