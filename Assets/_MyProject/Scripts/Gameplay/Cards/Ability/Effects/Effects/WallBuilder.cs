public class WallBuilder : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        CardBase _wall = GameplayManager.Instance.GetCardOfTypeNotPlaced(CardType.Wall, true);
        if (_wall==null)
        {
            DialogsManager.Instance.ShowOkDialog("You dont have available wall");
            return;
        }
        
        GameplayManager.Instance.BuildWall(_wall,0, FinishBuild);
    }

    private void FinishBuild()
    {
        OnActivated?.Invoke();
        RemoveAction();
    }
}
