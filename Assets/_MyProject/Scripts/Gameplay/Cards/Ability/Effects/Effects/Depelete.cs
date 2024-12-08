using System;

public class Depelete : AbilityEffect
{
    private int amount = 2;

    public override void ActivateForOwner()
    {
        GameplayPlayer _player = GameplayManager.Instance.OpponentPlayer;
        int _amount = Math.Min(amount,_player.StrangeMatter);
        _player.RemoveStrangeMatter(_amount);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}
