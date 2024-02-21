public class WallBuilder : AbilityEffect
{
    public override void ActivateForOwner()
    {
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
        CardBase _wall = GameplayManager.Instance.MyPlayer.GetCard(CardType.Wall);
        if (_wall==null)
        {
            UIManager.Instance.ShowOkDialog("You dont have available wall");
            return;
        }
        GameplayManager.Instance.BuildWall(_wall,0);
    }
}
