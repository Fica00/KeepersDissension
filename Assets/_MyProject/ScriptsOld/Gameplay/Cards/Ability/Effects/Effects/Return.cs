public class Return : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        var _myPlayer = GameplayManager.Instance.MyPlayer;
        var _opponentPlayer = GameplayManager.Instance.OpponentPlayer;
        
        _myPlayer.RemoveStrangeMatter(_myPlayer.StrangeMatter);
        _opponentPlayer.RemoveStrangeMatter(_opponentPlayer.StrangeMatter);
        RemoveAction();
        OnActivated?.Invoke();
    }
}