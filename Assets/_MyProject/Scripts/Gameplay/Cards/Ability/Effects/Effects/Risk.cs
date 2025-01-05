public class Risk : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        Activate();
        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }

    private void Activate()
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
            _playerThatActivated.DestroyCard(_activatorsLifeForce);
            _otherPlayer.DestroyCard(_otherLifeForce);
            return;
        }
        
        if (_activatorsLifeForce.Health<=0)
        {
            GameplayManager.Instance.EndGame(!_activatorsLifeForce.My);
        }
        else if (_otherLifeForce.Health<=0)
        {
            GameplayManager.Instance.EndGame(!_otherLifeForce.My);
        }
    }
}