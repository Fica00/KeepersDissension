using System;
using System.Collections.Generic;
using GameplayActions;

namespace FirebaseMultiplayer.Room
{
    [Serializable]
    public class RoomData
    {
        public string Name;
        public string Id;
        public RoomType Type;
        public RoomStatus Status;
        public string Owner;
        public List<RoomPlayer> RoomPlayers;
        public Dictionary<string,GameplayActionBase> Actions = new();
        public GameplayData GameplayData = new ();
    }
}

