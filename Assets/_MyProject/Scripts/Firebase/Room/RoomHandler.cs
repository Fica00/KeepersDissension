using System;
using System.Collections.Generic;
using Firebase.Database;
using FirebaseGameplay.Responses;
using Newtonsoft.Json;
using UnityEngine;

namespace FirebaseMultiplayer.Room
{
    [Serializable]
    public class RoomHandler
    {
        public static Action<RoomPlayer> OnPlayerJoined;
        public static Action<RoomPlayer> OnPlayerLeft;
        public static Action<BoardData> OnBoardUpdated;
        public static Action OnILeftRoom;

        private DatabaseReference database;

        private const string LEAVE_ROOM = "https://leaveroom-e3mmrpwoya-uc.a.run.app";
        private const string JOIN_ROOM = "https://joinroom-e3mmrpwoya-uc.a.run.app";
        private const string CREATE_ROOM = "https://createroom-e3mmrpwoya-uc.a.run.app";

        private string roomsPath;
        private RoomData roomData;
        private string localPlayerId;

        public bool IsOwner => roomData.Owner == localPlayerId;
        public RoomData RoomData => roomData;
        public BoardData BoardData => roomData.BoardData;
        private string RoomPath => roomsPath + "/" + roomData.Id;

        public bool IsTestingRoom => RoomData.Type == RoomType.Debug;

        public void Init(DatabaseReference _database, string _roomsPath)
        {
            database = _database;
            roomsPath = _roomsPath;
        }

        public void SetLocalPlayerId(string _localPlayerId)
        {
            localPlayerId = _localPlayerId;
        }

        public void LeaveRoom()
        {
            RoomPlayer _myPlayer = default;
            foreach (var _player in roomData.RoomPlayers)
            {
                if (_player.Id == localPlayerId)
                {
                    _myPlayer = _player;
                    break;
                }
            }

            if (_myPlayer == default)
            {
                return;
            }

            string _data = JsonConvert.SerializeObject(new { roomId = roomData.Id, playerId = localPlayerId });
            WebRequests.Instance.Post(LEAVE_ROOM, _data, _ =>
            {
                roomData.RoomPlayers.Remove(_myPlayer);
                UnsubscribeFromRoom();
                OnILeftRoom?.Invoke();
            }, _ => { Debug.Log(_); });
        }

        public void SubscribeToRoom(RoomData _roomData)
        {
            roomData = _roomData;
            database.Child(RoomPath).ValueChanged += RoomUpdated;
        }

        private void UnsubscribeFromRoom()
        {
            if (roomData == null)
            {
                return;
            }

            Debug.Log("Unsubscribed");
            database.Child(RoomPath).ValueChanged -= RoomUpdated;
            roomData = null;
        }

        private void RoomUpdated(object _sender, ValueChangedEventArgs _args)
        {
            if (_args == null)
            {
                return;
            }

            if (!_args.Snapshot.Exists)
            {
                return;
            }

            if (roomData == null)
            {
                return;
            }

            RoomData _data = JsonConvert.DeserializeObject<RoomData>(_args.Snapshot.GetRawJsonValue());
            if (_data == null)
            {
                return;
            }

            if (_data.RoomPlayers == null)
            {
                return;
            }

            CheckIfPlayerJoined(_data);
            CheckIfPlayerLeft(_data);
            roomData = _data;
        }

        private void CheckIfPlayerJoined(RoomData _data)
        {
            foreach (var _player in _data.RoomPlayers)
            {
                if (DoesPlayerExist(_player.Id, roomData.RoomPlayers))
                {
                    continue;
                }

                OnPlayerJoined?.Invoke(_player);
            }
        }

        private void CheckIfPlayerLeft(RoomData _data)
        {
            foreach (var _player in roomData.RoomPlayers)
            {
                if (DoesPlayerExist(_player.Id, _data.RoomPlayers))
                {
                    continue;
                }

                OnPlayerLeft?.Invoke(_player);
            }
        }

        private bool DoesPlayerExist(string _playerId, List<RoomPlayer> _players)
        {
            foreach (var _oldPlayer in _players)
            {
                if (_playerId == _oldPlayer.Id)
                {
                    return true;
                }
            }

            return false;
        }

        public void JoinRoom(RoomPlayer _playerData, RoomGameplayPlayer _gamePlayerData, RoomType _type, Action<NewJoinRoom> _callBack, string _name = 
                default)
        {
            string _postData = JsonConvert.SerializeObject(new { PlayerData = JsonConvert.SerializeObject(new {playerData = _playerData, gamePlayerData = _gamePlayerData }), Type = _type, Name = _name });

            WebRequests.Instance.Post(JOIN_ROOM, _postData, _response =>
            {
                NewJoinRoom _responseData = JsonConvert.DeserializeObject<NewJoinRoom>(_response);
                roomData = _responseData.Room;
                _responseData.Name = _name;
                _responseData.Type = _type;
                _callBack?.Invoke(_responseData);
            }, _response =>
            {
                Debug.Log(_response);
                NewJoinRoom _data = JsonConvert.DeserializeObject<NewJoinRoom>(_response);
                _callBack?.Invoke(_data);
            }, _includeHeader: false);
        }

        public void CreateRoom(RoomData _roomData, Action<NewCreateRoom> _callBack)
        {
            string _postData = JsonConvert.SerializeObject(new { _roomData.Id, JsonData = JsonConvert.SerializeObject(_roomData) });

            WebRequests.Instance.Post(CREATE_ROOM, _postData, _response =>
            {
                roomData = _roomData;
                _callBack?.Invoke(JsonConvert.DeserializeObject<NewCreateRoom>(_response));
            }, _response => { _callBack?.Invoke(JsonConvert.DeserializeObject<NewCreateRoom>(_response)); });
        }

        public RoomPlayer GetOpponent()
        {
            foreach (var _player in roomData.RoomPlayers)
            {
                if (_player.Id == localPlayerId)
                {
                    continue;
                }

                return _player;
            }

            throw new Exception("Can't find opponent");
        }
        
        public RoomPlayer GetMyPlayer()
        {
            foreach (var _player in roomData.RoomPlayers)
            {
                if (_player.Id == localPlayerId)
                {
                    return _player;
                }

            }

            throw new Exception("Can't find opponent");
        }
    }
}