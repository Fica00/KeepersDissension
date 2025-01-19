using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using FirebaseMultiplayer.Room;

public class GameplayManagerPvp : GameplayManager
{
    private RoomHandler RoomHandler => FirebaseManager.Instance.RoomHandler;
    private RoomData RoomData => RoomHandler.RoomData;
    private BoardData BoardData => RoomData.BoardData;
    
    protected override void Awake()
    {
        base.Awake();
        DataManager.Instance.PlayerData.CurrentRoomId = RoomData.Id;
    }

    private void OnEnable()
    {
        RoomHandler.OnPlayerLeft += OpponentLeftRoom;
        OnCardMoved += TrySaveSlowDown;
    }

    private void OnDisable()
    {
        RoomHandler.OnPlayerLeft -= OpponentLeftRoom;
        OnCardMoved -= TrySaveSlowDown;
    }

    private void TrySaveSlowDown(CardBase _cardThatMoved, int _arg2, int _arg3)
    {
        string _uniqueId = ((Card)_cardThatMoved).UniqueId;
        if (IsAbilityActive<SlowDown>())
        {
            SlowDown _slowDown = FindObjectOfType<SlowDown>();
            if (!_slowDown.CanMoveCard(_uniqueId))
            {
                return;
            }

            _slowDown.AddCard(_uniqueId);
        }
    }

    private void OpponentLeftRoom(RoomPlayer _player)
    {
        if (HasGameEnded())
        {
            return;
        }

        DialogsManager.Instance.ShowOkDialog("Opponent resigned");
        EndGame(true);
    }

    protected override void SetupTable()
    {
        MyPlayer.Setup(DataManager.Instance.PlayerData.FactionId, true);
        RoomPlayer _opponent = RoomHandler.GetOpponent();
        OpponentPlayer.Setup(_opponent.FactionId, false);
        GameplayUI.Instance.Setup();
        if (!RoomHandler.IsOwner)
        {
            return;
        }

        SetupStartingCards(MyPlayer.TableSideHandler, MyPlayer.FactionSo, FirebaseManager.Instance.PlayerId);
        SetupStartingCards(OpponentPlayer.TableSideHandler, OpponentPlayer.FactionSo, FirebaseManager.Instance.OpponentId);
        RoomUpdater.Instance.ForceUpdate();
    }

    private void SetupStartingCards(TableSideHandler _tableSideHandler, FactionSO _factionSo, string _playerId)
    {
        Card _wallCard = null;
        foreach (var _cardInDeck in CardsManager.Instance.Get(_factionSo))
        {
            if (_cardInDeck == null)
            {
                continue;
            }

            Card _card = CreateCard(_cardInDeck.Details.Id, _tableSideHandler, Guid.NewGuid().ToString(), true, _playerId);
            if (_card is Wall)
            {
                _wallCard = _card;
            }
        }

        for (int _i = 0; _i < 30; _i++)
        {
            CreateCard(_wallCard.Details.Id, _tableSideHandler, Guid.NewGuid().ToString(), true, _playerId);
        }
    }

    public override void OpponentCreatedCard(CardData _cardData)
    {
        CreateCard(_cardData.CardId, OpponentPlayer.TableSideHandler, _cardData.UniqueId, false, _cardData.Owner);
    }

    public override void OpponentCreatedAbility(AbilityData _abilityData)
    {
        var _card = Instantiate(Resources.LoadAll<AbilityCard>("Abilities").First(_ability => _ability.Details.Id == _abilityData.CardId));
        _card.Setup(_abilityData.UniqueId);
    }

    public override Card CreateCard(int _cardId, TableSideHandler _tableSideHandler, string _uniqueId, bool _addCard, string _owner)
    {
        Transform _cardsHolder = _tableSideHandler.CardsHolder;
        Card _card = CardsManager.Instance.CreateCard(_cardId);
        _card.transform.SetParent(_cardsHolder);
        _card.SetParent(_cardsHolder);

        if (_addCard)
        {
            CardData _cardData = _card.GenerateCardData(_owner, _uniqueId);
            AddCard(_cardData);
        }

        _card.SetUniqueId(_uniqueId);
        _card.Setup(_uniqueId);
        return _card;
    }

    protected override bool DecideWhoPlaysFirst()
    {
        return RoomHandler.IsOwner;
    }

    public override void EndGame(bool _didIWin)
    {
        if (HasGameEnded())
        {
            return;
        }

        var _winner = _didIWin ? BoardData.MyPlayer.PlayerId : BoardData.OpponentPlayer.PlayerId;
        SetHasGameEnded(true, _winner);
        ShowGameEnded(_winner);
        RoomUpdater.Instance.ForceUpdate();
    }

    public override void PlaceCard(CardBase _card, int _positionId)
    {
        string _cardId;
        if (_card is AbilityCard _abilityCard)
        {
            _cardId = _abilityCard.UniqueId;
            _card.Display.Setup(_abilityCard);
        }
        else
        {
            Card _cardWarrior = (Card)_card;
            _cardId = _cardWarrior.UniqueId;
            _card.Display.Setup(_cardWarrior);
        }

        PlaceCard(_cardId, _positionId);
    }

    private void PlaceCard(string _cardId, int _positionId)
    {
        CardBase _cardBase = GetAllCards().Find(_card => _card.UniqueId == _cardId);

        if (_cardBase == null)
        {
            return;
        }

        Card _card = null;

        if (_cardBase is Card _isCard)
        {
            _card = _isCard;
        }

        if (_card == null)
        {
            return;
        }

        _card.CardData.PlaceId = _positionId;
        RoomData.BoardData.Cards.Find(_card => _card.UniqueId == _cardId).PlaceId = _positionId;
        ShowCardPlaced(_card.UniqueId, _positionId);
    }

    public override void ShowCardPlaced(string _uniqueId, int _positionId)
    {
        Card _card = GetCard(_uniqueId);
        if (_card == null)
        {
            return;
        }

        _card.PositionOnTable(TableHandler.GetPlace(_positionId));
        OnPlacedCard?.Invoke(_card);
    }

    public override void ShowCardMoved(string _uniqueId, int _positionId, Action _callBack)
    {
        Card _card = GetCard(_uniqueId);
        if (_card == null)
        {
            _callBack?.Invoke();
            return;
        }

        var _destination = TableHandler.GetPlace(_positionId);
        if (_destination == null)
        {
            _callBack?.Invoke();
            return;
        }

        IsAnimating = true;
        _card.MoveToPosition(_destination, () =>
        {
            IsAnimating = false;
            _callBack?.Invoke();
        });
    }

    public override void AddAbilityToPlayer(string _owner, string _abilityId)
    {
        var _abilityData = FirebaseManager.Instance.RoomHandler.BoardData.Abilities.Find(_ability => _ability.UniqueId == _abilityId);
        if (_abilityData == null)
        {
            return;
        }

        _abilityData.Owner = _owner;
    }

    public override void AddAbilityToShop(string _abilityId)
    {
        var _abilityData = FirebaseManager.Instance.RoomHandler.BoardData.Abilities.Find(_ability => _ability.UniqueId == _abilityId);
        if (_abilityData == null)
        {
            return;
        }

        _abilityData.Owner = "shop";
    }

    public override void ExecuteMove(int _startingPlaceId, int _finishingPlaceId, string _firstCardId, Action _callBack)
    {
        TablePlaceHandler _destination = TableHandler.GetPlace(_finishingPlaceId);
        Card _movingCard = GetCard(_firstCardId);
        
        if (_movingCard == null)
        {
            _callBack?.Invoke();
            return;
        }

        if (!_movingCard.CheckCanMove())
        {
            _callBack?.Invoke();
            return;
        }

        if (_destination.ContainsMarker)
        {
            Card _marker = _destination.GetMarker();

            if (_marker.IsVoid)
            {
                ShowCardMoved(_movingCard.UniqueId, _destination.Id,
                    () => { DamageCardByAbility(_movingCard.UniqueId, _movingCard.CardData.Stats.Health, _ => _callBack?.Invoke()); });
                return;
            }
            else
            {
                CardBase _cardBase = _destination.GetCardNoWall();
                GameplayPlayer _markerOwner = _cardBase.GetIsMy() ? MyPlayer : OpponentPlayer;
                _markerOwner.DestroyCard(_cardBase as Card);
            }
        }

        _movingCard.CardData.PlaceId = _destination.Id;

        ShowCardMoved(_movingCard.UniqueId, _destination.Id, () =>
        {
            OnCardMoved?.Invoke(_movingCard, _startingPlaceId, _finishingPlaceId);

            if (IsAbilityActive<Penalty>())
            {
                Penalty _penalty = FindObjectOfType<Penalty>();
                if (!_penalty.IsMy && _movingCard is Keeper)
                {
                    int _distance =
                        TableHandler.DistanceBetweenPlaces(TableHandler.GetPlace(_startingPlaceId), TableHandler.GetPlace(_finishingPlaceId));
                    int _speed = _movingCard.CardData.Stats.Speed;
                    int _damageToTake = Mathf.Min(_speed, _distance);
                    DamageCardByAbility(_movingCard.UniqueId, _damageToTake, _ => { _callBack?.Invoke(); });

                    return;
                }
            }

            _callBack?.Invoke();
        });
        PlayMovingSoundEffect(_movingCard);
    }

    public override void ExecuteSwitchPlace(int _startingPlaceId, int _finishingPlaceId, string _firstCardId, string _secondCardId, Action _callBack)
    {
        TablePlaceHandler _startingDestination = TableHandler.GetPlace(_startingPlaceId);
        TablePlaceHandler _destination = TableHandler.GetPlace(_finishingPlaceId);

        Card _firstCard = GetCard(_firstCardId);
        Card _secondCard = GetCard(_secondCardId);

        if (_firstCard == null || _secondCard == null)
        {
            _callBack?.Invoke();
            return;
        }

        _firstCard.CardData.PlaceId = _destination.Id;
        _secondCard.CardData.PlaceId = _startingDestination.Id;

        ShowCardMoved(_firstCard.UniqueId, _destination.Id, null);
        ShowCardMoved(_secondCard.UniqueId, _startingDestination.Id, _callBack);

        OnSwitchedPlace?.Invoke(_firstCard, _secondCard);
    }

    public override void ExecuteAttack(string _firstCardId, string _secondCardId, Action _callBack)
    {
        if (IsAbilityActive<Truce>())
        {
            SaySomethingToAll("Truce is active, attack canceled");
            _callBack?.Invoke();
            return;
        }

        Card _attackingCard = GetCard(_firstCardId);
        Card _defendingCard = GetCard(_secondCardId);

        if (_attackingCard == null || _defendingCard == null)
        {
            _callBack?.Invoke();
            return;
        }

        AudioManager.Instance.PlaySoundEffect("Attack");
        int _attackerPlace = _attackingCard.GetTablePlace().Id;
        int _defenderPlace = _defendingCard.GetTablePlace().Id;

        if (_attackingCard == _defendingCard)
        {
            ResolveEndOfAttack(_attackingCard, _defendingCard, () =>
            {
                TryToApplyWallAbility(_defendingCard, _attackingCard, _callBack,_defenderPlace);
            });
            return;
        }

        BoardData.AttackAnimation = new AttackAnimation
        {
            AttackerId = _attackingCard.UniqueId, DefenderId = _defendingCard.UniqueId, Id = Guid.NewGuid().ToString()
        };


        AnimateAttack(_attackingCard.UniqueId, _defendingCard.UniqueId,
            () => { ResolveEndOfAttack(_attackingCard, _defendingCard, () =>
            {
                TryToApplyWallAbility(_defendingCard, _attackingCard, _callBack,_defenderPlace);
            }); });
    }
    
    private void TryToApplyWallAbility(Card _defendingCard, Card _attackingCard, Action _callBack, int _defenderPlace)
    {
        if (_defenderPlace == -1)
        {
            _callBack?.Invoke();
            return;    
        }
        if (_defendingCard is not Wall _wall)
        {
            _callBack?.Invoke();
            return;
        }

        if (IsAbilityActive<Collapse>())
        {
            DamageCardByAbility(_attackingCard.UniqueId, 1, _didKill =>
            {
                if (_didKill)
                {
                    _callBack?.Invoke();
                    return;
                }

                ApplyWallAbility(_wall);
            });
        }
        else
        {
            ApplyWallAbility(_wall);
        }

        void ApplyWallAbility(Wall _wall)
        {

            if (_wall.IsCyber())
            {
                ApplyCyborgAbility(_attackingCard.UniqueId, _defenderPlace, _callBack);
                return;
            }

            if (_wall.IsDragon())
            {
                ApplyDragonAbility(_defenderPlace);
                _callBack?.Invoke();
                return;
            }

            if (_wall.IsForest())
            {
                ApplyForestAbility(_attackingCard);
                _callBack?.Invoke();
                return;
            }

            if (_wall.IsSnow())
            {
                ApplySnowAbility(_attackingCard);
                _callBack?.Invoke();
                return;
            }

            _callBack?.Invoke();
        }
    }

    private void ApplyCyborgAbility(string _attacker, int _wallPosition, Action _callBack)
    {
        Card _attackerCard = GetCard(_attacker);
        if (_attackerCard.HasScaler())
        {
            if (_attackerCard.GetTablePlace().GetWall() != null)
            {
                return;
            }
        }
        if (CanPushBackCard(_attacker, _wallPosition))
        {
            PushCardBack(_attacker, _wallPosition, () => { _callBack?.Invoke(); });
        }
        else
        {
            DamageCardByAbility(_attacker, 1, _ => { _callBack?.Invoke(); });
        }
    }

    private bool CanPushBackCard(string _attacker, int _secondCardPlace)
    {
        Card _attackerCard = GetCard(_attacker);
        if (!_attackerCard.CheckCanMove())
        {
            return false;
        }

        int _attackerPlace = _attackerCard.GetTablePlace().Id;

        TablePlaceHandler _placeInBack = TableHandler.CheckForPlaceInBack(_attackerPlace, _secondCardPlace);
        if (_placeInBack == null)
        {
            return false;
        }

        if (_placeInBack.IsAbility)
        {
            return false;
        }

        if (_placeInBack.IsOccupied)
        {
            return false;
        }

        return true;
    }

    private void PushCardBack(string _attacker, int _secondPlace, Action _callBack)
    {
        Card _attackerCard = Instance.GetCard(_attacker);
        int _attackerPlace = _attackerCard.GetTablePlace().Id;

        TablePlaceHandler _placeInFront = TableHandler.CheckForPlaceInBack(_attackerPlace, _secondPlace);
        ExecuteMove(GetCard(_attacker).GetTablePlace().Id, _placeInFront.Id, _attacker, _callBack);
    }

    private void ApplyDragonAbility(int _wallPlace)
    {
        List<Card> _cards = TableHandler.GetAttackableCards(_wallPlace, CardMovementType.EightDirections);
        foreach (var _card in _cards)
        {
            if (_card is Wall)
            {
                continue;
            }

            DamageCardByAbility(_card.UniqueId, 1, null);
        }
    }

    private void ApplyForestAbility(Card _attacker)
    {
        if (_attacker.Health <= 0)
        {
            return;
        }

        _attacker.ChangeHealth(1);
    }

    private void ApplySnowAbility(Card _attacker)
    {
        _attacker.CardData.HasSnowWallEffect = true;
    }

    public override void AnimateAttack(string _attackerId, string _defenderId, Action _callBack = null)
    {
        Card _attackingCard = GetCard(_attackerId);
        Card _defendingCard = GetCard(_defenderId);

        _attackingCard.transform.DOMove(_defendingCard.transform.position, 0.5f).OnComplete(() =>
        {
            _attackingCard.transform.DOLocalMove(Vector3.zero, 0.5f).SetDelay(0.2f).OnComplete(() => { _callBack?.Invoke(); });
        });
    }

    private void ResolveEndOfAttack(Card _attackingCard, Card _defendingCard, Action _callBack)
    {
        StartCoroutine(ResolveEndOfAttackRoutine());

        IEnumerator ResolveEndOfAttackRoutine()
        {
            int _damage = _attackingCard.Damage;

            if (IsAbilityActive<HighStakes>())
            {
                HighStakes _highStakes = FindObjectOfType<HighStakes>();
                _highStakes.TryToCancel();
                _damage = 8;
            }


            bool _canGetResponse = true;
            bool _waitForSomething;

            if (_defendingCard.HasDelivery)
            {
                _waitForSomething = false;
                if (_defendingCard.GetIsMy())
                {
                    UseDelivery(_defendingCard.UniqueId, ContinueWithExecution);
                    yield return new WaitUntil(() => _waitForSomething);
                }
                else
                {
                    BoardData.DeliveryCard = _defendingCard.UniqueId;
                    var _currentState = GetGameplaySubState();
                    if (RoomHandler.IsOwner)
                    {
                        SetGameplaySubState(GameplaySubState.Player2DeliveryReposition);
                    }
                    else
                    {
                        SetGameplaySubState(GameplaySubState.Player1DeliveryReposition);
                    }

                    RoomUpdater.Instance.ForceUpdate();
                    yield return new WaitUntil(() => GetGameplaySubState() == _currentState);
                }

                _defendingCard.ChangeDelivery(false);
                _damage = 0;
                _canGetResponse = false;
            }

            if (IsAbilityActive<Hunter>())
            {
                Hunter _hunter = FindObjectOfType<Hunter>();
                if (_hunter.IsMy != _defendingCard.GetIsMy())
                {
                    if (_defendingCard is Guardian && _attackingCard is Keeper)
                    {
                        _damage *= 2;
                    }
                }
            }

            if (IsAbilityActive<Invincible>())
            {
                Invincible _invincible = FindObjectOfType<Invincible>();
                if (_invincible.IsMy == _defendingCard.GetIsMy())
                {
                    if (_defendingCard is Keeper)
                    {
                        _damage = 0;
                    }
                }
            }

            if (IsAbilityActive<Steadfast>())
            {
                Steadfast _steadfast = FindObjectOfType<Steadfast>();
                if (_steadfast.IsMy == _defendingCard.GetIsMy())
                {
                    if (_defendingCard is Keeper && _attackingCard is Minion)
                    {
                        _damage = 0;
                    }
                }
            }

            if (IsAbilityActive<Grounded>())
            {
                Grounded _grounded = FindObjectOfType<Grounded>();
                if (_grounded.IsMy && _attackingCard is Keeper && _grounded.CanApplyEffect())
                {
                    _grounded.ApplyEffect(_defendingCard.UniqueId);
                    _damage = 0;
                    SaySomethingToAll("Grounded activated");
                }
                else if (_grounded.IsCardEffected(_defendingCard.UniqueId))
                {
                    _grounded.EndEffect();
                }
            }

            if (_damage>0)
            {
                if (_defendingCard.HasBlockaderAbility())
                {
                    if (_defendingCard.TryToUseBlockaderAbility())
                    {
                        _damage--;
                    }
                }
            }

            _defendingCard.ChangeHealth(-_damage);
            var _defendingPosition = _defendingCard.GetTablePlace().Id;

            OnCardAttacked?.Invoke(_attackingCard, _defendingCard, _damage);
            bool _didGiveResponseAction = false;

            if (IsAbilityActive<Ambush>())
            {
                Ambush _ambush = FindObjectOfType<Ambush>();
                if (_ambush.IsMy == _attackingCard.GetIsMy())
                {
                    _ambush.MarkAsUsed();
                    SaySomethingToAll("Ambus activated");
                    _canGetResponse = false;
                }
            }

            if (_canGetResponse)
            {
                _didGiveResponseAction = CheckForResponseAction(_attackingCard, _defendingCard);
            }

            CheckIfDefenderIsDestroyed(_defendingCard, FinishResolve);

            void ContinueWithExecution()
            {
                _waitForSomething = true;
            }

            void FinishResolve(bool _didDie)
            {
                if (_didDie)
                {
                    if (_defendingCard is LifeForce)
                    {
                        EndGame(!_defendingCard.My);
                        return;
                    }

                    HandleLoot(_attackingCard, _defendingCard, _defendingPosition);
                }
                else
                {
                    if (IsAbilityActive<Repel>())
                    {
                        Repel _repel = FindObjectOfType<Repel>();
                        if (_repel.IsMy && _attackingCard is Keeper)
                        {
                            PushCard(_defendingCard.GetTablePlace().Id, _attackingCard.GetTablePlace().Id);
                        }
                    }
                }

                if (IsAbilityActive<Retaliate>())
                {
                    Retaliate _retaliate = FindObjectOfType<Retaliate>();
                    if (_retaliate.IsMy == _defendingCard.GetIsMy() && _defendingCard is Keeper)
                    {
                        DamageCardByAbility(_attackingCard.UniqueId, _damage, _ => CallEnd());
                        return;
                    }
                }

                CallEnd();
            }

            void CallEnd()
            {
                _callBack?.Invoke();
                if (_didGiveResponseAction && MyPlayer.Actions == 0)
                {
                    RoomUpdater.Instance.ForceUpdate();
                }
            }
        }
    }

    public override void UseDelivery(string _defendingCardId, Action _callBack)
    {
        Card _card = GetCard(_defendingCardId);
        int _startingPlace = GetCard(_defendingCardId).GetTablePlace().Id;
        if (_card is Guardian _guardian)
        {
            if (_guardian.IsChained)
            {
                HandleChainedGuardian();
                return;
            }    
        }
        List<TablePlaceHandler> _emptyPlaces = GetDeliveryPlaces();
        _emptyPlaces.Add(TableHandler.GetPlace(_startingPlace));
        if (_emptyPlaces.Count == 0)
        {
            _callBack?.Invoke();
            return;
        }

        if (_emptyPlaces.Count == 1)
        {
            ExecuteMove(_startingPlace, _emptyPlaces[0].Id, _defendingCardId, _callBack);
        }
        else
        {
            StartCoroutine(SelectPlace(_emptyPlaces, true, DoPlaceDeliveryCard));
        }


        void HandleChainedGuardian()
        {
            List<int> _lifeForcePlaces = new List<int>()
            {
                10,
                12,
            };
            List<TablePlaceHandler> _possiblePlaces = new();
            foreach (var _placeId in _lifeForcePlaces)
            {
                var _place = TableHandler.GetPlace(_placeId);
                if (_place.IsOccupied)
                {
                    continue;
                }
                    
                _possiblePlaces.Add(_place);
            }

            if (_possiblePlaces.Count==0)
            {
                _lifeForcePlaces = new List<int> { 19, 18, 17 };
                foreach (var _placeId in _lifeForcePlaces)
                {
                    var _place = TableHandler.GetPlace(_placeId);
                    if (_place.IsOccupied)
                    {
                        continue;
                    }
                    
                    _possiblePlaces.Add(_place);
                }
            }

            if (_possiblePlaces.Count==0)
            {
                _possiblePlaces.Add(_card.GetTablePlace());
            }
                
            if (_possiblePlaces.Count == 1)
            {
                ExecuteMove(_startingPlace, _possiblePlaces[0].Id, _defendingCardId, _callBack);
            }
            else
            {
                StartCoroutine(SelectPlace(_possiblePlaces, true, DoPlaceDeliveryCard));
            }
        }

        void DoPlaceDeliveryCard(int _placeId)
        {
            ExecuteMove(_startingPlace, _placeId, _defendingCardId, _callBack);
        }
    }

    private List<TablePlaceHandler> GetDeliveryPlaces()
    {
        List<TablePlaceHandler> _emptyPlaces = GetEmptyPlaces(new List<int>()
        {
            8,
            9,
            10,
            11,
            12,
            13,
            14
        });
        if (_emptyPlaces.Count != 0)
        {
            return _emptyPlaces;
        }

        _emptyPlaces = GetEmptyPlaces(new List<int>()
        {
            12,
            10,
            19,
            18,
            17
        });
        if (_emptyPlaces.Count != 0)
        {
            return _emptyPlaces;
        }

        _emptyPlaces = GetEmptyPlaces(new List<int>()
        {
            13,
            20,
            27,
            26,
            25,
            24,
            23,
            16,
            9
        });
        if (_emptyPlaces.Count != 0)
        {
            return _emptyPlaces;
        }

        _emptyPlaces = GetEmptyPlaces(new List<int>()
        {
            14,
            21,
            28,
            27,
            26,
            25,
            24,
            23,
            22,
            15,
            8
        });
        if (_emptyPlaces.Count != 0)
        {
            return _emptyPlaces;
        }

        for (int _i = 8; _i < 57; _i++)
        {
            TablePlaceHandler _place = TableHandler.GetPlace(_i);
            if (_place.IsOccupied)
            {
                continue;
            }

            _emptyPlaces.Add(_place);
        }

        return _emptyPlaces;

        List<TablePlaceHandler> GetEmptyPlaces(List<int> _placeIds)
        {
            List<TablePlaceHandler> _places = new List<TablePlaceHandler>();
            foreach (var _placeId in _placeIds)
            {
                TablePlaceHandler _place = TableHandler.GetPlace(_placeId);
                if (_place.IsOccupied)
                {
                    continue;
                }

                _places.Add(_place);
            }

            return _places;
        }
    }

    private bool CheckForResponseAction(Card _attackingCard, Card _defendingCard)
    {
        if (_defendingCard.My == _attackingCard.My)
        {
            return TryToAwardResponseForFalling(_attackingCard, _defendingCard);
        }

        if (!_defendingCard.IsWarrior())
        {
            return TryToAwardResponseForFalling(_attackingCard, _defendingCard);
        }

        if (_defendingCard.Health <= 0)
        {
            return false;
        }

        if (IsMyResponseAction())
        {
            return false;
        }

        SetResponseAction(_defendingCard.My && RoomHandler.IsOwner, _defendingCard.UniqueId);
        return true;
    }

    private bool TryToAwardResponseForFalling(Card _attackingCard, Card _defendingCard)
    {
        if (_defendingCard is Wall _wall)
        {
            TablePlaceHandler _place = _wall.GetTablePlace();
            foreach (var _cardOnPlace in _place.GetCards())
            {
                if (_cardOnPlace.GetIsMy() == _attackingCard.GetIsMy())
                {
                    continue;
                }

                if (_cardOnPlace is not Card _card)
                {
                    continue;
                }

                if (!_card.HasScaler())
                {
                    continue;
                }

                IsFallingResponse = true;
                SetResponseAction(_card.My && RoomHandler.IsOwner, _card.UniqueId);
                return true;
            }
        }

        return false;
    }

    private void SetResponseAction(bool _forMe, string _uniqueCardId)
    {
        BoardData.IdOfCardWithResponseAction = _uniqueCardId;
        if (_forMe)
        {
            if (RoomHandler.IsOwner)
            {
                SetGameplaySubState(GameplaySubState.Player1ResponseAction);
            }
            else
            {
                SetGameplaySubState(GameplaySubState.Player2ResponseAction);
            }
        }
        else
        {
            if (RoomHandler.IsOwner)
            {
                SetGameplaySubState(GameplaySubState.Player2ResponseAction);
            }
            else
            {
                SetGameplaySubState(GameplaySubState.Player1ResponseAction);
            }
        }
    }

    private void CheckIfDefenderIsDestroyed(Card _defendingCard, Action<bool> _callBack)
    {
        if (!(_defendingCard.IsWarrior() || _defendingCard is Wall or Marker))
        {
            _callBack?.Invoke(false);
            return;
        }

        if (_defendingCard.Health > 0)
        {
            _callBack?.Invoke(false);
            return;
        }

        if (_defendingCard is Keeper _keeper)
        {
            if (IsAbilityActive<Risk>())
            {
                if (_keeper.GetIsMy())
                {
                    EndGame(false);
                    return;
                }
                else
                {
                    EndGame(true);
                    return;
                }
            }

            int _maxHealth = _keeper.CardData.Stats.MaxHealth == -1 ? _keeper.Details.Stats.Health : _keeper.CardData.Stats.MaxHealth;
            float _healthToRecover = _maxHealth * _keeper.PercentageOfHealthToRecover / 100;
            int _heal = Mathf.RoundToInt(_healthToRecover + .3f);

            if (IsAbilityActive<Subdued>())
            {
                Subdued _subdued = FindObjectOfType<Subdued>();
                _subdued.TryToCancel();
            }

            OnKeeperDied?.Invoke(_keeper);
            _defendingCard.SetHealth(_heal);
            var _lifeForce = _defendingCard.My ? GetMyLifeForce() : GetOpponentsLifeForce();
            _lifeForce.ChangeHealth(-_defendingCard.Health);

            if (IsAbilityActive<Explode>())
            {
                var _explode = FindObjectOfType<Explode>();
                if (_keeper.GetIsMy() == _explode.IsMy)
                {
                    BombExploded(_keeper.GetTablePlace().Id, false);
                }
            }

            StartCoroutine(PlaceKeeperOnTable(_keeper, FinishPlaceKeeper));
        }
        else
        {
            GameplayPlayer _defendingPlayer = _defendingCard.My ? MyPlayer : OpponentPlayer;
            _defendingPlayer.DestroyCard(_defendingCard);
            _callBack?.Invoke(true);
        }

        void FinishPlaceKeeper()
        {
            if (IsAbilityActive<Comrade>())
            {
                ActivateComrade(_defendingCard, _callBack);
                return;
            }

            _callBack?.Invoke(true);
        }
    }

    private void ActivateComrade(Card _keeper, Action<bool> _callBack)
    {
        Comrade _comrade = FindObjectOfType<Comrade>();

        if (_comrade.IsMy)
        {
            if (_keeper.GetIsMy())
            {
                HandleComrade(_callBack);
            }
        }
        else
        {
            if (!_keeper.GetIsMy())
            {
                var _opponentsDeadMinions = GetDeadMinions(false);
                if (_opponentsDeadMinions == null || _opponentsDeadMinions.Count == 0)
                {
                    _callBack?.Invoke(true);
                    return;
                }

                if (RoomHandler.IsOwner)
                {
                    SetGameplaySubState(GameplaySubState.Player2UseComrade);
                }
                else
                {
                    SetGameplaySubState(GameplaySubState.Player1UseComrade);
                }

                StartCoroutine(WaitForOpponentToUseComrade(_callBack));
                RoomUpdater.Instance.ForceUpdate();
                return;
            }
        }

        _callBack?.Invoke(true);
    }

    private IEnumerator WaitForOpponentToUseComrade(Action<bool> _callBack)
    {
        yield return new WaitUntil(() => GetGameplaySubState() == GetGameplaySubStateHelper());
        _callBack?.Invoke(true);
    }

    public override void HandleComrade(Action<bool> _callBack)
    {
        List<CardBase> _validCards = GetDeadMinions(true).Cast<CardBase>().ToList();

        if (_validCards.Count == 0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have any dead minion to execute Comrade");
            _callBack?.Invoke(true);
            return;
        }

        ChooseCardImagePanel.Instance.Show(_validCards, _selected => { ReviveMinionComrade(_selected, _callBack); }, true, true);
    }

    private void ReviveMinionComrade(CardBase _cardBase, Action<bool> _callBack)
    {
        if (_cardBase == null)
        {
            _callBack?.Invoke(true);
            return;
        }

        BuyMinion(_cardBase, 0, () =>
        {
            _callBack?.Invoke(true);
        });
    }

    private IEnumerator PlaceKeeperOnTable(Keeper _card, Action _callBack)
    {
        bool _waitForSomething = false;
        if (_card.GetIsMy())
        {
            SelectPlaceForKeeper(_card.UniqueId, ContinueWithExecution);
            yield return new WaitUntil(() => _waitForSomething);
        }
        else
        {
            var _currentState = GetGameplaySubState();
            if (RoomHandler.IsOwner)
            {
                SetGameplaySubState(GameplaySubState.Player2UseKeeperReposition);
            }
            else
            {
                SetGameplaySubState(GameplaySubState.Player1UseKeeperReposition);
            }

            RoomUpdater.Instance.ForceUpdate();
            yield return new WaitUntil(() => GetGameplaySubState() == _currentState);
        }
        
        _callBack?.Invoke();

        void ContinueWithExecution()
        {
            _waitForSomething = true;
        }
    }

    public override void SelectPlaceForKeeper(string _cardId, Action _callBack)
    {
        var _card = GetCard(_cardId);
        var _availablePlaces = GetAvailablePlaces(_card.GetTablePlace().Id);
        if (_availablePlaces.Count==1)
        {
            DoPlaceKeeper(_availablePlaces[0].Id);
        }
        else
        {
            StartCoroutine(SelectPlace(_availablePlaces, true, DoPlaceKeeper));
        }

        void DoPlaceKeeper(int _placeId)
        {
            ExecuteMove(_card.GetTablePlace().Id, _placeId, _cardId, _callBack);
        }
    }

    private List<TablePlaceHandler> GetAvailablePlaces(int _startingPlace)
    {
        List<List<int>> _tierLists = new()
        {
            new List<int> { 10, 12 },

            new List<int> { 17 },

            new List<int> { 18, 19 },

            new List<int>
            {
                9,
                13,
                19,
                16,
                23,
                24,
                25,
                26,
                27
            },

            new List<int>
            {
                14,
                8,
                15,
                21,
                28,
                27,
                26,
                25,
                24,
                23,
                22
            },
        };

        foreach (var _tier in _tierLists)
        {
            List<TablePlaceHandler> _emptyPlaces = GetEmptyPlaces(_tier);

            if (_tier.Contains(_startingPlace))
            {
                TablePlaceHandler _currentPlace = TableHandler.GetPlace(_startingPlace);
                _emptyPlaces.Add(_currentPlace);
            }

            if (_emptyPlaces.Count > 0)
            {
                return _emptyPlaces;
            }
        }

        List<int> _tier6Ids = Enumerable.Range(8, 56 - 8 + 1).ToList();
        List<TablePlaceHandler> _tier6Places = GetEmptyPlaces(_tier6Ids);

        // If _startingPlace is within 8..56, it belongs to tier 6, so include it
        if (_tier6Ids.Contains(_startingPlace))
        {
            _tier6Places.Add(TableHandler.GetPlace(_startingPlace));
        }

        return _tier6Places;


        List<TablePlaceHandler> GetEmptyPlaces(List<int> _placeIds)
        {
            List<TablePlaceHandler> _results = new List<TablePlaceHandler>();
            foreach (int _placeId in _placeIds)
            {
                TablePlaceHandler _place = TableHandler.GetPlace(_placeId);
                // Add only if NOT occupied
                if (!_place.IsOccupied)
                {
                    _results.Add(_place);
                }
            }

            return _results;
        }
    }


    private void HandleLoot(Card _attackingCard, Card _defendingCard, int _placeOfDefendingCard)
    {
        int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChangeForRoomOwner() : LootChangeForOther();
        bool _didIAttack = _attackingCard.GetIsMy();
        int _amount = _additionalMatter;
        _amount += GetStrangeMatterForCard(_defendingCard);

        AddStrangeMatter(_amount, _didIAttack, _placeOfDefendingCard);
    }

    private void AddStrangeMatter(int _amount, bool _forMe, int _placeOfDefendingCard)
    {
        if (_forMe)
        {
            BoardData.MyPlayer.StrangeMatter += _amount;
        }
        else
        {
            BoardData.OpponentPlayer.StrangeMatter += _amount;
        }

        AnimateStrangeMatter(_amount, _forMe, _placeOfDefendingCard);
        BoardData.StrangeMatterAnimation = new StrangeMatterAnimation
        {
            Id = Guid.NewGuid().ToString(), Amount = _amount, ForMe = _forMe, PositionId = _placeOfDefendingCard,
        };
    }

    public override void AnimateStrangeMatter(int _amount, bool _forMe, int _placeOfDefendingCard)
    {
        var _startingPosition = TableHandler.GetPlace(_placeOfDefendingCard).transform.position;
        for (int _i = 0; _i < _amount; _i++)
        {
            EconomyPanelHandler.Instance.ShowBoughtMatter(_forMe, _startingPosition);
        }
    }

    private int LootChangeForRoomOwner()
    {
        return RoomHandler.IsOwner ? BoardData.MyPlayer.LootChange : BoardData.OpponentPlayer.LootChange;
    }

    private int LootChangeForOther()
    {
        return RoomHandler.IsOwner ? BoardData.OpponentPlayer.LootChange : BoardData.MyPlayer.LootChange;
    }

    private IEnumerator GetPlaceOnTable(Action<int> _callBack, bool _wholeTable = false)
    {
        List<TablePlaceHandler> _availablePlaces = new List<TablePlaceHandler>();
        List<int> _lifeForceRow = new List<int>()
        {
            8,
            9,
            10,
            11,
            12,
            13,
            14
        };
        List<int> _keepersRow = new List<int>()
        {
            15,
            16,
            17,
            18,
            19,
            20,
            21
        };
        List<int> _thirdRow = new List<int>()
        {
            22,
            23,
            24,
            25,
            26,
            27,
            28
        };
        List<int> _wallRow = new List<int>()
        {
            29,
            30,
            31,
            32,
            33,
            34,
            35
        };
        List<int> _fourthRow = new List<int>()
        {
            36,
            37,
            38,
            39,
            40,
            41,
            42
        };
        List<int> _opponentKeepersRow = new List<int>()
        {
            43,
            44,
            45,
            46,
            47,
            48,
            49
        };
        List<int> _opponentLifeForceRow = new List<int>()
        {
            50,
            51,
            52,
            53,
            54,
            55,
            56
        };


        if (_wholeTable)
        {
            CheckForEmptyPlace(_lifeForceRow);
            CheckForEmptyPlace(_keepersRow);
            CheckForEmptyPlace(_thirdRow);
            CheckForEmptyPlace(_wallRow);
            CheckForEmptyPlace(_fourthRow);
            CheckForEmptyPlace(_opponentKeepersRow);
            CheckForEmptyPlace(_opponentLifeForceRow);
        }
        else
        {
            CheckForEmptyPlace(_lifeForceRow);
            CheckForEmptyPlace(_keepersRow);
            CheckForEmptyPlace(_thirdRow);
            CheckForEmptyPlace(_wallRow);
            CheckForEmptyPlace(_fourthRow);
        }

        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        CardTableInteractions.OnPlaceClicked += DoSelectPlace;
        bool _hasSelectedPlace = false;
        int _selectedPlaceId = 0;

        yield return new WaitUntil(() => _hasSelectedPlace);

        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.white);
        }

        _callBack?.Invoke(_selectedPlaceId);

        void DoSelectPlace(TablePlaceHandler _place)
        {
            if (!_availablePlaces.Contains(_place))
            {
                return;
            }

            CardTableInteractions.OnPlaceClicked -= DoSelectPlace;
            _selectedPlaceId = _place.Id;
            _hasSelectedPlace = true;
        }

        void CheckForEmptyPlace(List<int> _places)
        {
            if (_availablePlaces.Count != 0 && !_wholeTable)
            {
                return;
            }

            for (int _i = 0; _i < _places.Count; _i++)
            {
                TablePlaceHandler _place = TableHandler.GetPlace(_places[_i]);
                if (_place.IsOccupied)
                {
                    continue;
                }

                _availablePlaces.Add(_place);
            }
        }
    }

    private void ReplaceKeeper(CardBase _card, int _placeId, Action _callBack)
    {
        PlaceCard(_card, _placeId);
        _callBack?.Invoke();
    }

    public override void EndTurn()
    {
        CloseAllPanels();
        TableHandler.ActionsHandler.ClearPossibleActions();

        if (IsResponseAction())
        {
            if (!IsMyResponseAction())
            {
                return;
            }

            SetGameplaySubState(GameplaySubState.Playing);
            RoomUpdater.Instance.ForceUpdate();
            return;
        }

        if (!IsMyTurn())
        {
            return;
        }

        DidIFinishMyTurn = true;
        SetPlayersTurn(false);
        SetAmountOfActions(0, true);
        SetAmountOfActions(3, false);

        FirebaseNotificationHandler.Instance.SendNotificationToUser(RoomHandler.GetOpponent().Id, "Your turn!", "Come back to game!");
        AudioManager.Instance.PlaySoundEffect("EndTurn");
        RoomUpdater.Instance.ForceUpdate();
    }

    public override void BuyMinion(CardBase _cardBase, int _cost, Action _callBack = null, int _placeId = -1)
    {
        string _cardId = ((Card)_cardBase).UniqueId;
        StartCoroutine(SelectPlaceRoutine());

        IEnumerator SelectPlaceRoutine()
        {
            if (_placeId == -1)
            {
                yield return StartCoroutine(GetPlaceOnTable(FinishRevive));
            }
            else
            {
                FinishRevive(_placeId);
            }

            void FinishRevive(int _positionId)
            {
                HandleBoughtMinion(_positionId);
                if (_cost > 0)
                {
                    MyPlayer.Actions--;
                    if (MyPlayer.Actions > 0)
                    {
                        RoomUpdater.Instance.ForceUpdate();
                    }
                }

                _callBack?.Invoke();
            }
        }

        void HandleBoughtMinion(int _positionId)
        {
            ChangeMyStrangeMatter(-_cost);
            ChangeStrangeMaterInEconomy(_cost);
            (_cardBase as Card)?.SetHasDied(false);
            PlaceCard(_cardId, _positionId);
        }
    }

    public override void BuildWall(CardBase _cardBase, int _cost, Action _callBack)
    {
        string _cardId = ((Card)_cardBase).UniqueId;
        StartCoroutine(SelectPlaceRoutine());

        IEnumerator SelectPlaceRoutine()
        {
            yield return StartCoroutine(GetPlaceOnTable(FinishRevive, true));

            void FinishRevive(int _positionId)
            {
                ChangeMyStrangeMatter(-_cost);
                ChangeStrangeMaterInEconomy(_cost);
                PlaceCard(_cardId, _positionId);

                if (_cost > 0)
                {
                    MyPlayer.Actions--;

                    if (MyPlayer.Actions > 0)
                    {
                        RoomUpdater.Instance.ForceUpdate();
                    }
                }

                _callBack?.Invoke();
            }
        }
    }

    public override void TellOpponentSomething(string _text)
    {
        BoardData.SaySomethingData = new SaySomethingData { Id = Guid.NewGuid().ToString(), Message = _text };
    }

    public override void ChangeOwnerOfCard(string _cardId)
    {
        Card _card = GetAllCards().Find(_card => _card.UniqueId == _cardId);
        if (_card == null)
        {
            return;
        }

        _card.ChangeOwner();
    }

    public override void BombExploded(int _placeId, bool _includeSelf)
    {
        int _attackingPlaceId = _placeId;
        List<Card> _availablePlaces = TableHandler.GetAttackableCards(_attackingPlaceId,
            CardMovementType.EightDirections, _includeCenter: _includeSelf);

        foreach (var _cardOnPlace in _availablePlaces.ToList())
        {
            DamageCardByAbility(_cardOnPlace.UniqueId, 3, _ => { HideCardActions(); });
        }

        BombAnimation _animation = new BombAnimation { Id = Guid.NewGuid().ToString(), PlaceId = _placeId };
        BoardData.BombAnimation = _animation;
        ShowBombAnimation(_placeId);
    }

    public override void ShowBombAnimation(int _placeId)
    {
        StartCoroutine(ShowBombEffect(_placeId));
    }

    private IEnumerator ShowBombEffect(int _placeId)
    {
        TablePlaceHandler _tablePlace = TableHandler.GetPlace(_placeId);
        GameObject _bombEffect = Instantiate(bombEffect, GameplayUI.Instance.transform);
        yield return null;
        _bombEffect.transform.position = _tablePlace.transform.position;
        yield return new WaitForSeconds(2);
        Destroy(_bombEffect);
    }

    public override void ActivateAbility(string _cardId)
    {
        if (IsAbilityActive<Tax>())
        {
            var _taxedCard = FindObjectOfType<Tax>();
            if (_taxedCard.IsCardEffected(_cardId))
            {
                if (MyStrangeMatter()<=0)
                {
                    DialogsManager.Instance.ShowOkDialog("You don't have enough strange matter to pay Tax");
                    return;
                }

                ChangeMyStrangeMatter(-1);
                ChangeOpponentsStrangeMatter(1);
            }
        }
        
        
        AbilityCard _ability = FindObjectsOfType<AbilityCard>().ToList().Find(_ability => _ability.UniqueId == _cardId);

        if (_ability.GetTablePlace() == null)
        {
            TableHandler.GetAbilityPosition(PlaceAbility);
        }
        else
        {
            HandleActivateAbility();
        }

        void PlaceAbility(int _placeId)
        {
            if (_placeId == -1)
            {
                DialogsManager.Instance.ShowOkDialog("There are no empty spaces in ability row");
                return;
            }

            HandleActivateAbility();
        }

        void HandleActivateAbility()
        {
            _ability.Activate();
            AudioManager.Instance.PlaySoundEffect("AbilityCardUsed");
        }
    }

    public override void BuyAbility(string _abilityId)
    {
        AbilityData _ability = AbilityCardsManagerBase.Instance.RemoveAbilityFromShop(_abilityId);
        if (_ability == null)
        {
            return;
        }

        Debug.Log("Buying ability");
        _ability.Owner = FirebaseManager.Instance.PlayerId;
        ChangeAmountOfAbilitiesICanBuy(-1);
        AudioManager.Instance.PlaySoundEffect("AbilityCardPurchased");
        MyPlayer.ActivateAbility(_abilityId);
        if (_ability.CardId == 1069)
        {
            //dont remove strange matter when portal is purchased
            RoomUpdater.Instance.ForceUpdate();
            return;
        }

        if (_ability.CardId == 1031)
        {
            //dont remove strange matter when void is purchased
            RoomUpdater.Instance.ForceUpdate();
            return;
        }        
        
        if (_ability.CardId == 1068)
        {
            RoomUpdater.Instance.ForceUpdate();
            return;
        }    
        
        if (_ability.CardId == 1060)
        {
            RoomUpdater.Instance.ForceUpdate();
            return;
        }

        MyPlayer.Actions--;
        if (MyPlayer.Actions > 0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    public override void PushCard(int _startingPlace, int _endingPlace, int _chanceForPush = 100)
    {
        Card _pushedCard = TableHandler.GetPlace(_startingPlace).GetWarrior() as Card;
        if (_pushedCard == null)
        {
            return;
        }

        if (!_pushedCard.IsWarrior())
        {
            return;
        }

        if (UnityEngine.Random.Range(0, 100) > _chanceForPush)
        {
            return;
        }

        TablePlaceHandler _pushedCardPlace = _pushedCard.GetTablePlace();
        Vector2 _indexBehindOfPushedCard = TableHandler.GetBehindIndex(_startingPlace, _endingPlace);
        TablePlaceHandler _placeBehindOfPushedCard = TableHandler.GetPlace(_indexBehindOfPushedCard);
        if (_placeBehindOfPushedCard == default)
        {
            DamageCardByAbility(_pushedCard.UniqueId, 1, null);
            return;
        }

        if (!_placeBehindOfPushedCard.IsOccupied)
        {
            Card _pushedCardBase = _pushedCardPlace.GetCard();
            if (!_pushedCardBase.CheckCanMove() || _placeBehindOfPushedCard.IsAbility)
            {
                DamageCardByAbility(_pushedCard.UniqueId, 1, null);
                return;
            }

            ExecuteMove(_pushedCardPlace.Id, _placeBehindOfPushedCard.Id, _pushedCard.UniqueId, null);
            return;
        }

        DamageCardByAbility(_pushedCard.UniqueId, 1, null);
    }

    public override void PlaceAbilityOnTable(string _abilityId)
    {
        TableHandler.GetAbilityPosition(_placeId => { PlaceAbilityOnTable(_abilityId, _placeId); });

    }

    public override void PlaceAbilityOnTable(string _abilityId, int _placeId)
    {
        AbilityCard _ability = GetAbilityCard(_abilityId);
        _ability.Data.PlaceId = _placeId;
        ShowAbilityOnTable(_abilityId, _placeId);
    }

    public override void ShowAbilityOnTable(string _abilityId, int _placeId)
    {
        AbilityCard _ability = GetAbility(_abilityId);
        TablePlaceHandler _tablePlace = TableHandler.GetPlace(_placeId);
        _ability.PositionOnTable(_tablePlace);
    }

    public override void ReturnAbilityFromActivationField(string _abilityId)
    {
        PlaceAbilityOnTable(_abilityId);
        MyPlayer.Actions--;
        if (MyPlayer.Actions > 0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    public override void BuyMatter()
    {
        int _amount = 1;
        ChangeMyStrangeMatter(_amount);
        ChangeStrangeMaterInEconomy(-_amount);
        BoardData.BoughtStrangeMatterAnimation =
            new BoughtStrangeMatterAnimation { Id = Guid.NewGuid().ToString(), DidOwnerBuy = RoomHandler.IsOwner };

        MyPlayer.Actions--;
        ShowBoughtMatter(true);
        if (MyPlayer.Actions > 0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    public override void ShowBoughtMatter(bool _didIBuy)
    {
        EconomyPanelHandler.Instance.ShowBoughtMatter(true);
    }

    public override void DestroyBombWithoutActivatingIt(int _cardId, bool _isMy)
    {
        Card _bomber = FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id == _cardId && _card.My == _isMy);
        GameplayPlayer _player = _bomber.My ? MyPlayer : OpponentPlayer;
        _player.DestroyWithoutNotify(_bomber);
    }

    public override void ChangeSprite(string _cardId, int _spriteId, bool _showPlaceAnimation = false)
    {
        Card _card = GetCard(_cardId);
        if (_card == null)
        {
            return;
        }

        Sprite _sprite = GetMarkerSprite(_spriteId);
        
        if (_card.IsVoid && _sprite != voidMarker)
        {
            return;
        }

        BoardData.ChangeSpriteData.Add(new ChangeSpriteData
        {
            Id = Guid.NewGuid().ToString(), CardId = _cardId, SpriteId = _spriteId, ShowPlaceAnimation = _showPlaceAnimation
        });
        
        ChangeSpriteAnimate(_cardId,_spriteId,_showPlaceAnimation);
    }

    public override void ChangeSpriteAnimate(string _uniqueId, int _spriteId, bool _showPlaceAnimation)
    {
        Card _card = GetCard(_uniqueId);
        bool _changedSprite = _card.Display.ChangeSprite(GetMarkerSprite(_spriteId));
        if (!_showPlaceAnimation || !_changedSprite)
        {
            return;
        }

        _card.transform.localPosition = new Vector3(-2000, 0);
        IsAnimating = true;
        _card.MoveToPosition(_card.GetTablePlace(), () =>
        {
            IsAnimating = false;
        });
    }

    private Sprite GetMarkerSprite(int _spriteId)
    {
        switch (_spriteId)
        {
            case 0:
                return voidMarker;
            case 1:
                return snowMarker;
            case 2:
                return cyborgMarker;
            case 3:
                return dragonMarker;
            case 4:
                return forestMarker;
            default:
                if (_spriteId<0)
                {
                    return FactionSO.Get(_spriteId * -1).BombSprite;
                }
                return null;
        }
    }

    public override void PlayAudioOnBoth(string _key, CardBase _cardBase)
    {
        if (string.IsNullOrEmpty(_key))
        {
            return;
        }

        Card _card = _cardBase as Card;

        if (AudioManager.Instance.IsPlayingSoundEffect)
        {
            return;
        }

        BoardData.SoundAnimation = new SoundAnimation { Id = Guid.NewGuid().ToString(), Key = _key, CardId = _card.UniqueId };
        AnimateSoundEffect(BoardData.SoundAnimation.Key, BoardData.SoundAnimation.Id);
    }

    public override void AnimateSoundEffect(string _key, string _cardUniqueId)
    {
        Card _card = GetCard(_cardUniqueId);
        if (_card != null)
        {
            _card.Display.ShowWhiteBox();
        }

        AudioManager.Instance.PlaySoundEffect(_key);
    }

    public override int StrangeMaterInEconomy()
    {
        return BoardData.StrangeMaterInEconomy;
    }

    public override void ChangeStrangeMaterInEconomy(int _amount)
    {
        BoardData.StrangeMaterInEconomy += _amount;
    }

    public override int StrangeMatterCostChange(bool _forMe)
    {
        return _forMe ? BoardData.MyPlayer.StrangeMatterCostChange : BoardData.OpponentPlayer.StrangeMatterCostChange;
    }

    public override void ChangeStrangeMatterCostChange(int _amount, bool _forMe)
    {
        if (_forMe)
        {
            BoardData.MyPlayer.StrangeMatterCostChange += _amount;
        }
        else
        {
            BoardData.OpponentPlayer.StrangeMatterCostChange += _amount;
        }
    }

    public override string IdOfCardWithResponseAction()
    {
        return BoardData.IdOfCardWithResponseAction;
    }

    private void SetIdOfCardWithResponseAction(string _cardId)
    {
        BoardData.IdOfCardWithResponseAction = _cardId;
    }

    public override void ChangeLootAmountForMe(int _amount)
    {
        BoardData.MyPlayer.LootChange += _amount;
    }

    public override int MyStrangeMatter()
    {
        return BoardData.MyPlayer.StrangeMatter;
    }

    public override int OpponentsStrangeMatter()
    {
        return BoardData.OpponentPlayer.StrangeMatter;
    }

    public override void ChangeMyStrangeMatter(int _amount)
    {
        BoardData.MyPlayer.StrangeMatter += _amount;
    }

    public override void ChangeOpponentsStrangeMatter(int _amount)
    {
        BoardData.OpponentPlayer.StrangeMatter += _amount;
    }

    public override int AmountOfAbilitiesPlayerCanBuy()
    {
        return BoardData.MyPlayer.AmountOfAbilitiesPlayerCanBuy;
    }

    public override void ChangeAmountOfAbilitiesICanBuy(int _amount)
    {
        BoardData.MyPlayer.AmountOfAbilitiesPlayerCanBuy += _amount;
    }

    public override List<Card> GetAllCardsOfType(CardType _type, bool _forMe)
    {
        return FindObjectsOfType<Card>().ToList().FindAll(_card => _card.Details.Type == _type && _card.GetIsMy() == _forMe);
    }

    public override void AddCard(CardData _cardData)
    {
        BoardData.Cards.Add(_cardData);
    }

    public override void AddAbility(AbilityData _abilityData, bool _forMe)
    {
        BoardData.Abilities.Add(_abilityData);
    }

    public override AbilityCard GetAbility(string _uniqueId)
    {
        foreach (var _ability in FindObjectsOfType<AbilityCard>())
        {
            if (_ability.UniqueId != _uniqueId)
            {
                continue;
            }

            return _ability;
        }

        return null;
    }

    public override void RemoveAbility(string _uniqueId, bool _forMe)
    {
        AbilityData _cardData = null;
        foreach (var _card in BoardData.Abilities)
        {
            if (_card.UniqueId != _uniqueId)
            {
                continue;
            }

            _cardData = _card;
            break;
        }

        if (_cardData == null)
        {
            return;
        }

        BoardData.Abilities.Remove(_cardData);
    }

    public override Card GetCardOfType(CardType _type, bool _forMe)
    {
        foreach (var _card in FindObjectsOfType<Card>())
        {
            if (_card.Details.Type != _type)
            {
                continue;
            }

            if (_card.GetIsMy() != _forMe)
            {
                continue;
            }

            return _card;
        }

        return null;
    }

    public override List<AbilityData> GetOwnedAbilities(bool _forMe)
    {
        string _playerId = _forMe ? BoardData.MyPlayer.PlayerId : BoardData.OpponentPlayer.PlayerId;
        return BoardData.Abilities.FindAll(_ability => _ability.Owner == _playerId);
    }

    public override Card GetCard(string _uniqueId)
    {
        foreach (var _ability in FindObjectsOfType<Card>())
        {
            if (_ability.UniqueId != _uniqueId)
            {
                continue;
            }

            return _ability;
        }

        return null;
    }

    public override CardPlace GetCardPlace(string _uniqueCardId)
    {
        return GetCard(_uniqueCardId).CardData.CardPlace;
    }

    public override void SetCardPlace(string _uniqueCardId, CardPlace _place)
    {
        GetCard(_uniqueCardId).CardData.CardPlace = _place;
    }

    public override void SetAbilityPlace(string _uniqueId, CardPlace _place)
    {
        GetAbility(_uniqueId).Data.CardPlace = _place;
    }

    public override CardPlace GetCardPlace(CardBase _cardBase)
    {
        CardPlace _cardPlace = CardPlace.Deck;
        if (_cardBase is Card _cardCard)
        {
            _cardPlace = _cardCard.CardData.CardPlace;
        }
        else if (_cardBase is AbilityCard _ability)
        {
            _cardPlace = _ability.Data.CardPlace;
        }

        return _cardPlace;
    }

    public override void TryUnchainGuardian()
    {
        if (!CanPlayerDoActions())
        {
            return;
        }
        
        if (!IsMyTurn())
        {
            return;
        }

        if (!GetMyGuardian().IsChained)
        {
            DialogsManager.Instance.ShowOkDialog("Guardian is already unchained");
            return;
        }

        if (IsAbilityActive<Famine>())
        {
            DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
            return;
        }

        int _price = BoardData.UnchainingGuardianPrice - StrangeMatterCostChange(true);

        if (MyStrangeMatter() < _price)
        {
            DialogsManager.Instance.ShowOkDialog($"You don't have enough strange matter, this action requires {_price}");
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog($"Spend {_price} to unchain guardian??", () => { YesUnchain(_price); });
    }

    private void YesUnchain(int _price)
    {
        UnchainGuardian(_price, true);
    }

    public override int AmountOfActionsPerTurn()
    {
        return BoardData.AmountOfActionsPerTurn;
    }

    public override bool IsSettingUpTable()
    {
        return RoomData.GameplayState is GameplayState.SettingUpTable or GameplayState.WaitingForPlayersToLoad;
    }

    public override void SetGameState(GameplayState _gameState)
    {
        RoomData.GameplayState = _gameState;
    }

    public override GameplayState GameState()
    {
        return RoomData.GameplayState;
    }

    public override void SetHasGameEnded(bool _status, string _winner)
    {
        RoomData.HasGameEnded = _status;
        RoomData.Winner = _winner;
    }

    public override bool HasGameEnded()
    {
        return RoomData.HasGameEnded;
    }

    public override bool IsMyTurn()
    {
        return RoomData.IsMyTurn;
    }

    public override void SetPlayersTurn(bool _isMyTurn)
    {
        if (RoomHandler.IsOwner)
        {
            RoomData.CurrentPlayerTurn = _isMyTurn ? 1 : 2;
        }
        else
        {
            RoomData.CurrentPlayerTurn = _isMyTurn ? 2 : 1;
        }
    }

    public override bool ShouldIPlaceStartingWall()
    {
        return !IsMyTurn();
    }

    public override void SetGameplaySubState(GameplaySubState _subState)
    {
        RoomData.GameplaySubState = _subState;
    }

    public override GameplaySubState GetGameplaySubState()
    {
        return RoomData.GameplaySubState;
    }

    protected override IEnumerator PlaceLifeForceAndGuardianRoutine()
    {
        SetGameplaySubState(GameplaySubState.Player1PlacingLifeForce);
        if (IsMyTurn())
        {
            yield return PlaceLifeForceAndGuardian();
            SetGameplaySubState(GameplaySubState.Player2PlacingLifeForce);
            RoomUpdater.Instance.ForceUpdate();
            yield return new WaitUntil(() => GetGameplaySubState() == GameplaySubState.FinishedPlacingStartingLifeForce);
        }
        else
        {
            yield return new WaitUntil(() => GetGameplaySubState() == GameplaySubState.Player2PlacingLifeForce);
            yield return PlaceLifeForceAndGuardian();
            SetGameplaySubState(GameplaySubState.FinishedPlacingStartingLifeForce);
            RoomUpdater.Instance.ForceUpdate();
        }

        yield return new WaitForSeconds(1);
    }

    private IEnumerator PlaceLifeForceAndGuardian()
    {
        Card _lifeForce = FindObjectsOfType<LifeForce>().ToList().Find(_lifeForce => _lifeForce.My);
        Card _guardianCard = FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My);
        PlaceCard(_lifeForce, 11);
        yield return new WaitForSeconds(0.4f);
        PlaceCard(_guardianCard, 18);
        MyPlayer.HideCards();
    }

    protected override IEnumerator PlaceMinions()
    {
        SetGameplaySubState(GameplaySubState.Player1SelectMinions);
        if (IsMyTurn())
        {
            yield return PlaceRestOfStartingCards();
            SetGameplaySubState(GameplaySubState.Player2SelectMinions);
            RoomUpdater.Instance.ForceUpdate();
            yield return new WaitUntil(() => GetGameplaySubState() == GameplaySubState.FinishedSelectingMinions);
        }
        else
        {
            yield return new WaitUntil(() => GetGameplaySubState() == GameplaySubState.Player2SelectMinions);
            yield return PlaceRestOfStartingCards();
            SetGameplaySubState(GameplaySubState.FinishedSelectingMinions);
            RoomUpdater.Instance.ForceUpdate();
        }

        SetGameplaySubState(GameplaySubState.Playing);
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator PlaceRestOfStartingCards()
    {
        yield return PlaceKeeper();
        DialogsManager.Instance.ShowOkBigDialog(
            "Now pick your minions to go into battle alongside you. Each minion has their own attributes and abilities. You can hold down on any card anytime to zoom in on that card and then you can tap that card to flip it over to see more details.");
        yield return RequestCardToBePlaced(14, CardType.Minion);
        yield return RequestCardToBePlaced(13, CardType.Minion);
        yield return RequestCardToBePlaced(12, CardType.Minion);
        yield return RequestCardToBePlaced(10, CardType.Minion);
        yield return RequestCardToBePlaced(9, CardType.Minion);
        yield return RequestCardToBePlaced(8, CardType.Minion);
        MyPlayer.HideCards();

        IEnumerator PlaceKeeper()
        {
            DialogsManager.Instance.ShowOkDialog("Select which side of your Life force that you, the Keeper, will start.");
            List<TablePlaceHandler> _availablePlaces = new List<TablePlaceHandler> { TableHandler.GetPlace(10), TableHandler.GetPlace(12) };

            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.green);
            }

            CardTableInteractions.OnPlaceClicked += DoSelectPlace;
            bool _hasSelectedPlace = false;
            int _selectedPlaceId = 0;

            yield return new WaitUntil(() => _hasSelectedPlace);

            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.white);
            }

            Card _keeperCard = FindObjectsOfType<Keeper>().ToList().Find(_guardian => _guardian.My);
            PlaceCard(_keeperCard, _selectedPlaceId);

            void DoSelectPlace(TablePlaceHandler _place)
            {
                if (!_availablePlaces.Contains(_place))
                {
                    return;
                }

                CardTableInteractions.OnPlaceClicked -= DoSelectPlace;
                _selectedPlaceId = _place.Id;
                _hasSelectedPlace = true;
            }
        }

        IEnumerator RequestCardToBePlaced(int _placeId, CardType _type)
        {
            bool _hasSelectedCard = false;
            Card _selectedCard = null;

            TablePlaceHandler _place = TableHandler.GetPlace(_placeId);
            if (_place.IsOccupied)
            {
                yield break;
            }

            _place.SetColor(Color.green);

            CardInHandDisplay.OnClicked += SelectCard;
            MyPlayer.ShowCards(_type);

            yield return new WaitUntil(() => _hasSelectedCard);

            _place.SetColor(Color.white);

            PlaceCard(_selectedCard, _placeId);

            void SelectCard(CardBase _card)
            {
                CardInHandDisplay.OnClicked -= SelectCard;
                _selectedCard = (_card as Card);
                _hasSelectedCard = true;
            }
        }
    }

    public override void PlaceStartingWall()
    {
        PlaceStartingWall(29);
        PlaceStartingWall(30);
        PlaceStartingWall(31);
        PlaceStartingWall(32);
        PlaceStartingWall(33);
        PlaceStartingWall(34);
        PlaceStartingWall(35);
        RoomUpdater.Instance.ForceUpdate();
    }

    private void PlaceStartingWall(int _positionId)
    {
        TablePlaceHandler _tablePlaceHandler = TableHandler.GetPlace(_positionId);
        if (_tablePlaceHandler.IsOccupied)
        {
            return;
        }

        Card _selectedCard = GetCardOfTypeNotPlaced(CardType.Wall, true);
        PlaceCard(_selectedCard, _positionId);
    }

    public override Card GetCardOfTypeNotPlaced(CardType _type, bool _forMe)
    {
        foreach (var _cardData in BoardData.Cards)
        {
            if (_cardData.IsMy != _forMe)
            {
                continue;
            }

            if (_cardData.PlaceId != -100)
            {
                continue;
            }

            Card _card = GetCard(_cardData.UniqueId);

            if (_card.Details.Type != _type)
            {
                continue;
            }

            if (_type == CardType.Marker)
            {
                _card.SetIsVoid(false);
            }

            return _card;
        }

        return null;
    }

    public override int AmountOfActions(bool _forMe)
    {
        int _actions = _forMe ? BoardData.MyPlayer.ActionsLeft : BoardData.OpponentPlayer.ActionsLeft;
        return _actions;
    }

    public override void SetAmountOfActions(int _amount, bool _forMe)
    {
        if (_amount < 0)
        {
            _amount = 0;
        }
        else if (_amount > 5)
        {
            _amount = 5;
        }

        if (_forMe)
        {
            BoardData.MyPlayer.ActionsLeft = _amount;
        }
        else
        {
            BoardData.OpponentPlayer.ActionsLeft = _amount;
        }
    }

    public override void ShowCardAsDead(string _uniqueId)
    {
        Card _card = GetCard(_uniqueId);
        _card.PositionAsDead();
    }

    public override void ShowGameEnded(string _winner)
    {
        DataManager.Instance.PlayerData.CurrentRoomId = string.Empty;
        StopAllCoroutines();
        GameplayUI.Instance.ShowResult(_winner == FirebaseManager.Instance.PlayerId);
    }

    public override void UnchainGuardian(int _price, bool _reduceAction)
    {
        BoardData.MyPlayer.DidUnchainGuardian = true;
        ChangeMyStrangeMatter(-_price);
        ChangeStrangeMaterInEconomy(_price);
        ShowGuardianUnchained(true);
        if (_reduceAction)
        {
            MyPlayer.Actions--;
        }

        OnUnchainedGuardian?.Invoke();
        if (MyPlayer.Actions > 0 && _reduceAction)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    public override void UnchainOpponentsGuardian()
    {
        BoardData.OpponentPlayer.DidUnchainGuardian = true;
        ShowGuardianUnchained(false);
        OnUnchainedGuardian?.Invoke();
    }

    public override void ShowGuardianUnchained(bool _didIUnchain)
    {
        Guardian _guardian = _didIUnchain ? GetMyGuardian() : GetOpponentGuardian();
        _guardian.ShowUnchain();
    }

    public override bool DidUnchainGuardian(bool _forMe)
    {
        return _forMe ? BoardData.MyPlayer.DidUnchainGuardian : BoardData.OpponentPlayer.DidUnchainGuardian;
    }

    public override bool IsMyResponseAction()
    {
        return RoomHandler.IsOwner
            ? GetGameplaySubState() == GameplaySubState.Player1ResponseAction
            : GetGameplaySubState() == GameplaySubState.Player2ResponseAction;
    }

    public override bool IsResponseAction()
    {
        return GetGameplaySubState() is GameplaySubState.Player1ResponseAction or GameplaySubState.Player2ResponseAction;
    }

    public override bool DamageCardByAbility(string _uniqueId, int _damage, Action<bool> _callBack,bool _checkForResponse = false, string _attacker =
            "", bool _applyWallEffects = false)
    {
        if (IsAbilityActive<HighStakes>())
        {
            HighStakes _highStakes = FindObjectOfType<HighStakes>();
            _highStakes.TryToCancel();
            _damage = 8;
        }

        Card _defendingCard = GetCard(_uniqueId);
        int _defenderPlace = -1;
        if (_defendingCard == null)
        {
            return false;
        }

        if (_defendingCard.GetTablePlace())
        {
            _defenderPlace = _defendingCard.GetTablePlace().Id;
        }
        
        _defendingCard.ChangeHealth(-_damage);
        bool _didGiveResponseAction = false;
        if (_checkForResponse)
        {
            Card _attackerCard = GetCard(_attacker);
            _didGiveResponseAction = CheckForResponseAction(_attackerCard, _defendingCard);
        }

        if (_applyWallEffects)
        {
            Card _attackerCard = GetCard(_attacker);
            TryToApplyWallAbility(_defendingCard,_attackerCard,null,_defenderPlace);
        }
        CheckIfDefenderIsDestroyed(_defendingCard, _callBack);

        return _didGiveResponseAction;
    }

    public override void MarkMarkerAsBomb(string _cardId)
    {
        Card _card = GetCard(_cardId);
        if (_card == null)
        {
            return;
        }

        ChangeSprite(_cardId, -MyPlayer.FactionSo.Id);
    }

    public override void SaySomethingToAll(string _text)
    {
        TellOpponentSomething(_text);
        DialogsManager.Instance.ShowOkDialog(_text);
    }

    public override void ManageAbilityActive(string _uniqueId, bool _status)
    {
        AbilityCard _ability = GetAbility(_uniqueId);
        if (_ability == null)
        {
            return;
        }

        _ability.ManageActiveDisplay(_status);
    }

    public override List<AbilityData> GetPurchasedAbilities(bool _forMe)
    {
        return BoardData.Abilities.FindAll(_ability =>
            _forMe ? _ability.Owner == FirebaseManager.Instance.PlayerId : _ability.Owner == FirebaseManager.Instance.OpponentId);
    }

    public override List<Card> GetDeadMinions(bool _forMe)
    {
        var _validCards = GetAllCardsOfType(CardType.Minion, _forMe).ToList().ToList();

        foreach (var _validCard in _validCards.ToList())
        {
            if (_validCard.HasDied)
            {
                continue;
            }

            _validCards.Remove(_validCard);
        }

        return _validCards.ToList();
    }

    public override void SetGameplaySubStateHelper(GameplaySubState _subState)
    {
        RoomData.GameplaySubStateHelper = _subState;
    }

    public override GameplaySubState GetGameplaySubStateHelper()
    {
        return RoomData.GameplaySubStateHelper;
    }

    public override void StartReduction(Action _callBack)
    {
        SetGameplaySubStateHelper(GetGameplaySubState());
        UseReduction(StartSecondPlayersReduction);

        void StartSecondPlayersReduction()
        {
            if (RoomHandler.IsOwner)
            {
                SetGameplaySubState(GameplaySubState.Player2UseReduction);
            }
            else
            {
                SetGameplaySubState(GameplaySubState.Player1UseReduction);
            }
            
            StartCoroutine(WaitForOpponentToUseReduction(_callBack));
            RoomUpdater.Instance.ForceUpdate();
        }
    }
    
    private IEnumerator WaitForOpponentToUseReduction(Action _callBack)
    {
        yield return new WaitUntil(() => GetGameplaySubState() == GetGameplaySubStateHelper());
        _callBack?.Invoke();
    }

    public override void UseReduction(Action _callBack)
    {
        List<Card> _cards = new();
        foreach (var _card in GetAllCards().ToList())
        {
            if (!_card.GetIsMy())
            {
                continue;
            }

            if (!_card.IsWarrior())
            {
                continue;
            }

            _cards.Add(_card);
        }

        List<TablePlaceHandler> _availablePlaces = new();
        foreach (var _card in _cards)
        {
            TablePlaceHandler _tablePlace = _card.GetTablePlace();
            if (_tablePlace == null)
            {
                continue;
            }

            _availablePlaces.Add(_tablePlace);
        }

        if (_availablePlaces.Count == 0)
        {
            DialogsManager.Instance.ShowOkDialog("You dont have warrior to sacrifice");
            _callBack?.Invoke();
            return;
        }

        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        DialogsManager.Instance.ShowOkDialog("Please select warrior to be sacrificed");
        TablePlaceHandler.OnPlaceClicked += TryToPlaceReduction;

        void TryToPlaceReduction(TablePlaceHandler _place)
        {
            if (!_availablePlaces.Contains(_place))
            {
                return;
            }

            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.white);
            }

            TablePlaceHandler.OnPlaceClicked -= TryToPlaceReduction;
            Card _card = _place.GetCardNoWall();
            AddStrangeMatter(GetStrangeMatterForCard(_card),true,_place.Id);
           
            DamageCardByAbility(_card.UniqueId,_card.CardData.Stats.Health, _ =>
            {
                _callBack?.Invoke();
            });
        }
    }

    public override void NoteVetoAnimation(string _uniqueId, bool _isVetoed)
    {
        BoardData.VetoAnimation = new VetoAnimation { Id = Guid.NewGuid().ToString(), CardId = _uniqueId, IsVetoed = _isVetoed };
        ShowVetoAnimation(_uniqueId, _isVetoed);
    }

    public override void ShowVetoAnimation(string _uniqueId, bool _isVetoed)
    {
        AbilityCard _ability = GetAbility(_uniqueId);
        if (_isVetoed)
        {
            _ability.RotateToBeVertical();
        }
        else
        {
            _ability.RotateNormal();
        }
    }

    public override void ClearChangeSpriteData()
    {
        BoardData.ChangeSpriteData.Clear();
    }

    public override bool CanPlayerDoActions()
    {
        var _subState = GetGameplaySubState();
        bool _isOwner = RoomHandler.IsOwner;
        
        if (IsAnimating)
        {
            return false;
        }
        
        if (_isOwner)
        {
            if (_subState == GameplaySubState.Player2DeliveryReposition)
            {
                return false;
            } 
            
            if (_subState == GameplaySubState.Player2UseKeeperReposition)
            {
                return false;
            }  
        }
        else
        {
            if (_subState == GameplaySubState.Player1DeliveryReposition)
            {
                return false;
            }
            
            if (_subState == GameplaySubState.Player1UseKeeperReposition)
            {
                return false;
            }  
        }

        return true;
    }

    public override bool IsRoomOwner()
    {
        return RoomHandler.IsOwner;
    }

    public override bool IsDeliveryReposition()
    {
        if (GetGameplaySubState() is GameplaySubState.Player1DeliveryReposition or GameplaySubState.Player2DeliveryReposition)
        {
            return true;
        }

        return false;
    }


    public override bool HasCardResponseAction(string _uniqueId)
    {
        return IdOfCardWithResponseAction() == _uniqueId;
    }

    public override GameplayState GetGameplayState()
    {
        return RoomData.GameplayState;
    }

    public override bool IsKeeperRepositionAction()
    {
        return GetGameplaySubState() is GameplaySubState.Player1UseKeeperReposition or GameplaySubState.Player2UseKeeperReposition; 
    }
}