using System;

namespace FirebaseMultiplayer.Room
{
    [Serializable]
    public enum RoomStatus
    {
        SearchingForOpponent = 0,
        MatchedUp = 1,
        Playing = 2
    }
}