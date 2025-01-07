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
            Debug.Log("Setting data, amount of abilities: "+roomData.BoardData.Abilities.Count);
            CheckIfPlayerJoined(_currentRoomState,_data);
            CheckIfPlayerLeft(_currentRoomState,_data);
            if (!SceneManager.IsGameplayScene)
            {
                return;
            }
            CheckIfCreatedCard(_currentRoomState,_data);
            CheckIfAbilityCard(_currentRoomState,_data);
            CheckIfCardMoved(_currentRoomState,_data);
            CheckIfAbilityPlaced(_currentRoomState, _data);
            CheckForAttackAnimation(_currentRoomState,_data);
            CheckIfCardDied(_currentRoomState,_data);
            CheckForStrangeMatterAnimation(_currentRoomState,_data);
            CheckForSoundAnimation(_currentRoomState,_data);
            CheckForBombAnimation(_currentRoomState,_data);
            CheckForBoughtStrangeMatterAnimation(_currentRoomState,_data);
            CheckForUnchain(_currentRoomState,_data);
            CheckForGameEnd(_currentRoomState,_data);
            CheckForAbilityDisplay(_currentRoomState,_data);
            CheckForDelivery(_currentRoomState, _data);
            ShouldEndTurn(_currentRoomState,_data);
            CheckIfOpponentEndedTurn(_currentRoomState, _data);
            CheckForComrade(_currentRoomState, _data);
            CheckForReduction(_currentRoomState, _data);
            CheckForVetoAnimation(_currentRoomState, _data);
            CheckForSpriteChange(_currentRoomState, _data);
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
                        ShowCardMoved(_card.UniqueId, _card.PlaceId);
                    }
                }
            }
        }
        
        private void CheckIfAbilityCard(RoomData _currentRoomData,RoomData _data)
        {
            foreach (var _card in _data.BoardData.Abilities)
            {
                bool _shouldSpawnCard = true;
                foreach (var _existingCard in _currentRoomData.BoardData.Abilities)
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
                    GameplayManager.Instance.OpponentCreatedAbility(_card);
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
        
        private void CheckIfAbilityPlaced(RoomData _currentRoomData,RoomData _data)
        {
            foreach (var _card in _data.BoardData.Abilities)
            {
                bool _shouldMoveCard = false;
                foreach (var _existingCard in _currentRoomData.BoardData.Abilities)
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
                    GameplayManager.Instance.ShowAbilityOnTable(_card.UniqueId, ConvertOpponentsPosition(_card.PlaceId));
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

        private void CheckForDelivery(RoomData _currentRoomData,RoomData _data)
        {
            var _currentState = _currentRoomData.GameplaySubState;
            if (IsOwner)
            {
                if (_data.GameplaySubState == GameplaySubState.Player1DeliveryReposition)
                {
                    if (_currentRoomData.GameplaySubState != GameplaySubState.Player1DeliveryReposition)
                    {
                        GameplayManager.Instance.UseDelivery(_data.BoardData.DeliveryCard, FinishDelivery);
                    }
                }
            }
            else
            {
                if (_data.GameplaySubState == GameplaySubState.Player2DeliveryReposition)
                {
                    if (_currentRoomData.GameplaySubState != GameplaySubState.Player2DeliveryReposition)
                    {
                        GameplayManager.Instance.UseDelivery(_data.BoardData.DeliveryCard, FinishDelivery);
                    }
                }
            }

            void FinishDelivery()
            {
                roomData.GameplaySubState = _currentState;
                RoomUpdater.Instance.ForceUpdate();
            }
        }
        
        private void ShouldEndTurn(RoomData _currentRoomData,RoomData _data)
        {
            bool _shouldEndTurn = false;
            if (IsOwner)
            {
                if (_currentRoomData.GameplaySubState == GameplaySubState.Player2ResponseAction)
                {
                    if (_data.GameplaySubState == GameplaySubState.Playing)
                    {
                        if (_data.BoardData.MyPlayer.ActionsLeft == 0)
                        {
                            _shouldEndTurn = true;
                        }
                    }
                }
            }
            else
            {
                if (_currentRoomData.GameplaySubState == GameplaySubState.Player1ResponseAction)
                {
                    if (_data.GameplaySubState == GameplaySubState.Playing)
                    {
                        if (_data.BoardData.MyPlayer.ActionsLeft == 0)
                        {
                            _shouldEndTurn = true;
                        }
                    }
                }
            }

            if (!_shouldEndTurn)
            {
                return;
            }
            
            GameplayManager.Instance.EndTurn();
        }
        
        private void CheckIfOpponentEndedTurn(RoomData _currentRoomData,RoomData _data)
        {
            bool _didOpponentEndTurn = false;
            if (IsOwner)
            {
                if (_currentRoomData.CurrentPlayerTurn == 2)
                {
                    if (_data.CurrentPlayerTurn == 1)
                    {
                        _didOpponentEndTurn = true;
                    }
                }
            }
            else
            {
                if (_currentRoomData.CurrentPlayerTurn == 1)
                {
                    if (_data.CurrentPlayerTurn == 2)
                    {
                        _didOpponentEndTurn = true;
                    }
                }
            }

            if (!_didOpponentEndTurn)
            {
                return;
            }

            GameplayManager.Instance.DidOpponentFinish = true;
        }

        private void ShowCardMoved(string _uniqueId, int _placeId)
        {
            GameplayManager.Instance.ShowCardMoved(_uniqueId, ConvertOpponentsPosition(_placeId),null);
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
        
        private void CheckForAbilityDisplay(RoomData _currentRoomData,RoomData _data)
        {
            foreach (var _currentAbility in _currentRoomData.BoardData.Abilities)
            {
                foreach (var _newAbility in _data.BoardData.Abilities)
                {
                    if (_newAbility.UniqueId != _currentAbility.UniqueId)
                    {
                        continue;
                    }
                    
                    if (_currentAbility.IsLightUp == _newAbility.IsLightUp)
                    {
                        continue;
                    }

                    GameplayManager.Instance.ManageAbilityActive(_newAbility.UniqueId, _newAbility.IsLightUp);
                }
            }
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
            
            if (_currentRoomData.BoardData == null)
            {
                return;
            }

            if (_data.BoardData.MyPlayer == null)
            {
                return;
            }
            
            if (_currentRoomData.BoardData.MyPlayer == null)
            {
                return;
            }
            
            if (_data.BoardData.OpponentPlayer == null)
            {
                return;
            }
            
            if (_currentRoomData.BoardData.OpponentPlayer == null)
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
        
        private void CheckForComrade(RoomData _currentRoomData,RoomData _data)
        {
            if (_currentRoomData.GameplaySubState == _data.GameplaySubState)
            {
                return;
            }
            
            if (_data.GameplaySubState is GameplaySubState.Player1UseComrade)
            {
                if (IsOwner)
                {
                     GameplayManager.Instance.SetGameplaySubStateHelper(_currentRoomData.GameplaySubState);
                    GameplayManager.Instance.HandleComrade(Finish);
                }
            }
            else if (_data.GameplaySubState is GameplaySubState.Player2UseComrade)
            {
                if (!IsOwner)
                {
                    GameplayManager.Instance.SetGameplaySubStateHelper(_currentRoomData.GameplaySubState);
                    GameplayManager.Instance.HandleComrade(Finish);
                }
            }


            void Finish(bool _)
            {
                GameplayManager.Instance.SetGameplaySubState(GameplayManager.Instance.GetGameplaySubStateHelper());
                RoomUpdater.Instance.ForceUpdate();
            }
        }
        
        private void CheckForReduction(RoomData _currentRoomData,RoomData _data)
        {
            if (_currentRoomData.GameplaySubState == _data.GameplaySubState)
            {
                return;
            }
            
            if (_data.GameplaySubState is GameplaySubState.Player1UseReduction)
            {
                if (IsOwner)
                {
                     GameplayManager.Instance.UseReduction(Finish);
                }
            }
            else if (_data.GameplaySubState is GameplaySubState.Player2UseReduction)
            {
                if (!IsOwner)
                {
                    GameplayManager.Instance.UseReduction(Finish);
                }
            }


            void Finish()
            {
                GameplayManager.Instance.SetGameplaySubState(GameplayManager.Instance.GetGameplaySubStateHelper());
                RoomUpdater.Instance.ForceUpdate();
            }
        }
        
        private void CheckForVetoAnimation(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.BoardData.VetoAnimation == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_data.BoardData.VetoAnimation.Id))
            {
                return;
            }

            if (_data.BoardData.VetoAnimation.Id == _currentRoomData.BoardData.VetoAnimation.Id)
            {
                return;
            }
            
            var _animationData = _data.BoardData.VetoAnimation;
            GameplayManager.Instance.ShowVetoAnimation(_animationData.CardId, _animationData.IsVetoed);
        }

        private void CheckForSpriteChange(RoomData _currentRoomData,RoomData _data)
        {
            if (_data.BoardData.ChangeSpriteData == null)
            {
                return;
            }

            if (_data.BoardData.ChangeSpriteData.Count == 0)
            {
                return;
            }

            foreach (var _changeSpriteData in _data.BoardData.ChangeSpriteData)
            {
                bool _found = false;
                foreach (var _knownChange in _currentRoomData.BoardData.ChangeSpriteData)
                {
                    if (_knownChange.Id == _changeSpriteData.Id)
                    {
                        _found = true;
                        break;
                    }
                }

                if (_found)
                {
                    continue;
                }
                GameplayManager.Instance.ChangeSpriteAnimate(_changeSpriteData.CardId, _changeSpriteData.SpriteId, _changeSpriteData.ShowPlaceAnimation);
            }
        }        
        
        public void JoinRoom(RoomPlayer _playerData, RoomGameplayPlayer _gamePlayerData, RoomType _type, Action<JoinRoom> _callBack, string _name = 
                default)
        {
            string _postData = JsonConvert.SerializeObject(new { PlayerData = JsonConvert.SerializeObject(new {playerData = _playerData, 
                gamePlayerData = _gamePlayerData }), Type = _type, Name = _name,  GameVersion = Application.version });

            WebRequests.Instance.Post(JOIN_ROOM, _postData, _response =>
            {
                JoinRoom _responseData = JsonConvert.DeserializeObject<JoinRoom>(_response);
                roomData = _responseData.Room;
                _responseData.Name = _name;
                _responseData.Type = _type;
                _callBack?.Invoke(_responseData);
            }, _response =>
            {
                JoinRoom _data = JsonConvert.DeserializeObject<JoinRoom>(_response);
                _callBack?.Invoke(_data);
            }, _includeHeader: false);
        }

        public void CreateRoom(RoomData _roomData, Action<CreateRoom> _callBack)
        {
            string _postData = JsonConvert.SerializeObject(new { _roomData.Id, JsonData = JsonConvert.SerializeObject(_roomData),GameVersion = Application.version });

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