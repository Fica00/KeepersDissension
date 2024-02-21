using System.Linq;

public class HealthMatch : AbilityEffect
{
    public override void ActivateForOwner()
    {
        Keeper _myKeeper = FindObjectsOfType<Keeper>().ToList().Find(_element => _element.My);
        Keeper _opponentKeeper = FindObjectsOfType<Keeper>().ToList().Find(_element => !_element.My);
        if (_myKeeper.Stats.Health>_opponentKeeper.Stats.Health)
        {
            _opponentKeeper.Stats.Health = _myKeeper.Stats.Health;
        }
        else
        {
            float _difference = _opponentKeeper.Stats.Health-_myKeeper.Stats.Health;
            _opponentKeeper.Stats.Health -= _difference;
        }
        
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        Keeper _myKeeper = FindObjectsOfType<Keeper>().ToList().Find(_element => _element.My);
        Keeper _opponentKeeper = FindObjectsOfType<Keeper>().ToList().Find(_element => !_element.My);
        if (_opponentKeeper.Stats.Health>_myKeeper.Stats.Health)
        {
            _myKeeper.Stats.Health = _opponentKeeper.Stats.Health;
        }
        else
        {
            float _difference = _myKeeper.Stats.Health - _opponentKeeper.Stats.Health;
            _myKeeper.Stats.Health -= _difference;
        }
    }
}
