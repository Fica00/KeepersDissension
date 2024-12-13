using System;

namespace GameplayActions
{
    [Serializable]
    public class AddAbilityCardToPlayer
    {
        public bool IsMyPlayer;
        public int AbilityId;
    }
}