public class Return : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        
        GameplayManager.Instance.ChangeStrangeMaterInEconomy(GameplayManager.Instance.MyStrangeMatter());
        GameplayManager.Instance.ChangeStrangeMaterInEconomy(GameplayManager.Instance.OpponentsStrangeMatter());
        
        GameplayManager.Instance.ChangeMyStrangeMatter(-GameplayManager.Instance.MyStrangeMatter());
        GameplayManager.Instance.ChangeOpponentsStrangeMatter(-GameplayManager.Instance.OpponentsStrangeMatter());
        
        RemoveAction();
        OnActivated?.Invoke();
    }
}