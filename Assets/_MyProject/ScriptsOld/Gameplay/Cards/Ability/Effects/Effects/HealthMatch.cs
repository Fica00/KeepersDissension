public class HealthMatch : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        Keeper _myKeeper = GameplayManager.Instance.GetMyKeeper();
        Keeper _opponentKeeper = GameplayManager.Instance.GetOpponentKeeper();
        if (_myKeeper == null || _opponentKeeper == null)
        {
            return;
        }
        
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
}
