using System;
using FirebaseMultiplayer.Room;
using UnityEngine;

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
        public string GameVersion => Application.version;
    }
}