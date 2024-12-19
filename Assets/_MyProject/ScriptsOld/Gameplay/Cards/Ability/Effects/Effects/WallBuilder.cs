public class WallBuilder : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
        CardBase _wall = GameplayManager.Instance.MyPlayer.GetCardOfType(CardType.Wall);
        if (_wall==null)
        {
            DialogsManager.Instance.ShowOkDialog("You dont have available wall");
            return;
        }
        GameplayManager.Instance.BuildWall(_wall,0);
    }
}
