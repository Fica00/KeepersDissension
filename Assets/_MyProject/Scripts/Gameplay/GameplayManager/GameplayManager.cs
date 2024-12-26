using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static Action<CardBase, int, int> OnCardMoved;
    public static Action<CardBase, CardBase, int> OnCardAttacked;
    public static Action<CardBase, CardBase> OnSwitchedPlace;
    public static Action<CardBase> OnPlacedCard;
    public static Action<Keeper> OnKeeperDied;
    public static GameplayManager Instance;
    public static Action FinishedSetup;
    public static Action OnUnchainedGuardian;
    
    public GameplayPlayer MyPlayer;
    public GameplayPlayer OpponentPlayer;
    public TableHandler TableHandler;
    
    [SerializeField] private HealthTracker healthTracker;

    [SerializeField] protected List<SimpleOpenAndClose> simplePanels;
    [SerializeField] protected List<ArrowPanel> arrowPanels;
    [SerializeField] protected Sprite voidMarker;
    [SerializeField] protected Sprite snowMarker;
    [SerializeField] protected Sprite cyborgMarker;
    [SerializeField] protected Sprite dragonMarker;
    [SerializeField] protected Sprite forestMarker;
    [SerializeField] protected GameObject bombEffect;

    [HideInInspector] public bool DidOpponentFinish;
    protected bool DidIFinishMyTurn;
    private bool doIPlayFirst;
    
    public bool IsKeeperResponseAction2 =>  GetMyKeeper().UniqueId == IdOfCardWithResponseAction();
    
    protected virtual void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupTable();
        MyPlayer.UpdatedActions += TryEndTurn;
        StartCoroutine(GameplayRoutine());
    }

    private void TryEndTurn()
    {
        if (MyPlayer.Actions != 0)
        {
            return;
        }

        EndTurn();
    }

    protected virtual void SetupTable()
    {
        throw new NotImplementedException();
    }

    private IEnumerator GameplayRoutine()
    {
        SetGameState(GameplayState.WaitingForPlayersToLoad);
        yield return new WaitForSeconds(2);
        SetGameState(GameplayState.SettingUpTable);
        DidIFinishMyTurn = false;

        yield return NewMatchRoutine();
        SetPlayersTurn(doIPlayFirst);
        healthTracker.Setup();
        ShowGuardianChains();
        
        SetGameState(GameplayState.Gameplay);
        
        while (!HasGameEnded())
        {
            yield return WaitUntilTheEndOfTurn();
            yield return WaitUntilTheEndOfTurn();
        }
    }

    private void ShowGuardianChains()
    {
        GetMyGuardian().ShowChain();
        GetOpponentGuardian().ShowChain();
    }

    private IEnumerator NewMatchRoutine()
    {
        SetPlayersTurn(DecideWhoPlaysFirst());
        doIPlayFirst = IsMyTurn();

        yield return PlaceLifeForceAndGuardianRoutine();
        yield return PlaceMinions();

        if (ShouldIPlaceStartingWall())
        {
            PlaceStartingWall();
        }

        yield return new WaitForSeconds(1f);
        AbilityCardsManagerBase.Instance.Setup();
        yield return new WaitForSeconds(1f);

        FinishedSetup?.Invoke();
    }
    
    protected virtual IEnumerator PlaceLifeForceAndGuardianRoutine()
    {
        throw new Exception();
    }

    protected virtual IEnumerator PlaceMinions()
    {
        throw new Exception();
    }

    protected virtual bool DecideWhoPlaysFirst()
    {
        throw new NotImplementedException();
    }

    private IEnumerator WaitUntilTheEndOfTurn()
    {
        if (IsMyTurn())
        {
            DidIFinishMyTurn = false;
            MyPlayer.NewTurn();
            if (MyPlayer.Actions == 0)
            {
                DidIFinishMyTurn = true;
                SetPlayersTurn(false);
            }
            yield return new WaitUntil(() => DidIFinishMyTurn);
            MyPlayer.EndedTurn();
        }
        else
        {
            DidOpponentFinish = false;
            OpponentPlayer.NewTurn();
            yield return new WaitUntil(() => DidOpponentFinish);
            OpponentPlayer.EndedTurn();
        }
        CloseAllPanels();
    }

    public void Resign()
    {
        EndGame(false);
    }

    public virtual void EndGame(bool _didIWin)
    {
        throw new NotImplementedException();
    }

    public virtual void EndTurn()
    {
        throw new NotImplementedException();
    }

    public virtual void TryUnchainGuardian()
    {
        throw new Exception();
    }

    public virtual void UnchainGuardian(int _price, bool _reduceAction)
    {
        throw new Exception();
    }

    public virtual void PlaceStartingWall()
    {
        throw new Exception();
    }

    public virtual void PlaceCard(CardBase _card, int _positionId)
    {
        throw new NotImplementedException();
    }

    public virtual void AddAbilityToPlayer(bool _isMyPlayer, string _abilityId)
    {
        throw new NotImplementedException();
    }

    public virtual void AddAbilityToShop(string _abilityId)
    {
        throw new NotImplementedException();
    }

    public void ExecuteCardAction(CardAction _action)
    {
        HideCardActions();
        switch (_action.Type)
        {
            case CardActionType.Attack:
                ExecuteAttack(_action.FirstCardId, _action.SecondCardId, () =>
                {
                    FinishActionExecution(_action);
                });
                break;
            case CardActionType.Move:
                ExecuteMove(_action.StartingPlaceId, _action.FinishingPlaceId, _action.FirstCardId, () =>
                {
                    FinishActionExecution(_action);
                });
                break;
            case CardActionType.SwitchPlace:
                ExecuteSwitchPlace(_action.StartingPlaceId, _action.FinishingPlaceId, _action.FirstCardId, _action.SecondCardId, () =>
                {
                    FinishActionExecution(_action);
                });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void HideCardActions()
    {
        StartCoroutine(ClosePanelRoutine());
        IEnumerator ClosePanelRoutine()
        {
            yield return new WaitForSeconds(.01f);
            CardActionsDisplay.Instance.Close();
        }
    }

    private void FinishActionExecution(CardAction _action)
    {
        MyPlayer.Actions-=_action.Cost;

        if (MyPlayer.Actions<=0)
        {
            return;
        }
        
        RoomUpdater.Instance.ForceUpdate();
    }

    protected virtual void ExecuteAttack(string _firstCardId, string _secondCardId,Action _callBack)
    {
        throw new Exception();
    }

    public virtual void UseDelivery(string _defendingCardId, Action _callBack)
    {
        throw new Exception();
    }
    
    public virtual void ExecuteMove(int _startingPlaceId,int _finishingPlaceId, string _firstCardId, Action _callBack)
    {
        throw new Exception();
    }
    
    protected virtual void ExecuteSwitchPlace(int _startingPlaceId, int _finishingPlaceId,string _firstCardId,string _secondCardId, Action _callBack)
    {
        throw new Exception();
    }
    
    public virtual void BuyMinion(CardBase _cardBase, int _cost, Action _callBack = null)
    {
        throw new NotImplementedException();
    }

    public virtual void BuildWall(CardBase _cardBase, int _cost)
    {
        throw new NotImplementedException();
    }

    public void SelectPlaceForSpecialAbility(int _startingPosition, int _range, PlaceLookFor _lookForPlace,
        CardMovementType _movementType, bool _includeSelf, LookForCardOwner _lookFor, Action<int> _callBack,
        bool _ignoreMarkers = true, bool _ignoreWalls=false)
    {
       TableHandler.ActionsHandler.ClearPossibleActions();
        List<TablePlaceHandler> _availablePlaces = TableHandler.GetPlacesAround(_startingPosition, _movementType, _range, _includeSelf);
            foreach (var _availablePlace in _availablePlaces.ToList())
            {
                bool _isOccupied = _availablePlace.IsOccupied;

                if (_lookForPlace == PlaceLookFor.Empty && _isOccupied)
                {
                    if (_includeSelf)
                    {
                        if (_startingPosition != _availablePlace.Id)
                        {
                            _availablePlaces.Remove(_availablePlace);
                            continue;
                        }
                    }
                    else if (!(_availablePlace.ContainsMarker && _ignoreMarkers))
                    {
                        _availablePlaces.Remove(_availablePlace);
                        continue;
                    }
                }

                if (_lookForPlace == PlaceLookFor.Occupied && !_isOccupied)
                {
                    _availablePlaces.Remove(_availablePlace);
                    continue;
                }

                if (!_includeSelf && _startingPosition == _availablePlace.Id)
                {
                    _availablePlaces.Remove(_availablePlace);
                    continue;
                }

                if (_lookForPlace == PlaceLookFor.Occupied)
                {
                    List<CardBase> _cardsAtPlace = _availablePlace.GetCards();
                    foreach (var _cardAtPlace in _cardsAtPlace)
                    {
                        if (_cardAtPlace.GetIsMy() && _lookFor == LookForCardOwner.Enemy)
                        {
                            _availablePlaces.Remove(_availablePlace);
                            break;
                        }

                        if (!_cardAtPlace.GetIsMy() && _lookFor == LookForCardOwner.My)
                        {
                            _availablePlaces.Remove(_availablePlace);
                            break;
                        }
                    }
                }
            }

            StartCoroutine(SelectPlace(_availablePlaces, _ignoreWalls, _callBack));
    }
    
    protected IEnumerator SelectPlace(List<TablePlaceHandler> _availablePlaces, bool _ignoreWalls, Action<int> _callBack)
    {
        foreach (var _availablePlace in _availablePlaces.ToList())
        {
            if (_availablePlace.ContainsWall && _ignoreWalls)
            {
                _availablePlaces.Remove(_availablePlace);
            }
        }

        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        if (_availablePlaces.Count == 0)
        {
            _callBack?.Invoke(-1);
            yield break;
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
    }

    public virtual void ChangeOwnerOfCard(string _placeId)
    {
        throw new NotImplementedException();
    }

    public virtual void TellOpponentSomething(string _key)
    {
        throw new NotImplementedException();
    }

    public virtual void BombExploded(int _placeId, string _cardId)
    {
        throw new NotImplementedException();
    }

    public virtual void ShowBombAnimation(int _placeId)
    {
        throw new Exception();
    }

    public virtual void ActivateAbility(string _cardId)
    {
        throw new NotImplementedException();
    }

    public virtual void BuyAbilityFromShop(string _abilityId)
    {
        throw new NotImplementedException();
    }

    public virtual void BuyAbilityFromHand(string _abilityId)
    {
        throw new NotImplementedException();
    }

    public virtual void PushCardBack(int _startingPlace, int _endingPlace, int _chanceForPush = 100)
    {
        throw new NotImplementedException();
    }

    public virtual void PlaceAbilityOnTable(string _abilityId)
    {
        throw new NotImplementedException();
    }
    
    public virtual void PlaceAbilityOnTable(string _abilityId, int _placeId)
    {
        throw new NotImplementedException();
    }

    public virtual void ReturnAbilityFromActivationField(string _abilityId)
    {
        throw new NotImplementedException();
    }

    public virtual void BuyMatter()
    {
        throw new NotImplementedException();
    }

    public virtual void ShowBoughtMatter(bool _didIBuy)
    {
        throw new Exception();
    }

    public virtual void MarkMarkerAsBomb(string _cardId)
    {
        throw new Exception();
    }

    public void CloseAllPanels()
    {
        foreach (var _arrowPanel in arrowPanels)
        {
            _arrowPanel.Hide();
        }

        foreach (var _simplePanel in simplePanels)
        {
            _simplePanel.CloseObject();
        }
    }

    public virtual void DestroyBombWithoutActivatingIt(int _cardId, bool _isMy)
    {
        throw new Exception();
    }

    public virtual void ChangeSprite(string _cardId, int _spriteId, bool _showPlaceAnimation=false)
    {
        throw new Exception();
    }

    public virtual void PlayAudioOnBoth(string _key, CardBase _card)
    {
        throw new Exception();
    }

    protected void PlayMovingSoundEffect(CardBase _card)
    {
        if (_card is Minion _bomber && _bomber.name.ToLower().Contains("bomber"))
        {
            if (_bomber.Details.Faction.IsCyber) //cyborg
            {
                PlayAudioOnBoth("CyborgBomberMoving", _card);
            }
            else if (_bomber.Details.Faction.IsDragon)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "GiftsToShare", "HahaLetsGo", "LetsGetThisPartyStarted"
                }),_card);
            }
            else if (_bomber.Details.Faction.IsForest)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IfYouHearTheSoundOfTicking", "LetMeKnowWhenYouNeedMe", "TriedAndTrueAreTheWays"
                }),_card);
            }
            else if (_bomber.Details.Faction.IsSnow)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IfYouSaySo", "LookOutForMyBombs", "YesButKnowThatVerySoon"
                }),_card);
            }
        }
        else if (_card is Guardian _guardian)
        {
            if (_guardian.IsChained)
            {
                if (_guardian.Details.Faction.IsCyber)
                {
                    PlayAudioOnBoth("CyberGuardianLoudClank",_card);
                }
                else if (_guardian.Details.Faction.IsDragon)
                {
                    PlayAudioOnBoth("WaterSplash",_card);
                }
                else if (_guardian.Details.Faction.IsSnow)
                {
                    PlayAudioOnBoth(GetRandomKey(new List<string>()
                    {
                        "Growl1", "Growl2", "Growl3"
                    }),_card);
                }
                else if (_guardian.Details.Faction.IsForest)
                {
                    PlayAudioOnBoth("ForestChained",_card);
                }
            }
            else
            {
                if (_guardian.Details.Faction.IsSnow)
                {
                    PlayAudioOnBoth("UnchainedGrowling",_card);
                }
                else if (_guardian.Details.Faction.IsCyber)
                {
                    PlayAudioOnBoth("CyborgGuardian2",_card);
                }
                else if (_guardian.Details.Faction.IsForest)
                {
                    PlayAudioOnBoth("ForestUnchained",_card);
                }
                else if (_guardian.Details.Faction.IsDragon)
                {
                    PlayAudioOnBoth("DragonGuardianUnchained",_card);
                }
            }
        }
        else if (_card is Minion _blockader && _blockader.name.ToLower().Contains("blockader"))
        {
            if (!_card.GetIsMy())
            {
                return;
            }

            if (_blockader.Details.Faction.IsCyber)
            {
                PlayAudioOnBoth(
                    GetRandomKey(new List<string>() { "IAmReadyForBattle", "TheyAreYearning", "YesGoodPlan" }),_card);
            }
            else if (_blockader.Details.Faction.IsDragon)
            {
                PlayAudioOnBoth(
                    GetRandomKey(new List<string>() { "ComeAtMe", "IEatRocksFor", "WouldYouLikeTobeAPrisoner" }),_card);
            }
            else if (_blockader.Details.Faction.IsForest)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "HmmIShallGoThisWay", "KeeperINeedMoreBattle", "WhyCantIBeTheGuardian"
                }),_card);
            }
            else if (_blockader.Details.Faction.IsSnow)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IWillTakeMyStand", "MayNoneBeInMyWay", "OfCourseYouAreTheKeeperBlockader"
                }),_card);
            }
        }
        else if (_card is Minion _mage && _mage.name.ToLower().Contains("mage"))
        {
            if (!_card.GetIsMy())
            {
                return;
            }

            if (_mage.Details.Faction.IsCyber)
            {
                PlayAudioOnBoth(
                    GetRandomKey(new List<string>() { "AsTheKeeperCommands", "IWillStayVigilant", "Yes" }),_card);
            }
            else if (_mage.Details.Faction.IsDragon)
            {
                PlayAudioOnBoth(
                    GetRandomKey(new List<string>() { "AlwaysReady", "IWillNotSecondGuess", "NoDangerWillSlow" }),_card);
            }
            else if (_mage.Details.Faction.IsForest)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IfPeaceCannotBeFound", "MyPowersAreAtYourCommand", "TheyAreComing"
                }),_card);
            }
            else if (_mage.Details.Faction.IsSnow)
            {
                PlayAudioOnBoth(
                    GetRandomKey(new List<string>() { "IFearOnlyOurLoss", "IfYouSauSo", "MyEnchantmentsWill" }),_card);
            }
        }
        else if (_card is Minion _orge && _orge.name.ToLower().Contains("org"))
        {
            if (!_card.GetIsMy())
            {
                return;
            }

            if (_orge.Details.Faction.IsCyber)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "ICannotWaitToDevourThem", "IWillDemolish", "TheIronIsHot"
                }),_card);
            }
            else if (_orge.Details.Faction.IsDragon)
            {
                PlayAudioOnBoth(
                    GetRandomKey(new List<string>() { "AFewMoreSteps", "ICanSeeTheFear", "IfIMust" }),_card);
            }
            else if (_orge.Details.Faction.IsForest)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "ISmellFear", "LookingForwardToAn", "YesOfCourseJustKeep"
                }),_card);
            }
            else if (_orge.Details.Faction.IsSnow)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IAmLookingForAChallenge", "IFearNothing", "TheColdIsMyAlly"
                }),_card);
            }
        }
        else if (_card is Minion _scout && _scout.name.ToLower().Contains("scout"))
        {
            if (!_card.GetIsMy())
            {
                return;
            }

            if (_scout.Details.Faction.IsCyber)
            {
                PlayAudioOnBoth(
                    GetRandomKey(new List<string>() { "AsYouWish", "MyVisionIsClear", "ThisAreaSeemsSafe" }),_card);
            }
            else if (_scout.Details.Faction.IsDragon)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IHaveSpottedSomething", "YesIWillScoutTheArea", "YesKeeperAndMayYour"
                }),_card);
            }
            else if (_scout.Details.Faction.IsForest)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "ICanSeeTheEnemy", "MayPeaceReturn", "OfCourseYouAreTheKeeper"
                }),_card);
            }
            else if (_scout.Details.Faction.IsSnow)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IWillScoutOutAhead", "NoOneHasBestMeYet", "YouAreTheKeeper2"
                }),_card);
            }
        }
        else if (_card is Minion _sniper && _sniper.name.ToLower().Contains("sniper"))
        {
            if (!_card.GetIsMy())
            {
                return;
            }

            if (_sniper.Details.Faction.IsCyber)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "MyAimWillBeTrue", "WithMyBowIShallPursue", "YesWithPurpose"
                }),_card);
            }
            else if (_sniper.Details.Faction.IsDragon)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IHaveNoConcerns", "ItsColdButThisIsWar", "WeWillHitThemHard"
                }),_card);
            }
            else if (_sniper.Details.Faction.IsForest)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "LetThemCome", "MayYourWisdom", "YesKeeper"
                }),_card);
            }
            else if (_sniper.Details.Faction.IsSnow)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "AsYouWish2", "IShallDoSoQuietly", "YesMaster"
                }),_card);
            }
        }
        else if (_card is Minion _scaler && _scaler.name.ToLower().Contains("scaler"))
        {
            if (!_card.GetIsMy())
            {
                return;
            }

            if (_scaler.Details.Faction.IsDragon)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "AsYouWishScat", "GiveMeAChallenge", "OurFoesWillScatter"
                }),_card);
            }
            else if (_scaler.Details.Faction.IsForest)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "AgileTheySay", "IHaveFoughtInWorse", "IWillConquerAnyWall"
                }),_card);
            }
            else if (_scaler.Details.Faction.IsSnow)
            {
                PlayAudioOnBoth(GetRandomKey(new List<string>()
                {
                    "IamNoBug", "IWillBuryThem", "NoWallCanHinderMe"
                }),_card);
            }
            else if (_scaler.Details.Faction.IsCyber)
            {
                PlayAudioOnBoth("CyborgScaler",_card);
            }
        }
    }

    private string GetRandomKey(List<string> _keys,int _chance=1)
    {
        int _randomChance = UnityEngine.Random.Range(0, _chance);
        if (_randomChance!=0)
        {
            return string.Empty;
        }
        int _randomNumber = UnityEngine.Random.Range(0, _keys.Count);
        return _keys[_randomNumber];
    }

    public Keeper GetMyKeeper()
    {
        return FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
    }
    
    public Keeper GetOpponentKeeper()
    {
        return FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
    }    
    
    public Guardian GetOpponentGuardian()
    {
        return FindObjectsOfType<Guardian>().ToList().Find(_guardian => !_guardian.My);
    }    
    
    public Guardian GetMyGuardian()
    {
        return FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My);
    }

    public LifeForce GetMyLifeForce()
    {
        return FindObjectsOfType<LifeForce>().ToList().Find(_lifeForce => _lifeForce.My);
    }

    public LifeForce GetOpponentsLifeForce()
    {
        return FindObjectsOfType<LifeForce>().ToList().Find(_lifeForce => !_lifeForce.My);
    }

    public bool IsCardVetoed(string _uniqueCardId)
    {
        var _vetoCard = FindObjectOfType<Veto>();
        if (_vetoCard==null)
        {
            return false;
        }

        if (!_vetoCard.IsActive)
        {
            return false;
        }

        return _vetoCard.IsEffected(_uniqueCardId);
    }
    
    public bool IsCardTaxed(string _uniqueId)
    {
        var _taxedCard = FindObjectOfType<Tax>();
        if (_taxedCard == null)
        {
            return false;
        }

        if (!_taxedCard.IsActive)
        {
            return false;
        }
        
        return _taxedCard.IsEffected(_uniqueId);
    }
    
    protected void FinishEffect<T>() where T : MonoBehaviour
    {
        var _ability = FindObjectOfType<T>() as AbilityEffect;
        if (_ability == null)
        {
            return;
        }

        _ability.TryToCancel();
    }
    
    public bool IsAbilityActiveForMe<T>() where T : MonoBehaviour
    {
        var _ability = FindObjectOfType<T>() as AbilityEffect;
        if (_ability == null)
        {
            return false;
        }

        return _ability.IsActive && _ability.IsMy;
    }

    public bool IsAbilityActiveForOpponent<T>() where T : MonoBehaviour
    {
        var _ability = FindObjectOfType<T>() as AbilityEffect;
        if (_ability == null)
        {
            return false;
        }

        return _ability.IsActive && !_ability.IsMy;
    }

    public bool IsAbilityActive<T>() where T : MonoBehaviour
    {
        var _ability = FindObjectOfType<T>() as AbilityEffect;
        if (_ability == null)
        {
            return false;
        }

        return _ability.IsActive;
    }

    public virtual void ChangeLootAmountForMe(int _amount)
    {
        throw new Exception();
    }
    
    public List<Card> GetAllCards()
    {
        return FindObjectsOfType<Card>().ToList();
    }

    public List<Minion> GetAllMinions()
    {
        return FindObjectsOfType<Minion>().ToList();
    }

    public virtual int StrangeMaterInEconomy()
    {
        throw new Exception();
    }

    public virtual void ChangeStrangeMaterInEconomy(int _amount)
    {
        throw new Exception();
    }

    public virtual int StrangeMatterCostChange()
    {
        throw new Exception();
    }

    public virtual void ChangeStrangeMatterCostChange(int _amount)
    {
        throw new Exception();
    }

    public virtual string IdOfCardWithResponseAction()
    {
        throw new Exception();
    }

    public virtual int MyStrangeMatter()
    {
        throw new Exception();
    }

    public virtual int OpponentsStrangeMatter()
    {
        throw new Exception();
    }

    public virtual void ChangeMyStrangeMatter(int _amount)
    {
        throw new Exception();
    }
    
    public virtual void ChangeOpponentsStrangeMatter(int _amount)
    {
        throw new Exception();
    }

    public virtual int AmountOfAbilitiesPlayerCanBuy()
    {
        throw new Exception();
    }

    public virtual void ChangeAmountOfAbilitiesICanBuy(int _amount)
    {
        throw new Exception();
    }
    
    public virtual List<Card> GetAllCardsOfType(CardType _type, bool _forMe)
    {
        throw new Exception();
    }

    public virtual Card GetCardOfType(CardType _type, bool _forMe)
    {
        throw new Exception();
    }
    
    public virtual void AddCard(CardData _cardData)
    {
        throw new Exception();
    }

    public virtual void AddAbility(AbilityData _abilityData, bool _forMe)
    {
        throw new Exception();
    }

    public virtual AbilityCard GetAbility(string _uniqueId)
    {
        throw new Exception();
    }

    public virtual void RemoveAbility(string _uniqueId, bool _forMe)
    {
        throw new Exception();
    }

    public virtual bool ContainsCard(CardData _requestedCard, bool _forMe)
    {
        throw new Exception();
    }

    public virtual List<AbilityData> GetOwnedAbilities(bool _forMe)
    {
        throw new Exception();
    }

    public virtual Card GetCard(string _uniqueId)
    {
        throw new Exception();
    }
    
    public virtual CardPlace GetCardPlace(string _uniqueCardId)
    {
        throw new Exception();
    }

    public virtual void SetCardPlace(string _uniqueCardId, CardPlace _place)
    {
        throw new Exception();
    }

    public virtual CardPlace GetCardPlace(CardBase _cardBase)
    {
        throw new Exception();
    }

    public virtual void OpponentCreatedCard(CardData _cardData)
    {
        throw new Exception();
    }

    public virtual void ShowCardPlaced(string _uniqueId, int _positionId)
    {
        throw new Exception();
    }

    public virtual void ShowCardMoved(string _uniqueId, int _positionId, Action _callBack)
    {
        throw new Exception();
    }

    public virtual int AmountOfActionsPerTurn()
    {
        throw new Exception();
    }

    public virtual bool IsSettingUpTable()
    {
        throw new Exception();
    }

    public virtual void SetGameState(GameplayState _gameState)
    {
        throw new Exception();
    }

    public virtual GameplayState GameState()
    {
        throw new Exception();
    }

    public virtual void SetHasGameEnded(bool _status, string _winner)
    {
        throw new Exception();
    }

    public virtual bool HasGameEnded()
    {
        throw new Exception();
    }

    public virtual bool IsMyTurn()
    {
        throw new Exception();
    }

    public virtual void SetPlayersTurn(bool _isMyTurn)
    {
        throw new Exception();
    }

    public virtual bool ShouldIPlaceStartingWall()
    {
        throw new Exception();
    }

    public virtual void SetGameplaySubState(GameplaySubState _subState)
    {
        throw new Exception();
    }

    public virtual GameplaySubState GetGameplaySubState()
    {
        throw new Exception();
    }

    public virtual Card GetCardOfTypeNotPlaced(CardType _type, bool _forMe)
    {
        throw new Exception();
    }

    public virtual int AmountOfActions(bool _forMe)
    {
        throw new Exception();
    }

    public virtual void SetAmountOfActions(int _amount, bool _forMe)
    {
        throw new Exception();
    }

    public virtual void ShowCardAsDead(string _uniqueId)
    {
        throw new Exception();
    }

    public virtual void AnimateAttack(string _attackerId, string _defenderId, Action _callBack = null)
    {
        throw new Exception();
    }

    public virtual void AnimateStrangeMatter(int _amount, bool _forMe, int _placeOfDefendingCard)
    {
        throw new Exception();
    }

    public virtual void AnimateSoundEffect(string _key, string _cardUniqueId)
    {
        throw new Exception();
    }

    public virtual void ShowGameEnded(string _winner)
    {
        throw new Exception();
    }

    public virtual bool DidUnchainGuardian(bool _forMe)
    {
        throw new Exception();
    }

    public virtual void ShowGuardianUnchained(bool _didIUnchain)
    {
        throw new Exception();
    }

    public virtual bool IsMyResponseAction2()
    {
        throw new Exception();
    }
    
    public virtual bool IsResponseAction2()
    {
        throw new Exception();
    }

    public virtual void DamageCardByAbility(string _uniqueId, int _damage, Action<bool> _callBack)
    {
        throw new Exception();
    }
}