public class Rebuild : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        GameplayManager.Instance.PlaceStartingWall();
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}