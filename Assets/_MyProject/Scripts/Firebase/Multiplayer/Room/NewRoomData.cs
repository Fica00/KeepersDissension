using System;
using System.Collections.Generic;

namespace FirebaseMultiplayer.Room
{
    [Serializable]
    public class NewRoomData
    {
        public string Name;
        public string Id;
        public RoomType Type;
        public RoomStatus Status;
        public GameplayState GameplayState;
        public string Owner;
        public List<RoomPlayer> RoomPlayers;
        public BoardData BoardData;
    }
}