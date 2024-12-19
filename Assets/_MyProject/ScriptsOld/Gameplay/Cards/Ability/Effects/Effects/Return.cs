public class Return : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        GameplayManager.Instance.ChangeMyStrangeMatter(GameplayManager.Instance.MyStrangeMatter());
        GameplayManager.Instance.ChangeOpponentsStrangeMatter(GameplayManager.Instance.OpponentsStrangeMatter());
        RemoveAction();
        OnActivated?.Invoke();
    }
}