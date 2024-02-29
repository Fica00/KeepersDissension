using System;

namespace FirebaseMultiplayer.Room
{
    [Serializable]
    public class RoomPlayer
    {
        public string Id;
        public int FactionId;
        public int MatchesPlayed;
        public DateTime DateCrated;
    }
}