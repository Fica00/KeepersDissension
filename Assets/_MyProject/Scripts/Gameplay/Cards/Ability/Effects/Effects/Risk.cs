using System.Linq;

public class Risk : AbilityEffect
{
    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            return;
        }
        
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
        MoveToActivationField();
    }

    private void Activate()
    {
        GameplayPlayer _playerThatActivated = GameplayManager.Instance.MyPlayer;
        GameplayPlayer _otherPlayer = GameplayManager.Instance.OpponentPlayer;

        var _activatorsLifeForce = FindObjectsOfType<LifeForce>().ToList().Find(_lifeForce => _lifeForce.My == _playerThatActivated.IsMy);
        var _otherLifeForce = FindObjectsOfType<LifeForce>().ToList().Find(_lifeForce => _lifeForce.My == _otherPlayer.IsMy);

        _activatorsLifeForce.ChangeHealth(-10);
        _otherLifeForce.ChangeHealth(-5);

        if (_activatorsLifeForce.Health<=0 && _otherLifeForce.Health<=0)
        {
            GameplayManager.Instance.UnchainGuardian();
            _playerThatActivated.DestroyCard(_activatorsLifeForce);
            _otherPlayer.DestroyCard(_otherLifeForce);
            return;
        }
        
        if (_activatorsLifeForce.Health<=0)
        {
            GameplayManager.Instance.StopGame(!_activatorsLifeForce.My);
        }
        else if (_otherLifeForce.Health<=0)
        {
            GameplayManager.Instance.StopGame(!_otherLifeForce.My);
        }
    }
}
