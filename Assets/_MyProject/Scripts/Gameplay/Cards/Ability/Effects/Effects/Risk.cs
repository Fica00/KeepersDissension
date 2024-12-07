using System.Linq;

public class Risk : AbilityEffect
{
    public static bool IsActive;

    private void Awake()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        Activate(true);
        RemoveAction();
        OnActivated?.Invoke();
        MoveToActivationField();
    }

    public override void ActivateForOther()
    {
        Activate(false);
    }

    private void Activate(bool _didIActivate)
    {
        IsActive = true;
        GameplayPlayer _playerThatActivated =
            _didIActivate ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;
        
        GameplayPlayer _otherPlayer =
            _didIActivate ? GameplayManager.Instance.OpponentPlayer : GameplayManager.Instance.MyPlayer;

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
        
        IsActive = false;
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
