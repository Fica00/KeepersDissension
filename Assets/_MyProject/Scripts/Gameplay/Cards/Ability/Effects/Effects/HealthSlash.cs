using System;
using System.Linq;

public class HealthSlash : AbilityEffect
{
    public override void ActivateForOwner()
    {
        ApplyEffect(FindObjectsOfType<Keeper>().ToList().Find(_keeper=>!_keeper.My));
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        
    }

    private void ApplyEffect(Keeper _keeper)
    {
        int _newHealth = (int)Math.Floor(_keeper.Health / 2.0);
        int _damage = _keeper.Health - _newHealth;

        if (_damage<1)
        {
            _damage = 1;
        }
        
        _keeper.ChangeHealth(-_damage);
    }
}
