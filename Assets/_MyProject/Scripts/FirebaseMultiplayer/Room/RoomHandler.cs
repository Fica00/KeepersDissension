using System;
using System.Collections.Generic;
using Firebase.Database;
using FirebaseGameplay.Responses;
using GameplayActions;
using Newtonsoft.Json;
using UnityEngine;

namespace FirebaseMultiplayer.Room
{
    public class RoomHandler
    {
        public static Action<RoomPlayer> OnPlayerJoined;
        public static Action<RoomPlayer> OnPlayerLeft;
        public static Action<ActionData> OnNewAction;
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
        private string RoomPath => roomsPath + "/" + roomData.Id;
        private int actionCounter;

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
                if (_player.Id==localPlayerId)
                {
                    _myPlayer = _player;
                    break;
                }
            }

            if (_myPlayer==default)
            {
                return;
            }

            string _data = JsonConvert.SerializeObject(new { roomId = roomData.Id, playerId = localPlayerId});
            WebRequests.Instance.Post(LEAVE_ROOM,_data , _ =>
            {
                roomData.RoomPlayers.Remove(_myPlayer);
                UnsubscribeFromRoom();
                OnILeftRoom?.Invoke();
            }, _ =>
            {
                Debug.Log(_);
            });
        }

        public void SubscribeToRoom()
        {
            database.Child(RoomPath).ValueChanged += RoomUpdated;
            actionCounter = 0;
        }

        public void UnsubscribeFromRoom()
        {
            if (roomData == null)
            {
                return;
            }

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

            RoomData _newData = JsonConvert.DeserializeObject<RoomData>(_args.Snapshot.GetRawJsonValue());
            if (_newData == null)
            {
                return;
            }

            CheckIfPlayerJoined(_newData);
            CheckIfPlayerLeft(_newData);
            CheckForNewAction(_newData);
            roomData = _newData;
        }

        private void CheckIfPlayerJoined(RoomData _newData)
        {
            foreach (var _player in _newData.RoomPlayers)
            {
                if (DoesPlayerExist(_player.Id, roomData.RoomPlayers))
                {
                    continue;
                }
                
                OnPlayerJoined?.Invoke(_player);
            }
        }

        private void CheckIfPlayerLeft(RoomData _newData)
        {
            foreach (var _player in roomData.RoomPlayers)
            {
                if (DoesPlayerExist(_player.Id,_newData.RoomPlayers))
                {
                    continue;
                }
                
                OnPlayerLeft?.Invoke(_player);
            }
        }

        private void CheckForNewAction(RoomData _newData)
        {
            foreach (var _action in _newData.Actions)
            {
                bool _found = false;
                foreach (var _oldActionId in roomData.Actions.Keys)
                {
                    if (_action.Key==_oldActionId)
                    {
                        _found = true;
                        break;
                    }
                }

                if (_found)
                {
                    continue;
                }

                if (_action.Value.Data.Owner == localPlayerId)
                {
                    continue;
                }
                
                OnNewAction?.Invoke(_action.Value);
            }
        }

        private bool DoesPlayerExist(string _playerId, List<RoomPlayer> _players)
        {
            foreach (var _oldPlayer in _players)
            {
                if (_playerId==_oldPlayer.Id)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void JoinRoom(RoomPlayer _playerData,RoomType _type, Action<JoinRoom> _callBack, string _name=default)
        {
            string _postData = JsonConvert.SerializeObject(new { PlayerData = JsonConvert.SerializeObject(_playerData), RoomType = _type, Name = _name
             });

            WebRequests.Instance.Post(JOIN_ROOM, _postData, 
                _response =>
                {
                    var _responseData = JsonConvert.DeserializeObject<JoinRoom>(_response);
                    roomData = _responseData.Room;
                    _callBack?.Invoke(_responseData);
                }, _response =>
                {
                    JoinRoom _data = JsonConvert.DeserializeObject<JoinRoom>(_response);
                    _data.Type = _type;
                    _data.Name = _name;
                    _callBack?.Invoke(_data);
                }, _includeHeader: false);
        }

        public void CreateRoom(RoomData _roomData, Action<CreateRoom> _callBack)
        {
            string _postData = JsonConvert.SerializeObject(new { _roomData.Id, JsonData = JsonConvert.SerializeObject(_roomData) });

            WebRequests.Instance.Post(CREATE_ROOM, _postData, _response =>
            {
                roomData = _roomData;
                _callBack?.Invoke(JsonConvert.DeserializeObject<CreateRoom>(_response));
            }, _response =>
            {
                _callBack?.Invoke(JsonConvert.DeserializeObject<CreateRoom>(_response));

            });
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

        public void SetStartingPlayerData(List<GameplayPlayerData> _data)
        {
            roomData.GameplayData.PlayersData = _data;
            if (IsOwner)
            {
                database.Child(RoomPath).Child(nameof(roomData.GameplayData)).Child(nameof(roomData.GameplayData.PlayersData))
                    .SetRawJsonValueAsync(JsonConvert.SerializeObject(_data));

            }
        }

        public void AddAction(ActionType _type, string _jsonData)
        {
            string _actionId = Guid.NewGuid().ToString();
            _actionId = actionCounter+_actionId;
            GameplayActionBase _actionData = new GameplayActionBase { Owner = localPlayerId, Type = _type };
            ActionData _data = new ActionData { Data = _actionData, JsonData = _jsonData };
            roomData.Actions.Add(_actionId,_data);
            database.Child(RoomPath).Child(nameof(roomData.Actions)).Child(_actionId).SetRawJsonValueAsync(JsonConvert.SerializeObject(_data));
            actionCounter++;
        }
    }
}