using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using FirebaseGameplay.Responses;
using Newtonsoft.Json;
using UnityEngine;

namespace FirebaseMultiplayer.Room
{
    public class RoomHandler
    {
        public static Action<RoomPlayer> OnPlayerJoined;
        public static Action<RoomPlayer> OnPlayerLeft;
        
        private DatabaseReference database;

        private string roomsPath;
        private RoomData roomData;
        private string localPlayerId;

        public bool IsOwner => roomData.Owner == localPlayerId;
        public RoomData RoomData => roomData;

        public void Init(DatabaseReference _database, string _roomsPath)
        {
            database = _database;
            roomsPath = _roomsPath;
        }

        public void SetLocalPlayerId(string _localPlayerId)
        {
            localPlayerId = _localPlayerId;
        }

        public void LeaveRoom(string _playerId)
        {
            RoomPlayer _myPlayer = default;
            foreach (var _player in roomData.RoomPlayers)
            {
                if (_player.Id==_playerId)
                {
                    _myPlayer = _player;
                    break;
                }
            }

            if (_myPlayer==default)
            {
                return;
            }

            roomData.RoomPlayers.Remove(_myPlayer);
            
            string _roomPath = roomsPath + "/" + roomData.Id;
            database.Child(_roomPath).Child(nameof(roomData.RoomPlayers)).SetValueAsync(roomData.RoomPlayers);
            UnsubscribeFromRoom();
            
        }

        public void SubscribeToRoom()
        {
            string _roomPath = roomsPath + "/" + roomData.Id;
            database.Child(_roomPath).ValueChanged += RoomUpdated;
        }

        public void UnsubscribeFromRoom()
        {
            if (roomData == null)
            {
                return;
            }

            string _roomPath = roomsPath + "/" + roomData.Id;
            database.Child(_roomPath).ValueChanged -= RoomUpdated;
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
        
        public void JoinRandomRoom(RoomPlayer _playerData, Action<JoinRoom> _callBack)
        {
            string _uri = "https://joinrandomroom-e3mmrpwoya-uc.a.run.app";
            string _postData = JsonConvert.SerializeObject(new { PlayerData = JsonConvert.SerializeObject(_playerData) });

            WebRequests.Instance.Post(_uri, _postData, 
                _response =>
                {
                    var _responseData = JsonConvert.DeserializeObject<JoinRoom>(_response);
                    roomData = _responseData.Room;
                    _callBack?.Invoke(_responseData);
                }, _response =>
                {
                    _callBack?.Invoke(JsonConvert.DeserializeObject<JoinRoom>(_response));
                }, _includeHeader: false);
        }

        public void CreateRoom(RoomData _roomData, Action<CreateRoom> _callBack)
        {
            string _uri = "https://createroom-e3mmrpwoya-uc.a.run.app";
            string _postData = JsonConvert.SerializeObject(new { _roomData.Id, JsonData = JsonConvert.SerializeObject(_roomData) });

            WebRequests.Instance.Post(_uri, _postData, _response =>
            {
                roomData = _roomData;
                _callBack?.Invoke(JsonConvert.DeserializeObject<CreateRoom>(_response));
            }, _response =>
            {
                _callBack?.Invoke(JsonConvert.DeserializeObject<CreateRoom>(_response));

            });
        }
    }
}