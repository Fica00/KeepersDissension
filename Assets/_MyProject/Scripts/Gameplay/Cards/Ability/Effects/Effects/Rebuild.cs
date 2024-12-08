public class Rebuild : AbilityEffect
{
    public override void ActivateForOwner()
    {
        GameplayManager.Instance.PlaceStartingWall();
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }
}