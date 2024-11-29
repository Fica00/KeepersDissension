using System;

namespace GameplayActions
{
    [Serializable]
    public class OpponentSaidToChangeSprite
    {
        public int CardPlace;
        public int CardId;
        public int SpriteId;
        public bool ShowPlaceAnimation;
    }
}