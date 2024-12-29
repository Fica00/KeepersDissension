using System;

public class Affliction : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        Card _opponentsGuardian = GameplayManager.Instance.GetOpponentGuardian();
        int _damage = Convert.ToInt32(_opponentsGuardian.Health / 2);
        _damage = Math.Clamp(_damage, 1, int.MaxValue);
        _opponentsGuardian.ChangeHealth(-_damage);
        OnActivated?.Invoke();
        RemoveAction();
    }
}