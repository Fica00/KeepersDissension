using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using FirebaseMultiplayer.Room;
using Newtonsoft.Json;

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
    }

    private void OnDisable()
    {
        RoomHandler.OnPlayerLeft -= OpponentLeftRoom;
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

    private Card CreateCard(int _cardId, TableSideHandler _tableSideHandler, string _uniqueId, bool _addCard, string _owner)
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

        Debug.Log("Ending game");
        var _winner = _didIWin ? BoardData.MyPlayer.PlayerId : BoardData.OpponentPlayer.PlayerId;
        SetHasGameEnded(true,_winner);
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
            Debug.Log($"Didn't manage to find card with id {_uniqueId}");
            return;
        }

        _card.PositionOnTable(TableHandler.GetPlace(_positionId));
        OnPlacedCard?.Invoke(_card);
    }

    public override void ShowCardMoved(string _uniqueId, int _positionId)
    {
        Card _card = GetCard(_uniqueId);
        if (_card == null)
        {
            Debug.Log($"Didn't manage to find card with id {_uniqueId}");
            return;
        }

        var _destination = TableHandler.GetPlace(_positionId);
        if (_destination==null)
        {
            return;
        }

        _card.MoveToPosition(_destination);
    }

    public override void AddAbilityToPlayer(bool _isMyPlayer, string _abilityId)
    {
        var _player = _isMyPlayer
            ? FirebaseManager.Instance.RoomHandler.BoardData.MyPlayer
            : FirebaseManager.Instance.RoomHandler.BoardData.OpponentPlayer;
        var _abilityData = FirebaseManager.Instance.RoomHandler.BoardData.AvailableAbilities.Find(_ability => _ability.UniqueId == _abilityId);
        if (_abilityData == null)
        {
            return;
        }

        _abilityData.Owner = _player.PlayerId;
        BoardData.Abilities.Add(_abilityData);
        BoardData.AvailableAbilities.Remove(_abilityData);
    }

    public override void AddAbilityToShop(string _abilityId)
    {
        var _abilityData = FirebaseManager.Instance.RoomHandler.BoardData.AvailableAbilities.Find(_ability => _ability.UniqueId == _abilityId);
        if (_abilityData == null)
        {
            return;
        }

        FirebaseManager.Instance.RoomHandler.BoardData.AvailableAbilities.Remove(_abilityData);
        FirebaseManager.Instance.RoomHandler.BoardData.AbilitiesInShop.Add(_abilityData);
    }

    protected override void ExecuteMove(CardAction _action, Action _callBack)
    {
        TablePlaceHandler _destination = TableHandler.GetPlace(_action.FinishingPlaceId);
        Card _movingCard = GetCard(_action.FirstCardId);

        if (_movingCard == null)
        {
            _callBack?.Invoke();
            return;
        }

        if (_action.ResetSpeed)
        {
            _movingCard.SetSpeed(0);
        }

        if (_destination.ContainsMarker)
        {
            Card _marker = _destination.GetMarker();
            if (!_marker.IsVoid)
            {
                CardBase _cardBase = _destination.GetCardNoWall();
                GameplayPlayer _markerOwner = _cardBase.GetIsMy() ? MyPlayer : OpponentPlayer;
                _markerOwner.DestroyCard(_cardBase as Card);
            }
        }

        _movingCard.CardData.PlaceId = _destination.Id;

        ShowCardMoved(_movingCard.UniqueId, _destination.Id);

        OnCardMoved?.Invoke(_movingCard, _action.StartingPlaceId, _action.FinishingPlaceId, _action.DidTeleport);
        PlayMovingSoundEffect(_movingCard);
        _callBack?.Invoke();
    }

    protected override void ExecuteSwitchPlace(CardAction _action, Action _callBack)
    {
        TablePlaceHandler _startingDestination = TableHandler.GetPlace(_action.StartingPlaceId);
        TablePlaceHandler _destination = TableHandler.GetPlace(_action.FinishingPlaceId);

        Card _firstCard = GetCard(_action.FirstCardId);
        Card _secondCard = GetCard(_action.SecondCardId);

        if (_firstCard == null || _secondCard == null)
        {
            _callBack?.Invoke();
            return;
        }

        _firstCard.CardData.PlaceId = _destination.Id;
        _secondCard.CardData.PlaceId = _startingDestination.Id;

        ShowCardMoved(_firstCard.UniqueId, _destination.Id);
        ShowCardMoved(_secondCard.UniqueId, _startingDestination.Id);

        OnSwitchedPlace?.Invoke(_firstCard, _secondCard);
        _callBack?.Invoke();
    }

    protected override void ExecuteMoveAbility(CardAction _action, Action _callBack)
    {
        TablePlaceHandler _destination = TableHandler.GetPlace(_action.FinishingPlaceId);
        CardBase _movingCard = GetCard(_action.FirstCardId);

        if (_movingCard == null)
        {
            _callBack?.Invoke();
            return;
        }

        if (_destination.ContainsMarker)
        {
            CardBase _cardBase = _destination.GetCardNoWall();
            GameplayPlayer _markerOwner = _cardBase.GetIsMy() ? MyPlayer : OpponentPlayer;
            _markerOwner.DestroyCard(_cardBase as Card);
        }

        _movingCard.MoveToPosition(_destination);
        _callBack?.Invoke();
    }

    protected override void ExecuteAttack(CardAction _action,Action _callBack)
    {
        Card _attackingCard = GetCard(_action.FirstCardId);
        Card _defendingCard = GetCard(_action.SecondCardId);

        if (_attackingCard == null || _defendingCard == null)
        {
            Debug.Log(_attackingCard);
            Debug.Log(_defendingCard);
            _callBack?.Invoke();
            return;
        }
        
        AudioManager.Instance.PlaySoundEffect("Attack");

        if (_attackingCard == _defendingCard)
        {
            ResolveEndOfAttack(_attackingCard, _defendingCard,_callBack);
            return;
        }

        BoardData.AttackAnimation = new AttackAnimation
        {
            AttackerId = _attackingCard.UniqueId, DefenderId = _defendingCard.UniqueId, Id = Guid.NewGuid().ToString()
        };


        AnimateAttack(_attackingCard.UniqueId, _defendingCard.UniqueId, () =>
        {
            ResolveEndOfAttack(_attackingCard, _defendingCard,_callBack);
        });
    }

    public override void AnimateAttack(string _attackerId, string _defenderId, Action _callBack = null)
    {
        Card _attackingCard = GetCard(_attackerId);
        Card _defendingCard = GetCard(_defenderId);
        
        _attackingCard.transform.DOMove(_defendingCard.transform.position, 0.5f).OnComplete(() =>
        {
            _attackingCard.transform.DOLocalMove(Vector3.zero, 0.5f).SetDelay(0.2f).OnComplete(() =>
            {
                _callBack?.Invoke();
            });
        });
    }

    private void ResolveEndOfAttack(Card _attackingCard, Card _defendingCard, Action _callBack)
    {
        int _damage = _attackingCard.Damage;
        _defendingCard.ChangeHealth(-_damage);
        var _defendingPosition = _defendingCard.GetTablePlace().Id;

        OnCardAttacked?.Invoke(_attackingCard, _defendingCard, _damage);
        CheckForResponseAction(_attackingCard, _defendingCard);
        CheckIfDefenderIsDestroyed(_defendingCard,FinishResolve);

        void FinishResolve(bool _didDie)
        {
            if (_didDie)
            {
                if (_defendingCard is LifeForce)
                {
                    EndGame(!_defendingCard.My);
                    return;
                }

                HandleLoot(_attackingCard, _defendingCard,_defendingPosition);
            }
            
            _callBack?.Invoke();
        }
    }

    private void CheckForResponseAction(Card _attackingCard, Card _defendingCard)
    {
        if (_defendingCard.My == _attackingCard.My)
        {
            return;
        }

        if (_defendingCard.Health <= 0)
        {
            return;
        }

        if (!_defendingCard.IsWarrior())
        {
            return;
        }

        SetResponseAction(_defendingCard.My && RoomHandler.IsOwner, _defendingCard.UniqueId);
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
            Debug.Log("Should not check for this card");
            _callBack?.Invoke(false);
            return;
        }

        if (_defendingCard.Health > 0)
        {
            Debug.Log("Card didn't die");
            _callBack?.Invoke(false);
            return;
        }

        if (_defendingCard is Keeper _keeper)
        {
            
            float _healthToRecover = _keeper.Details.Stats.Health * _keeper.PercentageOfHealthToRecover / 100;
            int _heal = Mathf.RoundToInt(_healthToRecover + .3f);

            OnKeeperDied?.Invoke(_keeper);
            _defendingCard.SetHealth(_heal);
            var _lifeForce = _defendingCard.My ? GetMyLifeForce() : GetOpponentsLifeForce();
            _lifeForce.ChangeHealth(-_heal);
            
            PlaceKeeperOnTable(_defendingCard, FinishPlaceKeeper);
        }
        else
        {
            GameplayPlayer _defendingPlayer = _defendingCard.My ? MyPlayer : OpponentPlayer;
            _defendingPlayer.DestroyCard(_defendingCard);
            _callBack?.Invoke(true);
        }

        void FinishPlaceKeeper()
        {
            _callBack?.Invoke(true);
        }
    }

    private void PlaceKeeperOnTable(CardBase _card, Action _callBack)
    {
        List<int> _placesNearLifeForce;

        if (_card.GetIsMy())
        {
            _placesNearLifeForce = new List<int>()
            {
                10,
                12,
                18,
                17,
                19,
                9,
                13,
                19,
                16,
                23,
                24,
                25,
                26,
                27
            };
        }
        else
        {
            _placesNearLifeForce = new List<int>()
            {
                54,
                52,
                46,
                47,
                45,
                55,
                51,
                45,
                48,
                41,
                40,
                39,
                38,
                37
            };
        }


        foreach (var _placeNear in _placesNearLifeForce)
        {
            if (TryPlaceKeeper(_placeNear))
            {
                ReplaceKeeper(_card, _placeNear,_callBack);
                return;
            }
        }

        if (_card.GetIsMy())
        {
            for (int _i = 8; _i < 57; _i++)
            {
                if (TryPlaceKeeper(_i))
                {
                    ReplaceKeeper(_card, _i,_callBack);
                    return;
                }
            }
        }
        else
        {
            for (int _i = 57; _i > 8; _i--)
            {
                if (TryPlaceKeeper(_i))
                {
                    ReplaceKeeper(_card, _i,_callBack);
                    return;
                }
            }
        }
    }

    private bool TryPlaceKeeper(int _placeIndex)
    {
        TablePlaceHandler _place = TableHandler.GetPlace(_placeIndex);
        if (!_place.IsOccupied)
        {
            return true;
        }

        foreach (var _cardOnPlace in _place.GetCards())
        {
            if (_cardOnPlace is Keeper { My: true })
            {
                return true;
            }
        }

        return false;
    }

    private void HandleLoot(Card _attackingCard, Card _defendingCard,int _placeOfDefendingCard)
    {
        int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChangeForRoomOwner() : LootChangeForOther();
        bool _didIAttack = _attackingCard.GetIsMy();
        int _amount = _additionalMatter;
        if (_defendingCard is Minion)
        {
            _amount += 2;
        }

        if (_defendingCard is Guardian)
        {
            _amount += 10;
        }

        if (_defendingCard is Keeper)
        {
            _amount += 5;
        }

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
            Id = Guid.NewGuid().ToString(),
            Amount = _amount,
            ForMe = _forMe,
            PositionId = _placeOfDefendingCard,
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

        if (IsResponseAction2())
        {
            if (!IsMyResponseAction2())
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

    public override void BuyMinion(CardBase _cardBase, int _cost, Action _callBack = null)
    {
        string _cardId = ((Card)_cardBase).UniqueId;
        StartCoroutine(SelectPlaceRoutine());

        IEnumerator SelectPlaceRoutine()
        {
            yield return StartCoroutine(GetPlaceOnTable(FinishRevive));

            void FinishRevive(int _positionId)
            {
                HandleBoughtMinion(_positionId);
                if (_cost > 0)
                {
                    MyPlayer.Actions--;
                }

                if (MyPlayer.Actions>0)
                {
                    RoomUpdater.Instance.ForceUpdate();
                }
                _callBack?.Invoke();
            }
        }

        void HandleBoughtMinion(int _positionId)
        {
            ChangeMyStrangeMatter(-_cost);
            (_cardBase as Card)?.SetHasDied(false);
            PlaceCard(_cardId, _positionId);
        }
    }

    public override void BuildWall(CardBase _cardBase, int _cost)
    {
        string _cardId = ((Card)_cardBase).UniqueId;
        StartCoroutine(SelectPlaceRoutine());

        IEnumerator SelectPlaceRoutine()
        {
            yield return StartCoroutine(GetPlaceOnTable(FinishRevive, true));

            void FinishRevive(int _positionId)
            {
                ChangeMyStrangeMatter(-_cost);
                PlaceCard(_cardId, _positionId);
                
                if (_cost > 0)
                {
                    MyPlayer.Actions--;
                }
                
                if (MyPlayer.Actions>0)
                {
                    RoomUpdater.Instance.ForceUpdate();
                }
            }
        }
    }

    public override void TellOpponentSomething(string _text)
    {

    }

    public override void ChangeOwnerOfCard(string _cardId)
    {
        Card _card = GetAllMinions().Find(_card => _card.UniqueId == _cardId);
        _card.ChangeOwner();
    }

    private void ForceResponseAction(string _cardId)
    {
        SetIdOfCardWithResponseAction(_cardId);
        MyPlayer.Actions = 1;
        DialogsManager.Instance.ShowOkDialog("Your warrior survived attack, you get 1 response action");
    }

    public override void MarkMarkerAsBomb(string _cardId)
    {
        CardBase _cardBase = GetAllCards().Find(_card => _card.UniqueId == _cardId);
        Sprite _bombSprite = _cardBase.GetIsMy() ? MyPlayer.FactionSo.BombSprite : OpponentPlayer.FactionSo.BombSprite;
        _cardBase.Display.ChangeSprite(_bombSprite);
        OnFoundBombMarker?.Invoke(_cardBase);
    }

    public override void BombExploded(int _placeId, string _cardId)
    {
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
        _bombEffect.transform.position=_tablePlace.transform.position;
        yield return new WaitForSeconds(2);
        Destroy(_bombEffect);
    }

    public override void ActivateAbility(string _cardId)
    {
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

    public override void BuyAbilityFromShop(string _abilityId)
    {
        AbilityData _ability = AbilityCardsManagerBase.Instance.RemoveAbilityFromShop(_abilityId);
        if (_ability == null)
        {
            return;
        }

        _ability.Owner = FirebaseManager.Instance.PlayerId;
        BoardData.Abilities.Add(_ability);
        ChangeAmountOfAbilitiesICanBuy(-1);
        AudioManager.Instance.PlaySoundEffect("AbilityCardPurchased");

        int _abilityIdInt = _ability.CardId;
        if (_abilityIdInt == 1031)
        {
            return;
        }

        if (!(_abilityIdInt == 1005 && MyPlayer.Actions == 1))
        {
            MyPlayer.Actions--;
        }
    }

    public override void BuyAbilityFromHand(string _abilityId)
    {
        BoardData.AbilitiesInShop.Remove(BoardData.AbilitiesInShop.Find(_ability => _ability.UniqueId == _abilityId));
        MyPlayer.AddOwnedAbility(_abilityId);
        ChangeAmountOfAbilitiesICanBuy(-1);

        AudioManager.Instance.PlaySoundEffect("AbilityCardPurchased");
        var _ability = GetAbility(_abilityId);
        int _abilityIdInt = _ability.Details.Id;
        if (_abilityIdInt == 1031)
        {
            return;
        }

        if (!(_abilityIdInt == 1005 && MyPlayer.Actions == 1))
        {
            MyPlayer.Actions--;
        }
    }

    public override int PushCardForward(int _startingPlace, int _endingPlace, int _chanceForPush = 100, bool _tryToMoveSelf = false)
    {
        Card _pushedCard = TableHandler.GetPlace(_endingPlace).GetCard();

        if (_pushedCard == null)
        {
            return -1;
        }

        if (!_pushedCard.IsWarrior())
        {
            return -1;
        }

        if (UnityEngine.Random.Range(0, 100) > _chanceForPush)
        {
            return -1;
        }

        TablePlaceHandler _pushedCardPlace = _pushedCard.GetTablePlace();
        Vector2 _indexInFrontOfPushedCard = TableHandler.GetFrontIndex(_startingPlace, _pushedCardPlace.Id);
        TablePlaceHandler _placeInFrontOfPushedCard = TableHandler.GetPlace(_indexInFrontOfPushedCard);
        if (_placeInFrontOfPushedCard == null)
        {
            StartCoroutine(DamagePushedCard(true));
            return -1;
        }

        if (_placeInFrontOfPushedCard.IsAbility)
        {
            StartCoroutine(DamagePushedCard(true));
            return -1;
        }

        if (_placeInFrontOfPushedCard.GetCard() == null)
        {
            CardBase _pushedCardBase = _pushedCardPlace.GetCard();
            CardAction _moveCardInFront = new CardAction
            {
                FirstCardId = ((Card)_pushedCardBase).UniqueId,
                StartingPlaceId = _pushedCardPlace.Id,
                FinishingPlaceId = _placeInFrontOfPushedCard.Id,
                Type = CardActionType.Move,
                Cost = 0,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false,
                ResetSpeed = true
            };

            ExecuteCardAction(_moveCardInFront);
            return _pushedCardPlace.Id;
        }

        StartCoroutine(DamagePushedCard(true));
        return -1;

        IEnumerator DamagePushedCard(bool _shouldMoveSelf)
        {
            yield return new WaitForSeconds(0.5f);
            CardAction _damage = new CardAction()
            {
                FirstCardId = TableHandler.GetPlace(_endingPlace).GetCard().UniqueId,
                SecondCardId = _pushedCardPlace.GetCard().UniqueId,
                StartingPlaceId = _endingPlace,
                FinishingPlaceId = _pushedCardPlace.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = false,
                Damage = 1,
                CanCounter = false,
            };

            ExecuteCardAction(_damage);
            if (_shouldMoveSelf && _tryToMoveSelf)
            {
                var _attackedCard = TableHandler.GetPlace(_endingPlace).GetCard();
                if (_attackedCard != null)
                {
                    if (_attackedCard.Health != 0)
                    {
                        yield break;
                    }
                }

                var _myCardTable = TableHandler.GetPlace(_startingPlace).GetCard();
                if (_myCardTable == null)
                {
                    yield break;
                }

                if (_myCardTable.Health == 0)
                {
                    yield break;
                }

                CardAction _moveSelf = new CardAction()
                {
                    FirstCardId = _myCardTable.UniqueId,
                    StartingPlaceId = _myCardTable.GetTablePlace().Id,
                    FinishingPlaceId = _endingPlace,
                    Type = CardActionType.Move,
                    Cost = 0,
                    CanTransferLoot = false,
                    Damage = -1,
                    CanCounter = false,
                };

                ExecuteCardAction(_moveSelf);
            }
        }
    }

    public override void PushCardBack(int _startingPlace, int _endingPlace, int _chanceForPush = 100)
    {
        Card _pushedCard = TableHandler.GetPlace(_startingPlace).GetCardNoWall();

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
            return;
        }

        if (_placeBehindOfPushedCard.GetCard() == null)
        {
            Card _pushedCardBase = _pushedCardPlace.GetCard();
            if (!_pushedCardBase.CanMove || _placeBehindOfPushedCard.IsAbility)
            {
                StartCoroutine(DamagePushedCard());
                return;
            }

            CardAction _moveCardInFront = new CardAction
            {
                FirstCardId = _pushedCardPlace.GetCardNoWall().UniqueId,
                StartingPlaceId = _pushedCardPlace.Id,
                FinishingPlaceId = _placeBehindOfPushedCard.Id,
                Type = CardActionType.Move,
                Cost = 0,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false,
                ResetSpeed = true
            };

            ExecuteCardAction(_moveCardInFront);
            return;
        }

        StartCoroutine(DamagePushedCard());
        return;

        IEnumerator DamagePushedCard()
        {
            yield return new WaitForSeconds(0.5f);
            CardAction _damage = new CardAction()
            {
                FirstCardId = _pushedCardPlace.GetCard().UniqueId,
                SecondCardId = _pushedCardPlace.GetCard().UniqueId,
                StartingPlaceId = _pushedCardPlace.Id,
                FinishingPlaceId = _pushedCardPlace.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = false,
                Damage = 1,
                CanCounter = false,
            };

            ExecuteCardAction(_damage);
        }
    }

    public override void PlaceAbilityOnTable(string _abilityId)
    {
        TableHandler.GetAbilityPosition((_placeId) => { PlaceAbilityOnTable(_abilityId, _placeId); });
    }

    public override void PlaceAbilityOnTable(string _abilityId, int _placeId)
    {
        AbilityCard _ability = Resources.FindObjectsOfTypeAll<AbilityCard>().ToList().Find(_ability => _ability.UniqueId == _abilityId);
        TablePlaceHandler _tablePlace = TableHandler.GetPlace(_placeId);
        _ability.PositionOnTable(_tablePlace);
    }

    public override void ReturnAbilityFromActivationField(string _abilityId)
    {
        StartCoroutine(Handle());

        IEnumerator Handle()
        {
            yield return new WaitForSeconds(1);
            PlaceAbilityOnTable(_abilityId);
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

    public override void ChangeSprite(int _cardPlace, int _cardId, int _spriteId, bool _showPlaceAnimation = false)
    {
        StartCoroutine(HandleRoutine());

        IEnumerator HandleRoutine()
        {
            yield return new WaitForSeconds(1);
            Card _card = FindObjectsOfType<Card>().ToList().Find(_card =>
                _card.Details.Id == _cardId && _card.GetTablePlace() != null && _card.GetTablePlace().Id == _cardPlace);
            if (_card == null)
            {
                yield break;
            }

            Sprite _sprite = null;

            switch (_spriteId)
            {
                case 0:
                    _sprite = voidMarker;
                    _card.SetIsVoid(true);
                    break;
                case 1:
                    _sprite = snowMarker;
                    break;
                case 2:
                    _sprite = cyborgMarker;
                    break;
                case 3:
                    _sprite = dragonMarker;
                    break;
                case 4:
                    _sprite = forestMarker;
                    break;
            }

            if (_card.IsVoid && _sprite != voidMarker)
            {
                yield break;
            }

            bool _changedSprite = _card.Display.ChangeSprite(_sprite);
            if (_showPlaceAnimation && _changedSprite)
            {
                _card.transform.localPosition = new Vector3(-2000, 0);
                _card.MoveToPosition(_card.GetTablePlace());
            }
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

    private void UseDelivery(string _defendingCardId, int _startingPlace)
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
        if (_emptyPlaces.Count == 0)
        {
            _emptyPlaces = GetEmptyPlaces(new List<int>()
            {
                12,
                10,
                19,
                18,
                17
            });
            if (_emptyPlaces.Count == 0)
            {
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
                if (_emptyPlaces.Count == 0)
                {
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
                    if (_emptyPlaces.Count == 0)
                    {
                        for (int _i = 8; _i < 57; _i++)
                        {
                            if (PlaceAnywhere(_i, _defendingCardId, _startingPlace))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        if (_emptyPlaces.Count == 1)
        {
            DoPlace(_emptyPlaces[0].Id, _defendingCardId, _startingPlace);
        }
        else
        {
            StartCoroutine(SelectPlace(_emptyPlaces, true, DoPlaceOnTable));
        }


        List<TablePlaceHandler> GetEmptyPlaces(List<int> _places)
        {
            List<TablePlaceHandler> _emptyPlaces2 = new List<TablePlaceHandler>();
            foreach (var _placeId in _places)
            {
                TablePlaceHandler _place = TableHandler.GetPlace(_placeId);
                if (_place.IsOccupied)
                {
                    continue;
                }

                _emptyPlaces2.Add(_place);
            }

            return _emptyPlaces2;
        }

        bool PlaceAnywhere(int _index, string _defendingCardId2, int _startingPlace2)
        {
            TablePlaceHandler _place = TableHandler.GetPlace(_index);
            if (_place.IsOccupied)
            {
                return false;
            }

            DoPlace(_index, _defendingCardId2, _startingPlace2);
            return true;
        }

        void DoPlaceOnTable(int _placeId)
        {
            DoPlace(_placeId, _defendingCardId, _startingPlace);
        }

        void DoPlace(int _index, string _defendingCardId3, int _startingPlace3)
        {
            CardAction _actionMove = new CardAction
            {
                FirstCardId = _defendingCardId3,
                StartingPlaceId = _startingPlace3,
                FinishingPlaceId = _index,
                Type = CardActionType.Move,
                Cost = 0,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false
            };
            ExecuteCardAction(_actionMove);
        }
    }

    public override int StrangeMaterInEconomy()
    {
        return BoardData.StrangeMaterInEconomy;
    }

    public override void ChangeStrangeMaterInEconomy(int _amount)
    {
        BoardData.StrangeMaterInEconomy += _amount;
    }

    public override int StrangeMatterCostChange()
    {
        return BoardData.StrangeMatterCostChange;
    }

    public override void ChangeStrangeMatterCostChange(int _amount)
    {
        BoardData.StrangeMatterCostChange += _amount;
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
        return BoardData.Abilities.FindAll(_ability => _ability.UniqueId == _playerId);
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

        int _price = BoardData.UnchainingGuardianPrice - StrangeMatterCostChange();

        if (MyStrangeMatter() < _price && !GameplayCheats.HasUnlimitedGold)
        {
            DialogsManager.Instance.ShowOkDialog($"You don't have enough strange matter, this action requires {_price}");
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog($"Spend {_price} to unchain guardian??", () => { YesUnchain(_price); });
    }

    private void YesUnchain(int _price)
    {
        UnchainGuardian(_price,true);
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

            CardHandInteractions.OnCardClicked += SelectCard;
            MyPlayer.ShowCards(_type);

            yield return new WaitUntil(() => _hasSelectedCard);

            _place.SetColor(Color.white);

            PlaceCard(_selectedCard, _placeId);

            void SelectCard(CardBase _card)
            {
                CardHandInteractions.OnCardClicked -= SelectCard;
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
        ShowGuardianUnchained(true);
        if (_reduceAction)
        {
            MyPlayer.Actions--;
        }
        OnUnchainedGuardian?.Invoke();
        if (MyPlayer.Actions>0 && _reduceAction)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
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

    public override bool IsMyResponseAction2()
    {
        return RoomHandler.IsOwner
            ? GetGameplaySubState() == GameplaySubState.Player1ResponseAction
            : GetGameplaySubState() == GameplaySubState.Player2ResponseAction;
    }
    
    public override bool IsResponseAction2()
    {
        return GetGameplaySubState() is GameplaySubState.Player1ResponseAction or GameplaySubState.Player2ResponseAction;
    }
}