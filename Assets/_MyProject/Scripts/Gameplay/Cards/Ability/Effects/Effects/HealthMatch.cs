using System.Linq;

public class HealthMatch : AbilityEffect
{
    public override void ActivateForOwner()
    {
        Keeper _myKeeper = FindObjectsOfType<Keeper>().ToList().Find(_element => _element.My);
        Keeper _opponentKeeper = FindObjectsOfType<Keeper>().ToList().Find(_element => !_element.My);
        if (_myKeeper.Health>_opponentKeeper.Health)
        {
            _opponentKeeper.SetHealth(_myKeeper.Health);
        }
        else
        {
            int _difference = _opponentKeeper.Health-_myKeeper.Health;
            _opponentKeeper.ChangeHealth(_difference);
        }
        
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        
    }
}
