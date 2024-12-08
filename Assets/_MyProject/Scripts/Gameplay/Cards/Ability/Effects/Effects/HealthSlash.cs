using System;

public class HealthSlash : AbilityEffect
{
    public override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetOpponentKeeper();
        int _newHealth = (int)Math.Floor(_keeper.Health / 2.0);
        int _damage = _keeper.Health - _newHealth;

        if (_damage<1)
        {
            _damage = 1;
        }
        
        _keeper.ChangeHealth(-_damage);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}