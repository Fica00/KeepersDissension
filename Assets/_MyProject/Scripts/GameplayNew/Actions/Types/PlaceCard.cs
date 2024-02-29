namespace GameplayActions
{
    public class PlaceCard : GameplayActionBase
    {
        public override ActionType Type => ActionType.PlaceCard;
        public int CardId;
        public int PositionId;
        public bool DontCheckIfPlayerHasIt;
    }
}