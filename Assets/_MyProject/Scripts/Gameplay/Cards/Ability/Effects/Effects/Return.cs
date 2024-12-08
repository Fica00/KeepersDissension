public class Return : AbilityEffect
{
    public override void ActivateForOwner()
    {   
        MoveToActivationField();
        GameplayManager.Instance.WhiteStrangeMatter.AmountInEconomy += GameplayManager.Instance.OpponentPlayer.StrangeMatter;
        GameplayManager.Instance.MyPlayer.RemoveStrangeMatter(GameplayManager.Instance.MyPlayer.StrangeMatter);
        RemoveAction();
        OnActivated?.Invoke();
    }
}
