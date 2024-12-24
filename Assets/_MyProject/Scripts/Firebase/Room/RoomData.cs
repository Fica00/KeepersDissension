using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace FirebaseMultiplayer.Room
{
    [Serializable]
    public class RoomData
    {
        public string Name;
        public string Id;
        public RoomType Type;
        public RoomStatus Status;
        public GameplayState GameplayState;
        public GameplaySubState GameplaySubState;
        public int CurrentPlayerTurn = 1;
        public bool HasGameEnded;
        public string Winner;
        public string Owner;
        public List<RoomPlayer> RoomPlayers;
        public BoardData BoardData = new ();

        [JsonIgnore] public bool IsMyTurn => FirebaseManager.Instance.RoomHandler.IsOwner ? CurrentPlayerTurn == 1 : CurrentPlayerTurn == 2;
        public string GameVersion  => Application.version;
    }
}