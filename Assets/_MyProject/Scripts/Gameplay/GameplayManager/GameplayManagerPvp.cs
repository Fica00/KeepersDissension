using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using FirebaseMultiplayer.Room;

public class GameplayManagerPvp : GameplayManager
{
    private RoomHandler roomHandler;
    private RoomData RoomData => roomHandler.RoomData;
    private BoardData boardData => roomHandler.RoomData.BoardData;

    protected override void Awake()
    {
        base.Awake();
        roomHandler = FirebaseManager.Instance.RoomHandler;
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
        StopGame(true);
    }

    protected override void SetupTable()
    {
        MyPlayer.Setup(DataManager.Instance.PlayerData.FactionId, true);
        RoomPlayer _opponent = roomHandler.GetOpponent();
        OpponentPlayer.Setup(_opponent.FactionId,false);
        GameplayUI.Instance.Setup();
        if (!roomHandler.IsOwner)
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

            Card _card =  CreateCard(_cardInDeck.Details.Id,_tableSideHandler,_playerId);
            if (_card is Wall)
            {
                _wallCard = _card;
            }
            AddCard(_card.Data);
        }

        for (int _i = 0; _i < 30; _i++)
        {
            Card _card = CreateCard(_wallCard.Details.Id,_tableSideHandler, _playerId);
            _card.Data.CardId = _card.Details.Id;
            AddCard(_card.Data);
        }
    }

    public override void OpponentCreatedCard(CardData _cardData)
    {
        var _createdCard = CreateCard(_cardData.CardId, OpponentPlayer.TableSideHandler, _cardData.Owner);
        _createdCard.SetData(_cardData);
    }

    private Card CreateCard(int _cardId,TableSideHandler _tableSideHandler, string _playerId)
    {
        Transform _cardsHolder = _tableSideHandler.CardsHolder;
        Card _card = CardsManager.Instance.CreateCard(_cardId);
        _card.transform.SetParent(_cardsHolder);
        _card.SetParent(_cardsHolder);
        _card.Setup(_playerId);
        return _card;
    }

    protected override bool DecideWhoPlaysFirst()
    {
        return roomHandler.IsOwner;
    }

    public override void StopGame(bool _didIWin)
    {
        if (HasGameEnded())
        {
            return;
        }
        
        Debug.Log("Ending game");
        SetHasGameEnded(true);
        DataManager.Instance.PlayerData.CurrentRoomId=string.Empty;
        StopAllCoroutines();
        GameplayUI.Instance.ShowResult(_didIWin);
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
        
        if (_cardBase==null)
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
        
        _card.Data.PlaceId = _positionId;
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
        
        _card.MoveToPosition(_destination);
    }

    public override void AddAbilityToPlayer(bool _isMyPlayer, string _abilityId)
    {
        var _player = _isMyPlayer ? FirebaseManager.Instance.RoomHandler.BoardData.MyPlayer : FirebaseManager.Instance.RoomHandler.BoardData.OpponentPlayer;
        var _abilityData = FirebaseManager.Instance.RoomHandler.BoardData.AvailableAbilities.Find(_ability => _ability.UniqueId == _abilityId);
        if (_abilityData==null)
        {
            return;
        }

        _abilityData.Owner = _player.PlayerId;
        boardData.Abilities.Add(_abilityData);
        boardData.AvailableAbilities.Remove(_abilityData);
    }

    public override void AddAbilityToShop(string _abilityId)
    {
        var _abilityData = FirebaseManager.Instance.RoomHandler.BoardData.AvailableAbilities.Find(_ability => _ability.UniqueId == _abilityId);
        if (_abilityData==null)
        {
            return;
        }
        FirebaseManager.Instance.RoomHandler.BoardData.AvailableAbilities.Remove(_abilityData);
        FirebaseManager.Instance.RoomHandler.BoardData.AbilitiesInShop.Add(_abilityData);
    }

    protected override void ExecuteMove(CardAction _action)
    {
        TablePlaceHandler _startingDestination = TableHandler.GetPlace(_action.StartingPlaceId);
        TablePlaceHandler _destination = TableHandler.GetPlace(_action.FinishingPlaceId);
        Card _movingCard = null;
        
        foreach (var _possibleCard in _startingDestination.GetCards())
        {
            Card _card = _possibleCard as Card;
            if (_card==null)
            {
                continue;
            }

            if (_action.FirstCardId != _card.UniqueId)
            {
                continue;
            }
            
            _movingCard = _card;
            break;
        }

        if (_movingCard==null)
        {
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
                SniperStealth.ReturnDiscoveryCardTo = _action.StartingPlaceId;
                CardBase _cardBase = _destination.GetCardNoWall();
                GameplayPlayer _markerOwner = _cardBase.GetIsMy() ? MyPlayer : OpponentPlayer;
                _markerOwner.DestroyCard(_cardBase);
            }
        }

        ShowCardMoved(_movingCard.UniqueId, _destination.Id);
        OnCardMoved?.Invoke(_movingCard,_action.StartingPlaceId,_action.FinishingPlaceId, _action.DidTeleport);
        PlayMovingSoundEffect(_movingCard);
    }

    protected override void ExecuteSwitchPlace(CardAction _action)
    {
        TablePlaceHandler _startingDestination = TableHandler.GetPlace(_action.StartingPlaceId);
        TablePlaceHandler _destination = TableHandler.GetPlace(_action.FinishingPlaceId);

        List<CardBase> _firstCards = _startingDestination.GetCards();
        List<CardBase> _secondCards = _destination.GetCards();
        CardBase _firstCard = null;
        CardBase _secondCard = null;

        foreach (var _cardBase in _firstCards)
        {
            Card _card = _cardBase as Card;
            if (_card==null)
            {
                continue;
            }

            if (_card.UniqueId != _action.FirstCardId)
            {
                continue;
            }
            
            _firstCard = _card;
            break;
        }
        
        foreach (var _cardBase in _secondCards)
        {
            Card _card = _cardBase as Card;
            if (_card == null)
            {
                continue;
            }

            if (_card.UniqueId != _action.SecondCardId)
            {
                continue;
            }
            
            _secondCard = _card;
            break;
        }

        if (_firstCard == null || _secondCard == null)
        {
            return;
        }
        
        _firstCard.MoveToPosition(_destination);
        _secondCard.MoveToPosition(_startingDestination);
        OnSwitchedPlace?.Invoke(_firstCard,_secondCard);
    }

    protected override void ExecuteMoveAbility(CardAction _action)
    {
        TablePlaceHandler _destination = TableHandler.GetPlace(_action.FinishingPlaceId);
        TablePlaceHandler _startingDestination = TableHandler.GetPlace(_action.StartingPlaceId);
        CardBase _movingCard = null;
       
        foreach (var _possibleCard in _startingDestination.GetCards())
        {
            AbilityCard _card = _possibleCard as AbilityCard;
            if (_card==null)
            {
                continue;
            }

            if (_action.FirstCardId != _card.UniqueId)
            {
                continue;
            }
            
            _movingCard = _possibleCard;
            break;
        }

        if (_movingCard==null)
        {
            return;
        }
        
        if (_destination.ContainsMarker)
        {
            SniperStealth.ReturnDiscoveryCardTo = _action.StartingPlaceId;
            CardBase _cardBase = _destination.GetCardNoWall();
            GameplayPlayer _markerOwner = _cardBase.GetIsMy() ? MyPlayer : OpponentPlayer;
            _markerOwner.DestroyCard(_cardBase);
        }

        _movingCard.MoveToPosition(_destination);
    }
    
    protected override void ExecuteAttack(CardAction _action)
    {
        if (IsAbilityActive<Truce>() && _action.CanBeBlocked)
        {
            DialogsManager.Instance.ShowOkDialog("Attacks are blocked by Truce");
            return;
        }
        
        TablePlaceHandler _attackingPosition = TableHandler.GetPlace(_action.StartingPlaceId);
        TablePlaceHandler _defendingPosition = TableHandler.GetPlace(_action.FinishingPlaceId);

        List<CardBase> _attackingCards = _attackingPosition.GetCards();
        List<CardBase> _defendingCards = _defendingPosition.GetCards();
        Card _attackingCard;
        Card _defendingCard;
        
        try
        {
            _attackingCard = _attackingCards.Find(_card=> ((Card)_card).UniqueId== _action.FirstCardId) as Card;
            _defendingCard = _defendingCards.Find(_card=> ((Card)_card).UniqueId== _action.SecondCardId) as Card;
        }
        catch
        {
            return;
        }

        if (_attackingCard==null || _defendingCard == null)
        {
            return;
        }
        
        Vector3 _positionOfDefendingCard = _defendingCard.transform.position;

        if (_action.Type == CardActionType.Attack&& _action.CanCounter)
        {
            WallBase.AttackerPlace = _action.StartingPlaceId;
        }
        else
        {
            WallBase.AttackerPlace = -1;
        }

        GameplayPlayer _attackingPlayer = _attackingCard.My ? MyPlayer : OpponentPlayer;
        GameplayPlayer _defendingPlayer = _defendingCard.My ? MyPlayer : OpponentPlayer;
        
        if (_defendingCard is Wall && TableHandler.DistanceBetweenPlaces(_attackingCard.GetTablePlace(),_defendingCard.GetTablePlace())>1)
        {
            _attackingPlayer.Actions -= _action.Cost;
            return;
        }
        
        AudioManager.Instance.PlaySoundEffect("Attack");

        if (_attackingCard == _defendingCard)
        {
            ResolveEndOfAttack();
            return;
        }

        _attackingCard.transform.DOMove(_defendingCard.transform.position, 0.5f)
            .OnComplete(() =>
            {
                _attackingCard.transform.DOLocalMove(Vector3.zero, 0.5f)
                    .SetDelay(0.2f)
                    .OnComplete(ResolveEndOfAttack);
            });

        void ResolveEndOfAttack()
        {
            bool _dealDamage = true;

            if (_defendingCard.CanFlyToDodgeAttack && _action.CanBeBlocked)
            {
                _dealDamage = false;
                _action.Damage = 0;
            }
            
            int _damage = _action.Damage != -1 ? _action.Damage : _attackingCard.Damage;
            Grounded _grounded = FindObjectOfType<Grounded>();
            bool _usedGrounded = false;
            if (_grounded!=null && _grounded.IsActive(_attackingCard,_defendingCard) && _action.CanBeBlocked)
            {
                _damage = 0;
                _usedGrounded = true;
                if (_defendingCard.My)
                {
                    // ChangeMovementForCard(_defendingCard.GetTablePlace().Id,false);
                }
            }
            
            if (_dealDamage)
            {
                //hunter ability
                if (_attackingCard.My)
                {
                    if (IsAbilityActiveForMe<Hunter>() && _attackingCard is Keeper _keeper && _keeper.My && _defendingCard is Guardian _guardian && 
                        !_guardian.My)
                    {
                        _damage *= 2;
                    }
                }
                else
                {
                    if (IsAbilityActiveForOpponent<Hunter>() && _attackingCard is Keeper _keeper && !_keeper.My && _defendingCard is Guardian 
                    _guardian && _guardian.My)
                    {
                        _damage *= 2;
                    }
                }
                //hunter ability ends
                
                if (_defendingCard is Keeper)
                {
                    if (IsAbilityActive<Invincible>() && _defendingCard.My&& _action.CanBeBlocked)
                    {
                        DialogsManager.Instance.ShowOkDialog("Damage blocked by Invincible ability");
                        _damage = 0;
                    }
                    else if (IsAbilityActiveForOpponent<Invincible>() && !_defendingCard.My&& _action.CanBeBlocked)
                    {
                        DialogsManager.Instance.ShowOkDialog("Damage blocked by Invincible ability");
                        _damage = 0;
                    }
                    else if (IsAbilityActive<Steadfast>() && _defendingCard.My && _attackingCard is Minion && !_attackingCard.My&& _action.CanBeBlocked)
                    {
                        DialogsManager.Instance.ShowOkDialog("Damage blocked by Steadfast ability");
                        _damage = 0;
                    }
                    else if (IsAbilityActiveForOpponent<Steadfast>() && !_defendingCard.My && _attackingCard is Minion && 
                    _attackingCard.My && _action.CanBeBlocked)
                    {
                        DialogsManager.Instance.ShowOkDialog("Damage blocked by Steadfast ability");
                        _damage = 0;
                    }

                    if (_action.CanBeBlocked && !_usedGrounded)
                    {
                        Armor _armor = FindObjectsOfType<AbilityEffect>().ToList().Find(_abilityEffect =>
                            _abilityEffect is Armor &&
                            _abilityEffect.IsMy == _defendingCard.My) as Armor;
                        if (_armor !=null && _armor.CanExecuteThisTurn)
                        {
                            _armor.MarkAsUsed();
                            _damage--;
                        }
                    }
                }

                if (IsAbilityActive<HighStakes>() && _action.CanBeBlocked)
                {
                    _damage = 8;
                    FinishEffect<HighStakes>();
                }

                if (_defendingCard is Minion)
                {
                    foreach (var _ability in _defendingCard.SpecialAbilities)
                    {
                        if (_ability is BlockaderCard _blockaderAbility && _blockaderAbility.CanBlock)
                        {
                            _damage--;
                            // _blockaderAbility.CanBlock = false;
                        }
                    }
                }
                
                _defendingCard.ChangeHealth(-_damage);
            }

            if (_defendingCard.CanFlyToDodgeAttack&& _action.CanBeBlocked)
            {
                _defendingCard.SetCanFlyToDodgeAttack(false);
                _attackingPlayer.Actions -= _action.Cost;
                if (_attackingCard.My != _defendingCard.My)
                {
                    if(!_defendingCard.My && _attackingCard.My)
                    {
                        return;
                    }
                }
                else
                {
                    if (!_defendingCard.My)
                    {
                        return;
                    }
                }
                
                UseDelivery(_defendingCard.UniqueId,_defendingCard.GetTablePlace().Id);
                return;
            }

            OnCardAttacked?.Invoke(_attackingCard,_defendingCard, _damage);
            CheckForResponseAction();
            CheckIfDefenderIsDestroyed();

            if (_action.Cost!=0)
            {
                _attackingPlayer.Actions -= _action.Cost;
            }
        }

        void CheckForResponseAction()
        {
            string _idOfDefendingCard = string.Empty;
            if (_defendingCard.My == _attackingCard.My)
            {
                return;
            }

            if (!_defendingCard.IsWarrior())
            {
                if (_defendingCard is Wall _defendingWall)
                {
                    TablePlaceHandler _defendingTablePlace = _defendingWall.GetTablePlace();
                    Card _cardOnWall = _defendingTablePlace.GetCardNoWall();
                    if (_cardOnWall == null)
                    {
                        return;
                    }

                    if (_cardOnWall.My == _attackingCard.My)
                    {
                        return;
                    }

                    _idOfDefendingCard = _cardOnWall.UniqueId;

                    if (_cardOnWall.My)
                    {
                        ForceResponseAction(_cardOnWall.UniqueId);
                    }
                    else
                    {
                        OpponentGotResponseAction();
                    }

                    return;
                }

                return;
            }

            if (!_action.CanCounter)
            {
                return;
            }

            if (GameState() is not (GameplayState.Playing or GameplayState.Waiting))
            {
                return;
            }

            if (!(_defendingCard.Health > 0) || _attackingCard.My == _defendingCard.My)
            {
                return;
            }

            if ((IsAbilityActiveForMe<Ambush>() && !_defendingCard.My) || (IsAbilityActiveForOpponent<Ambush>() && _defendingCard.My))
            {
                DialogsManager.Instance.ShowOkDialog("Ignore next response action activated!");
                FinishEffect<Ambush>();
                return;
            }

            if (_defendingCard.My)
            {
                ForceResponseAction(_idOfDefendingCard);
            }
            else
            {
                OpponentGotResponseAction();
            }
        }

        void CheckIfDefenderIsDestroyed()
        {
            if (!(_defendingCard.IsWarrior() || _defendingCard is Wall or Marker))
            {
                return;
            }

            if (_defendingCard.Health > 0)
            {
                return;
            }


            if (_defendingCard is Keeper _keeper)
            {
                if (IsAbilityActive<Subdued>())
                {
                    FinishEffect<Subdued>();
                }
                if (IsAbilityActive<Explode>())
                {
                    BombExploded(_defendingCard.GetTablePlace().Id, false);
                }
                
                if (_defendingCard.My)
                {
                    PlaceKeeperOnTable(_defendingCard);
                }

                float _healthToRecover = (_keeper.Details.Stats.Health *_keeper.PercentageOfHealthToRecover)/100;
                int _heal = Mathf.RoundToInt(_healthToRecover + .3f);
                if (IsAbilityActive<Minionized>())
                {
                    _heal = 1;
                }
                OnKeeperDied?.Invoke(_defendingCard as Keeper);
                _defendingCard.SetHealth(_heal);
                FindObjectsOfType<Card>().ToList()
                    .Find(_element => _element is LifeForce && _element.My == _defendingCard.My).ChangeHealth(-_heal);
            }
            else
            {
                _defendingCard.AllowCardEffectOnDeath = _action.AllowCardEffectOnDeath;
                _defendingPlayer.DestroyCard(_defendingCard);
            }

            if (!_action.DiedByBomb)
            {
                if (_attackingPlayer.IsMy)
                {
                    int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChangeForRoomOwner() : LootChangeForOther();
                    if (_defendingCard is Minion)
                    {
                        GetMatter(2+_additionalMatter, true);
                    }

                    if (_defendingCard is Guardian)
                    {
                        GetMatter(10+_additionalMatter, true);
                    }

                    if (_defendingCard is Keeper)
                    {
                        GetMatter(5+_additionalMatter, true);
                    }
                }
                else
                {
                    int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChangeForRoomOwner() : LootChangeForOther();
                    if (_defendingCard is Minion)
                    {
                        GetMatter(2+_additionalMatter, false);
                    }

                    if (_defendingCard is Guardian)
                    {
                        GetMatter(10+_additionalMatter, false);
                    }

                    if (_defendingCard is Keeper)
                    {
                        GetMatter(5+_additionalMatter, false);
                    }
                }
            }
            else
            {
                if (true)
                {
                    int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChangeForRoomOwner() : LootChangeForOther();
                    if (_defendingCard is Minion)
                    {
                        GetMatter(2+_additionalMatter, true);
                    }

                    if (_defendingCard is Guardian)
                    {
                        GetMatter(10+_additionalMatter, true);
                    }

                    if (_defendingCard is Keeper)
                    {
                        GetMatter(5+_additionalMatter, true);
                    }
                }
                else
                {
                    int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChangeForRoomOwner() : LootChangeForOther();
                    if (_defendingCard is Minion)
                    {
                        GetMatter(2+_additionalMatter, false);
                    }

                    if (_defendingCard is Guardian)
                    {
                        GetMatter(10+_additionalMatter, false);
                    }

                    if (_defendingCard is Keeper)
                    {
                        GetMatter(5+_additionalMatter, false);
                    }
                }
            }
            

            if (_defendingCard is LifeForce)
            {
                StopGame(!_defendingCard.My);
            }

            if (_defendingCard.My == _attackingCard.My)
            {
                if (_action.GiveLoot)
                {
                    ChangeOpponentsStrangeMatter(MyStrangeMatter());
                }
                return;
            }

            if (!_action.CanTransferLoot)
            {
                return;
            }

            if (_defendingCard.Details.Type == CardType.Keeper)
            {
                if (!_attackingCard.My)
                {
                    return;
                }
                ChangeMyStrangeMatter(OpponentsStrangeMatter());
            }
        }

        void GetMatter(int _amount, bool _didIBuy)
        {
            for (int _i = 0; _i < _amount; _i++)
            {
                EconomyPanelHandler.Instance.ShowBoughtMatter(_didIBuy,_positionOfDefendingCard);
            }
        }

        void PlaceKeeperOnTable(CardBase _card)
        {
            List<int> _placesNearLifeForce = new List<int>(){10,12,18,17,19,9,13,19,16,23,24,25,26,27};
            foreach (var _placeNear in _placesNearLifeForce)
            {
                if (TryPlaceKeeper(_placeNear))
                {
                    ReplaceKeeper(_card, _placeNear);
                    return;
                }
            }
            for (int _i = 8; _i < 57; _i++)
            {
                if (TryPlaceKeeper(_i))
                {
                    ReplaceKeeper(_card, _i);
                    return;
                }
            }
        }

        bool TryPlaceKeeper(int _placeIndex)
        {
            TablePlaceHandler _place = TableHandler.GetPlace(_placeIndex);
            if (_place.IsOccupied)
            {
                foreach (var _cardOnPlace in _place.GetCards())
                {
                    if (_cardOnPlace is Keeper _keeper && _keeper.My)
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }
    }
        
    private int LootChangeForRoomOwner()
    {
        return roomHandler.IsOwner ? boardData.MyPlayer.LootChange : boardData.OpponentPlayer.LootChange;
    }

    private int LootChangeForOther()
    {
        return roomHandler.IsOwner ? boardData.OpponentPlayer.LootChange : boardData.MyPlayer.LootChange;
    }

    private IEnumerator GetPlaceOnTable(Action<int> _callBack, bool _wholeTable = false)
    {
        List<TablePlaceHandler> _availablePlaces = new List<TablePlaceHandler>();
        List<int> _lifeForceRow = new List<int>() { 8, 9, 10, 11, 12, 13, 14 };
        List<int> _keepersRow = new List<int>() { 15, 16, 17, 18, 19, 20, 21 };
        List<int> _thirdRow = new List<int>() { 22, 23, 24, 25, 26, 27, 28 };
        List<int> _wallRow = new List<int>() { 29, 30, 31, 32, 33, 34, 35 };
        List<int> _fourthRow = new List<int>() { 36, 37, 38, 39, 40, 41, 42 };
        List<int> _opponentKeepersRow = new List<int>() { 43, 44, 45, 46, 47, 48, 49 };
        List<int> _opponentLifeForceRow = new List<int>() { 50, 51, 52, 53, 54, 55, 56 };


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

    private void ReplaceKeeper(CardBase _card, int _placeId)
    {
        StartCoroutine(ReplaceRoutine());
        IEnumerator ReplaceRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            PlaceCard(_card, _placeId);
            if (IsAbilityActive<Comrade>())
            {
                List<CardBase> _cards = GetAllCardsOfType(CardType.Minion,true).Cast<CardBase>().ToList();
                if (_cards.Count==0)
                {
                    yield break;
                }
                ChooseCardPanel.Instance.ShowCards(_cards, PlaceMinion);
            }

            void PlaceMinion(CardBase _minion)
            {
                StartCoroutine(ChoosePlaceForMinion(_minion));
            }

            IEnumerator ChoosePlaceForMinion(CardBase _selectedMinion)
            {
                yield return StartCoroutine(GetPlaceOnTable(FinishPlaceMinion));

                void FinishPlaceMinion(int _positionId)
                {
                    PlaceCard(_selectedMinion,_positionId);
                }
            }
        }
    }

    public override void EndTurn()
    {
        if (IsAbilityActive<Casters>())
        {
            return;
        }
        
        CloseAllPanels();
        if (GameState() == GameplayState.WaitingForAttackResponse)
        {
            return;
        }

        TableHandler.ActionsHandler.ClearPossibleActions();

        if (GameState() == GameplayState.AttackResponse)
        {
            SetGameState(GameplayState.Waiting);
            MyPlayer.Actions=0;
            GameplayUI.Instance.ForceActionUpdate(OpponentPlayer.Actions == 0? 1:OpponentPlayer.Actions,false,false);
            Debug.Log("Finished response action");
            AudioManager.Instance.PlaySoundEffect("EndTurn");
            return;
        }

        if (!IsMyTurn())
        {
            return;
        }

        if (GameState() != GameplayState.Playing)
        {
            return;
        }

        DidIFinishMyTurn = true;
        SetPlayersTurn(false);

        FirebaseNotificationHandler.Instance.SendNotificationToUser(roomHandler.GetOpponent().Id, "Your turn!", "Come back to game!");
        AudioManager.Instance.PlaySoundEffect("EndTurn");
    }
    
    public override void BuyMinion(CardBase _cardBase, int _cost, Action _callBack=null)
    {
        GameplayState _gameState = GameState(); 
        SetGameState(GameplayState.BuyingMinion);
        string _cardId = ((Card)_cardBase).UniqueId;
        StartCoroutine(SelectPlaceRoutine());

        IEnumerator SelectPlaceRoutine()
        {
            yield return StartCoroutine(GetPlaceOnTable(FinishRevive));

            void FinishRevive(int _positionId)
            {
                HandleBoughtMinion(_positionId);
                SetGameState(_gameState);
                _callBack?.Invoke();
                if (_cost>0)
                {
                    MyPlayer.Actions--;
                }
            }
        }
        
        void HandleBoughtMinion(int _positionId)
        {
            GameplayPlayer _player = _cardBase.GetIsMy() ? MyPlayer : OpponentPlayer;
            if (_player.IsMy)
            {
                if (_cardBase.GetIsMy())
                {
                    ChangeMyStrangeMatter(-_cost);
                }
                else
                {
                    ChangeOpponentsStrangeMatter(-_cost);
                }
            }

            PlaceCard(_cardId, _positionId);
        
            (_cardBase as Card)?.SetHasDied(false);
        }
    }

    public override void BuildWall(CardBase _cardBase, int _cost)
    {
        GameplayState _state = GameState();
        SetGameState(GameplayState.BuildingWall);
        string _cardId = ((Card)_cardBase).UniqueId;
        StartCoroutine(SelectPlaceRoutine());

        IEnumerator SelectPlaceRoutine()
        {
            yield return StartCoroutine(GetPlaceOnTable(FinishRevive, true));

            void FinishRevive(int _positionId)
            {
                HandleBuildWall(_cardBase, _cost, _positionId, _cardId);
                SetGameState(_state);
                if (_cost>0)
                {
                    MyPlayer.Actions--;
                }
            }
        }
    }

    private void HandleBuildWall(CardBase _cardBase, int _cost, int _positionId, string _cardId)
    {
        GameplayPlayer _player = _cardBase.GetIsMy() ? MyPlayer : OpponentPlayer;
        if (_player.IsMy)
        {
            if (_player.IsMy)
            {
                ChangeMyStrangeMatter(-_cost);
            }
            else
            {
                ChangeOpponentsStrangeMatter(-_cost);
            }
        }
        PlaceCard(_cardId, _positionId);
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
        SetGameState(GameplayState.AttackResponse);
        GameplayUI.Instance.ForceActionUpdate(MyPlayer.Actions, true,true);
        DialogsManager.Instance.ShowOkDialog("Your warrior survived attack, you get 1 response action");
    }

    public override void MarkMarkerAsBomb(string _cardId)
    {
        CardBase _cardBase = GetAllCards().Find(_card => _card.UniqueId == _cardId);
        Sprite _bombSprite = _cardBase.GetIsMy() ? MyPlayer.FactionSo.BombSprite : OpponentPlayer.FactionSo.BombSprite;
        _cardBase.Display.ChangeSprite(_bombSprite);
        OnFoundBombMarker?.Invoke(_cardBase);
    }
    
    public override void BombExploded(int _placeId, bool _includeCenter=true)
    {
        StartCoroutine(BombExplodedRoutine());
        
        IEnumerator BombExplodedRoutine()
        {
            yield return new WaitForSeconds(0.3f);
            HandleBombExploded(_placeId,_includeCenter);
        }
    }

    private void HandleBombExploded(int _placeId, bool _includeCenter)
    {
        SpawnBombEffect(_placeId);
        List<TablePlaceHandler> _availablePlaces =
            TableHandler.GetPlacesAround(_placeId,
                CardMovementType.EightDirections, _includeCenter:_includeCenter);

        foreach (var _availablePlace in _availablePlaces)
        {
            if (!_availablePlace.IsOccupied)
            {
                continue;
            }

            List<CardBase> _cardsOnPlace = _availablePlace.GetCards();
            int _amountOfCardsOnPlace = +_cardsOnPlace.Count;
            foreach (var _cardOnPlace in _cardsOnPlace)
            {
                if (_cardOnPlace is not Card _card)
                {
                    continue;
                }

                if (_amountOfCardsOnPlace>1)
                {
                    bool _isScaler = false;
                    foreach (var _ability in _card.SpecialAbilities)
                    {
                        if (_ability is ScalerScale)
                        {
                            _isScaler = true;
                            break;
                        }
                    }

                    if (_isScaler)
                    {
                        continue;
                    }
                }

                CardAction _action = new CardAction
                {
                    FirstCardId = _card.UniqueId,
                    SecondCardId = _card.UniqueId,
                    StartingPlaceId = _availablePlace.Id,
                    FinishingPlaceId = _availablePlace.Id,
                    Type = CardActionType.Attack,
                    Cost = 0,
                    CanTransferLoot = false,
                    Damage = 3,
                    CanCounter = false,
                    DiedByBomb = true
                };

                ExecuteCardAction(_action);
            }
        }
        
        TableHandler.ActionsHandler.ClearPossibleActions();
    }

    private void SpawnBombEffect(int _placeId)
    {
        StartCoroutine(ShowBombEffect());
        
        IEnumerator ShowBombEffect()
        {
            TablePlaceHandler _tablePlace = TableHandler.GetPlace(_placeId);
            GameObject _bombEffect = Instantiate(bombEffect, GameplayUI.Instance.transform);
            yield return null;
            _bombEffect.transform.position=_tablePlace.transform.position;
            yield return new WaitForSeconds(2);
            Destroy(_bombEffect);
        }
    }

    public override void ActivateAbility(string _cardId)
    {
        AbilityCard _ability =
            FindObjectsOfType<AbilityCard>().ToList().Find(_ability => _ability.UniqueId == _cardId);
        
        if (_ability.GetTablePlace()==null)
        {
            TableHandler.GetAbilityPosition(PlaceAbility);
        }
        else
        {
            HandleActivateAbility();
        }
        
        void PlaceAbility(int _placeId)
        {
            if (_placeId==-1)
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
        boardData.Abilities.Add(_ability);
        ChangeAmountOfAbilitiesICanBuy(-1);
        AudioManager.Instance.PlaySoundEffect("AbilityCardPurchased");

        int _abilityIdInt = _ability.CardId;
        if (_abilityIdInt == 1031)
        {
            return;
        }
        
        if (!(_abilityIdInt==1005 && MyPlayer.Actions==1))
        {
            MyPlayer.Actions--;
        }
    }

    public override void BuyAbilityFromHand(string _abilityId)
    {
        boardData.AbilitiesInShop.Remove(boardData.AbilitiesInShop.Find(_ability => _ability.UniqueId == _abilityId));
        MyPlayer.AddOwnedAbility(_abilityId);
        ChangeAmountOfAbilitiesICanBuy(-1);

        AudioManager.Instance.PlaySoundEffect("AbilityCardPurchased");
        var _ability = GetAbility(_abilityId);
        int _abilityIdInt = _ability.Details.Id;
        if (_abilityIdInt == 1031)
        {
            return;
        }
        
        if (!(_abilityIdInt==1005 && MyPlayer.Actions==1))
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
        if (_placeInFrontOfPushedCard==null)
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
                    if (_attackedCard.Health!=0)
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
        Vector2 _indexBehindOfPushedCard = TableHandler.GetBehindIndex(_startingPlace,_endingPlace);
        TablePlaceHandler _placeBehindOfPushedCard = TableHandler.GetPlace(_indexBehindOfPushedCard);
        if (_placeBehindOfPushedCard==default)
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
        TableHandler.GetAbilityPosition((_placeId)=>
        {
            PlaceAbilityOnTable(_abilityId,_placeId);
        });
    }
    
    public override void PlaceAbilityOnTable(string _abilityId,int _placeId)
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
        EconomyPanelHandler.Instance.ShowBoughtMatter(true);
    }

    public override void DestroyBombWithoutActivatingIt(int _cardId, bool _isMy)
    {
        Card _bomber = FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id == _cardId && _card.My == _isMy);
        GameplayPlayer _player = _bomber.My ? MyPlayer : OpponentPlayer;
        _player.DestroyWithoutNotify(_bomber);
    }
    
    public override void ChangeSprite(int _cardPlace, int _cardId, int _spriteId,bool _showPlaceAnimation=false)
    {
        StartCoroutine(HandleRoutine());
        IEnumerator HandleRoutine()
        {
            yield return new WaitForSeconds(1);
            Card _card = FindObjectsOfType<Card>().ToList().Find(_card =>
                _card.Details.Id == _cardId && _card.GetTablePlace() != null && _card.GetTablePlace().Id == _cardPlace);
            if (_card==null)
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
            if (_showPlaceAnimation&&_changedSprite)
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
        if (_card!=null)
        {
            _card.Display.ShowWhiteBox();
        }
        AudioManager.Instance.PlaySoundEffect(_key);
    }

    private void OpponentGotResponseAction()
    {
        TableHandler.ActionsHandler.ClearPossibleActions();
        OpponentPlayer.Actions = 1;
        SetGameState(GameplayState.WaitingForAttackResponse);
        GameplayUI.Instance.ForceActionUpdate(OpponentPlayer.Actions, false,true);
        DialogsManager.Instance.ShowOkDialog("Opponents warrior survived, he gets 1 response action");
    }

    private void UseDelivery(string _defendingCardId, int _startingPlace)
    {
        List<TablePlaceHandler> _emptyPlaces = GetEmptyPlaces(new List<int>(){8,9,10,11,12,13,14});
        if (_emptyPlaces.Count==0)
        {
            _emptyPlaces = GetEmptyPlaces(new List<int>(){12,10,19,18,17});
            if (_emptyPlaces.Count==0)
            {
                _emptyPlaces = GetEmptyPlaces(new List<int>(){13,20,27,26,25,24,23,16,9});
                if (_emptyPlaces.Count==0)
                {
                    _emptyPlaces = GetEmptyPlaces(new List<int>(){14,21,28,27,26,25,24,23,22,15,8});
                    if (_emptyPlaces.Count==0)
                    {
                        for (int _i = 8; _i < 57; _i++)
                        {
                            if (PlaceAnywhere(_i,_defendingCardId,_startingPlace))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        if (_emptyPlaces.Count==1)
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

                DoPlace(_index,_defendingCardId2,_startingPlace2);
                return true;
            }
        
        void DoPlaceOnTable(int _placeId)
        { 
            DoPlace(_placeId,_defendingCardId,_startingPlace);
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
        return boardData.StrangeMaterInEconomy;
    }

    public override void ChangeStrangeMaterInEconomy(int _amount)
    {
        boardData.StrangeMaterInEconomy += _amount;
    }
    
    public override int StrangeMatterCostChange()
    {
        return boardData.StrangeMatterCostChange;
    }

    public override void ChangeStrangeMatterCostChange(int _amount)
    {
        boardData.StrangeMatterCostChange += _amount;
    }
    
    public override string IdOfCardWithResponseAction()
    {
        return boardData.IdOfCardWithResponseAction;
    }

    private void SetIdOfCardWithResponseAction(string _cardId)
    {
        boardData.IdOfCardWithResponseAction = _cardId;
    }
    
    public override void ChangeLootAmountForMe(int _amount)
    {
        boardData.MyPlayer.LootChange += _amount;
    }

    public override int MyStrangeMatter()
    {
        return boardData.MyPlayer.StrangeMatter;
    }

    public override int OpponentsStrangeMatter()
    {
        return boardData.OpponentPlayer.StrangeMatter;
    }
    
    public override void ChangeMyStrangeMatter(int _amount)
    {
        boardData.MyPlayer.StrangeMatter += _amount;
    }
    
    public override void ChangeOpponentsStrangeMatter(int _amount)
    {
        boardData.OpponentPlayer.StrangeMatter += _amount;
    }
    
    public override int AmountOfAbilitiesPlayerCanBuy()
    {
        return boardData.MyPlayer.AmountOfAbilitiesPlayerCanBuy;
    }

    public override void ChangeAmountOfAbilitiesICanBuy(int _amount)
    {
        boardData.MyPlayer.AmountOfAbilitiesPlayerCanBuy += _amount;
    }
    
    public override List<Card> GetAllCardsOfType(CardType _type, bool _forMe)
    {
        return FindObjectsOfType<Card>().ToList().FindAll(_card => _card.Details.Type == _type && _card.GetIsMy() == _forMe);
    }
    
    public override void AddCard(CardData _cardData)
    {
        boardData.Cards.Add(_cardData);
    }

    public override void AddAbility(AbilityData _abilityData, bool _forMe)
    {
        boardData.Abilities.Add(_abilityData);
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
        foreach (var _card in boardData.Abilities)
        {
            if (_card.UniqueId != _uniqueId)
            {
                continue;                
            }

            _cardData = _card;
            break;
        }
        if (_cardData ==null)
        {
            return;
        }

        boardData.Abilities.Remove(_cardData);    
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
        string _playerId = _forMe ? boardData.MyPlayer.PlayerId : boardData.OpponentPlayer.PlayerId;
        return boardData.Abilities.FindAll(_ability => _ability.UniqueId == _playerId);
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
        return GetCard(_uniqueCardId).Data.CardPlace;
    }

    public override void SetCardPlace(string _uniqueCardId, CardPlace _place)
    {
        GetCard(_uniqueCardId).Data.CardPlace = _place;
    }

    public override CardPlace GetCardPlace(CardBase _cardBase)
    {
        CardPlace _cardPlace = CardPlace.Deck;
        if (_cardBase is Card _cardCard)
        {
            _cardPlace = _cardCard.Data.CardPlace;
        }
        else if (_cardBase is AbilityCard _ability)
        {
            _cardPlace = _ability.Data.CardPlace;
        }

        return _cardPlace;
    }

    public override void TryUnchainGuardian()
    {
        if (GameState() != GameplayState.Playing)
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

        int _price = boardData.UnchainingGuardianPrice - StrangeMatterCostChange();

        if (MyStrangeMatter() < _price && !GameplayCheats.HasUnlimitedGold)
        {
            DialogsManager.Instance.ShowOkDialog($"You don't have enough strange matter, this action requires {_price}");
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog($"Spend {_price} to unchain guardian??", () => { YesUnchain(_price); });
    }
    
    private void YesUnchain(int _price)
    {
        ChangeMyStrangeMatter(-_price);
        UnchainGuardian();
        MyPlayer.Actions--;
    }

    public override int AmountOfActionsPerTurn()
    {
        return boardData.AmountOfActionsPerTurn;
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

    public override void SetHasGameEnded(bool _status)
    {
        RoomData.HasGameEnded = _status;
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
        if (roomHandler.IsOwner)
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
        Debug.Log("Is my turn: "+ IsMyTurn());
        if (IsMyTurn())
        {
            Debug.Log("Placing my life force and guardian");
            yield return PlaceLifeForceAndGuardian();
            SetGameplaySubState(GameplaySubState.Player2PlacingLifeForce);
            Debug.Log("Waiting for 2 to place his life force and guardian");
            yield return new WaitUntil(() => GetGameplaySubState() == GameplaySubState.FinishedPlacingStartingLifeForce);
        }
        else
        {
            Debug.Log("Waiting for 1 player to place life force and guardian");
            yield return new WaitUntil(() => GetGameplaySubState() ==GameplaySubState.Player2PlacingLifeForce);
            Debug.Log("Placing my life force and guardian");
            yield return PlaceLifeForceAndGuardian();
            yield return new WaitForSeconds(5f);
            Debug.Log("Telling player 1 that I placed my guardian and life force");
            SetGameplaySubState(GameplaySubState.FinishedPlacingStartingLifeForce);
            RoomUpdater.Instance.ForceUpdate();
        }

        
        Debug.Log("Finished with placing starting life force and guardian");
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
        Debug.Log("Is my turn inside place minions: "+IsMyTurn());
        if (IsMyTurn())
        {
            Debug.Log("Placing starting minions");
            yield return PlaceRestOfStartingCards();
            SetGameplaySubState(GameplaySubState.Player2SelectMinions);
            yield return new WaitUntil(() => GetGameplaySubState() == GameplaySubState.FinishedSelectingMinions);
        }
        else
        {
            Debug.Log("Waiting for opponent to place starting cards");
            yield return new WaitUntil(() => GetGameplaySubState() == GameplaySubState.Player2SelectMinions);
            yield return PlaceRestOfStartingCards();
            yield return new WaitForSeconds(5f);
            Debug.Log("Telling 1. player that I finished with setting up minions");
            SetGameplaySubState(GameplaySubState.FinishedSelectingMinions);
            RoomUpdater.Instance.ForceUpdate();
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log("Finished with setting up the table");
    }
    
     private IEnumerator PlaceRestOfStartingCards()
     {
        yield return PlaceKeeper();
        DialogsManager.Instance.ShowOkBigDialog("Now pick your minions to go into battle alongside you. Each minion has their own attributes and abilities. You can hold down on any card anytime to zoom in on that card and then you can tap that card to flip it over to see more details.");
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
            List<TablePlaceHandler> _availablePlaces = new List<TablePlaceHandler>
            {
                TableHandler.GetPlace(10),
                TableHandler.GetPlace(12)
            };

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
            PlaceCard(_keeperCard,_selectedPlaceId);

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
}