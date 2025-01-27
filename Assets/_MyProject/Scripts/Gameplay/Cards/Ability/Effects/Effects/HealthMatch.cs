public class HealthMatch : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        Keeper _myKeeper = GameplayManager.Instance.GetMyKeeper();
        Keeper _opponentKeeper = GameplayManager.Instance.GetOpponentKeeper();
        
        _opponentKeeper.SetHealth(_myKeeper.Health);
        
        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }
}