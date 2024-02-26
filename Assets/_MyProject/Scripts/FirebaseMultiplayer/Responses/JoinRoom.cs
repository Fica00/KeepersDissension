using System;
using FirebaseMultiplayer.Room;

namespace FirebaseGameplay.Responses
{
    [Serializable]
    public class JoinRoom
    {
        public bool Success;
        public string Message;
        public RoomData Room;
    }
}