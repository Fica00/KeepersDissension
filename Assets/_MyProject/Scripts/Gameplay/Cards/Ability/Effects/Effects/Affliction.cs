using System;
using System.Linq;

public class Affliction : AbilityEffect
{
    public override void ActivateForOwner()
    {
        Guardian _opponentsGuardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My == false);
        RemoveAction();
        int _damage = Convert.ToInt32(_opponentsGuardian.Health / 2);
        _damage = Math.Clamp(_damage, 1, int.MaxValue);
        _opponentsGuardian.ChangeHealth(-_damage);
        OnActivated?.Invoke();
    }
}
