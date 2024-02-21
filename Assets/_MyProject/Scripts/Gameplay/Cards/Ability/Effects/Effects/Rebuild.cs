public class Rebuild : AbilityEffect
{
    public override void ActivateForOwner()
    {
        Activate();
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        Activate();
    }

    private void Activate()
    {
        if (GameplayManager.Instance.ShouldIPlaceStartingWall)
        {
            GameplayManager.Instance.PlaceStartingWall();
        }
    }
}
