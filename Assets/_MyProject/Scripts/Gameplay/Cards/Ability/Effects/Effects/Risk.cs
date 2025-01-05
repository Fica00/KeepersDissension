public class Risk : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        bool _didGameEnd = Activate();
        if (_didGameEnd)
        {
            return;
        }
        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }

    private bool Activate()
    {
        GameplayPlayer _playerThatActivated = GameplayManager.Instance.MyPlayer;
        GameplayPlayer _otherPlayer = GameplayManager.Instance.OpponentPlayer;

        var _activatorsLifeForce = GameplayManager.Instance.GetMyLifeForce();
        var _otherLifeForce = GameplayManager.Instance.GetOpponentsLifeForce();

        _activatorsLifeForce.ChangeHealth(-10);
        _otherLifeForce.ChangeHealth(-5);

        if (_activatorsLifeForce.Health<=0 && _otherLifeForce.Health<=0)
        {
            GameplayManager.Instance.UnchainGuardian(0,false);
            GameplayManager.Instance.UnchainOpponentsGuardian();
            _playerThatActivated.DestroyCard(_activatorsLifeForce);
            _otherPlayer.DestroyCard(_otherLifeForce);
            return false;
        }
        
        if (_activatorsLifeForce.Health<=0)
        {
            GameplayManager.Instance.EndGame(!_activatorsLifeForce.My);
            return true;
        }

        if (_otherLifeForce.Health<=0)
        {
            GameplayManager.Instance.EndGame(!_otherLifeForce.My);
            return true;
        }

        return false;
    }
}