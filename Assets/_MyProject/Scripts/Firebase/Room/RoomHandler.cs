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

        private const string LEAVE_ROOM = "https://us-central1-keepersdissension.cloudfunctions.net/leaveRoom";
        private const string JOIN_ROOM = "https://joinroom-e3mmrpwoya-uc.a.run.app";
        private const string CREATE_ROOM = "https://us-central1-keepersdissension.cloudfunctions.net/createRoom";

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

        public void SubscribeToRoom()
        {
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
                Debug.Log("Updated but without args?");
                return;
            }

            if (!_args.Snapshot.Exists)
            {
                return;
            }

            if (roomData == null)
            {
                Debug.Log("Room data is null?");
                return;
            }

            RoomData _data = JsonConvert.DeserializeObject<RoomData>(_args.Snapshot.GetRawJsonValue());
            if (_data == null)
            {
                Debug.Log("Didn't manage to convert: "+_args.Snapshot.GetRawJsonValue()+" to roomdata");
                return;
            }

            if (_data.RoomPlayers == null)
            {
                Debug.Log("Players dont exist");
                return;
            }

            var _currentRoomState = JsonConvert.DeserializeObject<RoomData>(JsonConvert.SerializeObject(roomData));

            if (_currentRoomState == roomData)
            {
                Debug.Log("Data is the same");
                return;
            }
            
            roomData = _data;
            CheckIfPlayerJoined(_currentRoomState,_data);
            CheckIfPlayerLeft(_currentRoomState,_data);
            CheckIfCreatedCard(_currentRoomState,_data);
            CheckIfCardMoved(_currentRoomState,_data);
            CheckForAttackAnimation(_currentRoomState,_data);
            CheckIfCardDied(_currentRoomState,_data);
            CheckForStrangeMatterAnimation(_currentRoomState,_data);
            CheckForSoundAnimation(_currentRoomState,_data);
            CheckForBombAnimation(_currentRoomState,_data);
            CheckForBoughtStrangeMatterAnimation(_currentRoomState,_data);
            CheckForUnchain(_currentRoomState,_data);
            CheckForGameEnd(_currentRoomState,_data);
        }

        private void CheckIfPlayerJoined(RoomData _currentRoomData,RoomData _data)
        {
            foreach (var _player in _data.RoomPlayers)
            {
                if (DoesPlayerExist(_player.Id, _currentRoomData.RoomPlayers))
                {
                    continue;
                }

                OnPlayerJoined?.Invoke(_player);
            }
        }

        private void CheckIfPlayerLeft(RoomData _currentRoomData,RoomData _data)
        {
            foreach (var _player in _currentRoomData.RoomPlayers)
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

        private void CheckIfCreatedCard(RoomData _currentRoomData,RoomData _data)
        {
            foreach (var _card in _data.BoardData.Cards)
            {
                bool _shouldSpawnCard = true;
                foreach (var _existingCard in _currentRoomData.BoardData.Cards)
                {
                    if (_card.UniqueId != _existingCard.UniqueId)
                    {
                        continue;
                    }
                    
                    _shouldSpawnCard = false;
                    break;
                }


                if (_shouldSpawnCard)
                {
                    GameplayManager.Instance.OpponentCreatedCard(_card);
                    if (_card.PlaceId != -100)
                    {
                        GameplayManager.Instance.OpponentCreatedCard(_card);
                        ShowCardMoved(_card.UniqueId, _card.PlaceId);
                    }
                }
            }
        }
        
        private void CheckIfCardMoved(RoomData _currentRoomData,RoomData _data)
        {
            foreach (var _card in _data.BoardData.Cards)
            {
                bool _shouldMoveCard = false;
                foreach (var _existingCard in _currentRoomData.BoardData.Cards)
                {
                    if (_existingCard.UniqueId != _card.UniqueId)
                    {
                        continue;
                    }
                    
                    if (_card.PlaceId != _existingCard.PlaceId)
                    {
                        _shouldMoveCard = true;
                        break;
                    }
                }

                if (_shouldMoveCard)
                {
                    ShowCardMoved(_card.UniqueId, _card.PlaceId);
                }
            }
        }
        
        private void CheckForAttackAnimation(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.BoardData.AttackAnimation == null)
            {
               return;
            }

            if (string.IsNullOrEmpty(_data.BoardData.AttackAnimation.Id))
            {
                return;
            }

            if (_data.BoardData.AttackAnimation.Id == _currentRoomData.BoardData.AttackAnimation.Id)
            {
                return;
            }
            
            var _animationData = _data.BoardData.AttackAnimation;
            GameplayManager.Instance.AnimateAttack(_animationData.AttackerId,_animationData.DefenderId);
        }
        
        private void CheckIfCardDied(RoomData _currentRoomData,RoomData _data)
        {
            foreach (var _card in _data.BoardData.Cards)
            {
                bool _didCardDie = false;
                foreach (var _existingCard in _currentRoomData.BoardData.Cards)
                {
                    if (_existingCard.UniqueId != _card.UniqueId)
                    {
                        continue;
                    }

                    if (!_card.HasDied)
                    {
                        continue;
                    }
                    
                    if (_card.HasDied != _existingCard.HasDied)
                    {
                        _didCardDie = true;
                        break;
                    }
                }

                if (_didCardDie)
                {
                    GameplayManager.Instance.ShowCardAsDead(_card.UniqueId);
                }
            }
        }
        
        private void CheckForStrangeMatterAnimation(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.BoardData.StrangeMatterAnimation == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_data.BoardData.StrangeMatterAnimation.Id))
            {
                return;
            }

            if (_data.BoardData.StrangeMatterAnimation.Id == _currentRoomData.BoardData.StrangeMatterAnimation.Id)
            {
                return;
            }
            
            var _animationData = _data.BoardData.StrangeMatterAnimation;
            GameplayManager.Instance.AnimateStrangeMatter(_animationData.Amount,_animationData.ForMe,ConvertOpponentsPosition(_animationData.PositionId));
        }

        private void ShowCardMoved(string _uniqueId, int _placeId)
        {
            GameplayManager.Instance.ShowCardMoved(_uniqueId, ConvertOpponentsPosition(_placeId));
        }
        
        private int ConvertOpponentsPosition(int _position)
        {
            int _totalAmountOfFields = 64;
            return _totalAmountOfFields - _position;
        }
        
        private void CheckForSoundAnimation(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.BoardData.SoundAnimation == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_data.BoardData.SoundAnimation.Id))
            {
                return;
            }

            if (_data.BoardData.SoundAnimation.Id == _currentRoomData.BoardData.SoundAnimation.Id)
            {
                return;
            }
            
            var _animationData = _data.BoardData.SoundAnimation;
            GameplayManager.Instance.AnimateSoundEffect(_animationData.Key,_animationData.CardId);
        }        
        
        private void CheckForBombAnimation(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.BoardData.BombAnimation == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_data.BoardData.BombAnimation.Id))
            {
                return;
            }

            if (_data.BoardData.BombAnimation.Id == _currentRoomData.BoardData.BombAnimation.Id)
            {
                return;
            }
            
            var _animationData = _data.BoardData.BombAnimation;
            GameplayManager.Instance.ShowBombAnimation(ConvertOpponentsPosition(_animationData.PlaceId));
        }
        
        private void CheckForBoughtStrangeMatterAnimation(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.BoardData.BoughtStrangeMatterAnimation == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_data.BoardData.BoughtStrangeMatterAnimation.Id))
            {
                return;
            }

            if (_data.BoardData.BoughtStrangeMatterAnimation.Id == _currentRoomData.BoardData.BoughtStrangeMatterAnimation.Id)
            {
                return;
            }
            
            var _animationData = _data.BoardData.BoughtStrangeMatterAnimation;
            GameplayManager.Instance.ShowBoughtMatter(IsOwner && _animationData.DidOwnerBuy);
        }        
        
        private void CheckForUnchain(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.BoardData == null)
            {
                return;
            }

            if (_data.BoardData.MyPlayer == null)
            {
                return;
            }
            
            if (_data.BoardData.OpponentPlayer == null)
            {
                return;
            }
            
            if (_data.BoardData.MyPlayer.DidUnchainGuardian && _data.BoardData.MyPlayer.DidUnchainGuardian != _currentRoomData.BoardData.MyPlayer.DidUnchainGuardian)
            {
                GameplayManager.Instance.ShowGuardianUnchained(true);
            }
            
            if (_data.BoardData.OpponentPlayer.DidUnchainGuardian && _data.BoardData.OpponentPlayer.DidUnchainGuardian != _currentRoomData.BoardData.OpponentPlayer.DidUnchainGuardian)
            {
                GameplayManager.Instance.ShowGuardianUnchained(false);
            }
        }        
        
        private void CheckForGameEnd(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.HasGameEnded == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(_data.Winner))
            {
                return;
            }

            if (_currentRoomData.Winner == _data.Winner)
            {
                return;
            }

            GameplayManager.Instance.ShowGameEnded(_data.Winner);
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
            }, _response =>
            {
                _callBack?.Invoke(JsonConvert.DeserializeObject<NewCreateRoom>(_response));
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
        
        public RoomPlayer GetOpponent(RoomData _roomData)
        {
            foreach (var _player in _roomData.RoomPlayers)
            {
                if (_player.Id == localPlayerId)
                {
                    continue;
                }

                return _player;
            }

            throw new Exception("Can't find opponent");
        }
        
        public RoomPlayer GetMyPlayer(RoomData _roomData)
        {
            foreach (var _player in _roomData.RoomPlayers)
            {
                if (_player.Id == localPlayerId)
                {
                    return _player;
                }

            }

            throw new Exception("Can't find opponent");
        }
        
        public void UpdateRoomData()
        {
            database.Child(RoomPath).SetRawJsonValueAsync(JsonConvert.SerializeObject(roomData));
        }
    }
}