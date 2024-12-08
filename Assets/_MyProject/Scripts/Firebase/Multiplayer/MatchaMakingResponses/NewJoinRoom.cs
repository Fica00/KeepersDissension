using System;
using FirebaseMultiplayer.Room;

namespace FirebaseGameplay.Responses
{
    [Serializable]

    public class NewJoinRoom
    {
        public bool Success;
        public string Message;
        public RoomType Type;
        public string Name;
        public RoomData Room;
    }
}