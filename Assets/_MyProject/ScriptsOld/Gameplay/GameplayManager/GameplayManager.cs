using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameplayActions;
using Unity.VisualScripting;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static Action<CardBase, int, int, bool> OnCardMoved;
    public static Action<CardBase, CardBase, int> OnCardAttacked;
    public static Action<CardBase, CardBase> OnSwitchedPlace;
    public static Action<CardBase> OnPlacedCard;
    public static Action<Keeper> OnKeeperDied;
    public static GameplayManager Instance;
    public static Action FinishedSetup;
    public static Action<CardBase> OnFoundBombMarker;
    public static Action OnUnchainedGuardian;
    public GameplayPlayer MyPlayer;
    public GameplayPlayer OpponentPlayer;
    public TableHandler TableHandler;
    public int AmountOfActionsPerTurn;
    public int UnchainingGuardianPrice;
    public int AmountOfAbilitiesPlayerCanBuy;
    public StrangeMatter WhiteStrangeMatter;
    [HideInInspector] public int StrangeMatterCostChange;
    [HideInInspector] public bool ShouldIPlaceStartingWall;
    [SerializeField] protected List<SimpleOpenAndClose> simplePanels;
    [SerializeField] protected List<ArrowPanel> arrowPanels;
    [SerializeField] protected Sprite voidMarker;
    [SerializeField] protected Sprite snowMarker;
    [SerializeField] protected Sprite cyborgMarker;
    [SerializeField] protected Sprite dragonMarker;
    [SerializeField] protected Sprite forestMarker;
    [SerializeField] protected GameObject bombEffect;

    protected bool IsMyTurn;
    protected bool HasGameEnded;
    protected bool Finished;
    protected bool OpponentFinished;
    protected bool HasOpponentPlacedStartingCards;
    private bool doIPlayFirst;

    protected StrangeMatterTracker strangeMatterTracker;

    public bool UsingVisionToDestroyMarkers;
    protected ActionData lastAction;

    public bool IsSettingUpTable =>
        GameState is GameplayState.SettingUpTable or GameplayState.WaitingForPlayersToLoad;

    [HideInInspector] public CardAction LastAction;
    [HideInInspector] public GameplayState GameState;
    [SerializeField] protected HealthTracker healthTracker;

    public int[]
        LootChanges = { 0, 0 }; //add those changes when loot for killing is given, index 0 is for master client

    [HideInInspector] public int IdOfCardWithResponseAction;
    public bool IsKeeperResponseAction => IdOfCardWithResponseAction == 10 || IdOfCardWithResponseAction == 60;

    public bool MyTurn => IsMyTurn;

    public bool IsExecutingOldActions;
    

    protected virtual void Awake()
    {
        Instance = this;
        strangeMatterTracker = FindObjectOfType<StrangeMatterTracker>();
        BomberMinefield.BombMarkers = new List<CardBase>();
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
        GameState = GameplayState.WaitingForPlayersToLoad;
        yield return new WaitForSeconds(2); //just in case, wait a bit for players to load correctly
        GameState = GameplayState.SettingUpTable;
        int _round = 0;
        Finished = false;
        OpponentFinished = false;

        yield return NewMatchRoutine();
        IsMyTurn = doIPlayFirst;
        healthTracker.Setup();
        ShowGuardianChains();
        MyPlayer.UpdatedStrangeMatter += TellOpponentThatIUpdatedWhiteStrangeMatter;
        WhiteStrangeMatter.UpdatedAmountInEconomy += TellOpponentToUpdateWhiteStrangeMatterReserves;
        
        while (!HasGameEnded)
        {
            yield return WaitUntilTheEndOfTurn(); //first players turn
            yield return new WaitForSeconds(1); //sync up
            yield return WaitUntilTheEndOfTurn(); //second players turn
            yield return new WaitForSeconds(1); //sync up
            _round++;
        }
    }

    protected virtual void TellOpponentThatIUpdatedWhiteStrangeMatter()
    {
        throw new Exception();
    }

    protected virtual void TellOpponentToUpdateWhiteStrangeMatterReserves()
    {
        throw new Exception();
    }
    
    private void ShowGuardianChains()
    {
        foreach (var _guardian in FindObjectsOfType<Guardian>())
        {
            _guardian.ShowChain();
        }
    }

    private IEnumerator NewMatchRoutine()
    {
        IsMyTurn = DecideWhoPlaysFirst();
        doIPlayFirst = IsMyTurn;
        ShouldIPlaceStartingWall = !IsMyTurn;

        yield return HandlePlacingLifeForceAndGuardian();
        yield return HandlePlaceRestOfTheCards();

        if (ShouldIPlaceStartingWall)
        {
            PlaceStartingWall();
        }

        yield return new WaitForSeconds(1f);
        AbilityCardsManagerBase.Instance.Setup();
        yield return new WaitForSeconds(1f);
        FinishedSetup?.Invoke();
        if (!doIPlayFirst)
        {
            GameState = GameplayState.Waiting;
        }
    }

    protected virtual bool DecideWhoPlaysFirst()
    {
        throw new NotImplementedException();
    }

    protected virtual IEnumerator WaitUntilTheEndOfTurn()
    {
        throw new NotImplementedException();
    }

    public virtual void Resign()
    {
        throw new NotImplementedException();
    }

    public virtual void StopGame(bool _didIWin)
    {
        throw new NotImplementedException();
    }

    public virtual void EndTurn(bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public void TryUnchainGuardian()
    {
        if (GameState != GameplayState.Playing)
        {
            return;
        }

        if (!FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My).IsChained)
        {
            DialogsManager.Instance.ShowOkDialog("Guardian is already unchained");
            return;
        }

        int _price = UnchainingGuardianPrice - StrangeMatterCostChange;
        if (IsAbilityActive<Famine>())
        {
            DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
            return;
        }

        if (MyPlayer.StrangeMatter < _price && !GameplayCheats.HasUnlimitedGold)
        {
            DialogsManager.Instance.ShowOkDialog($"You don't have enough strange matter, this action requires {_price}");
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog($"Spend {_price} to unchain guardian??", () => { YesUnchain(_price); });
    }

    private void YesUnchain(int _price)
    {
        MyPlayer.RemoveStrangeMatter(_price);
        UnchainGuardian();
        MyPlayer.Actions--;
    }

    public virtual void UnchainGuardian(bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    protected virtual IEnumerator HandlePlacingLifeForceAndGuardian()
    {
        throw new NotImplementedException();
    }

    protected virtual IEnumerator HandlePlaceRestOfTheCards()
    {
        throw new NotImplementedException();
    }

    public virtual void PlaceStartingWall()
    {
        throw new NotImplementedException();
    }

    public virtual void PlaceCard(CardBase _card, int _positionId, bool _dontCheckIfPlayerHasIt = false, bool _tellRoom=true)
    {
        throw new NotImplementedException();
    }

    public virtual void AddAbilityToPlayer(bool _isMyPlayer, int _abilityId, bool _tellRoom=true)
    {
        throw new NotImplementedException();
    }

    public virtual void AddAbilityToShop(int _abilityId,bool _tellRoom=true)
    {
        throw new NotImplementedException();
    }

    public virtual void ExecuteCardAction(CardAction _action, bool _tellOpponent = true)
    {
        throw new NotImplementedException();
    }
    
    public virtual void SpawnBombEffect(int _placeId)
    {
        throw new NotImplementedException();
    }

    public virtual void TakeLoot(bool _tellRoom=true)
    {
        throw new NotImplementedException();
    }

    public virtual void ForceUpdatePlayerActions(bool _tellRoom=true)
    {
        throw new NotImplementedException();
    }

    public virtual void BuyMinion(CardBase _cardBase, int _cost, Action _callBack = null,bool _placeMinion=true, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void BuildWall(CardBase _cardBase, int _cost, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void ManageBlockaderAbility(bool _status, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void SelectPlaceForSpecialAbility(int _startingPosition, int _range, PlaceLookFor _lookForPlace,
        CardMovementType _movementType, bool _includeSelf, LookForCardOwner _lookFor, Action<int> _callBack,
        bool _ignoreMarkers = true, bool _ignoreWalls=false)
    {
        throw new NotImplementedException();
    }

    public virtual void ChangeOwnerOfCard(int _placeId, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void OpponentCardDiedInMyPosition(int _cardId, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void ChangeMovementForCard(int _placeId, bool _status, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void TellOpponentSomething(string _key, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void ChangeCanFlyToDodge(int _cardId, bool _status,bool _isCardMy,  bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void ForceResponseAction(int _cardId)
    {
        throw new NotImplementedException();
    }

    public virtual void TryDestroyMarkers(List<int> _places, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void BombExploded(int _placeId, bool _includeCenter=true, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void HandleSnowUltimate(bool _status, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void ActivateAbility(int _cardId, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void BuyAbilityFromShop(int _abilityId, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void BuyAbilityFromHand(int _abilityId, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual int PushCardForward(int _startingPlace, int _endingPlace, int _chanceForPush = 100, bool _tryToMoveSelf = false)
    {
        throw new NotImplementedException();
    }

    public virtual void PushCardBack(int _startingPlace, int _endingPlace, int _chanceForPush = 100)
    {
        throw new NotImplementedException();
    }

    public virtual void ManageBombExplosion(bool _state, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void ManageChangeOrgAttack(int _amount, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void PlaceAbilityOnTable(int _abilityId)
    {
        throw new NotImplementedException();
    }

    public virtual void PlaceAbilityOnTable(int _abilityId, int _placeId, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void ReturnAbilityFromActivationField(int _abilityId, bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void BuyMatter( bool _tellRoom = true)
    {
        throw new NotImplementedException();
    }

    public virtual void FinishedReductionAction(bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void CheckForBombInMarkers(List<int> _markers, Action<List<int>> _callBack, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void MarkMarkerAsBomb(int _placeId, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void TellOpponentToRemoveStrangeMatter(int _amount, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void VetoCard(AbilityCard _card, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void TellOpponentToPlaceFirstCardCasters(bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void OpponentPlacedFirstAbilityForCasters(bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void FinishCasters(bool _tellRoom = true)
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

    public virtual void UpdateHealth(int _cardId, bool _isMine, int _health, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void DestroyBombWithoutActivatingIt(int _cardId, bool _isMy, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void TellOpponentToUseStealth(int _cardId, int _stealthFromPlace, int _placeMinionsFrom, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void ChangeSprite(int _cardPlace, int _cardId, int _spriteId, bool _showPlaceAnimation=false, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void RequestResponseAction(int _cardId, bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void PlayAudioOnBoth(string _key, CardBase _card, bool _tellRoom = true)
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

    public virtual void TellOpponentThatIUsedUltimate(bool _tellRoom = true)
    {
        throw new Exception();
    }

    public virtual void SetTaxCard(int _id)
    {
        throw new Exception();
    }

    public virtual void ActivatedTaxedCard()
    {
        throw new Exception();
    }

    public virtual void TellOpponentToUpdateMyStrangeMatter()
    {
        throw new Exception();
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
    
    public void FinishEffect<T>() where T : MonoBehaviour
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

    public void ChangeLootAmountForMe(int _amount)
    {
        if (FirebaseManager.Instance.RoomHandler.IsOwner)
        {
            LootChanges[0] += _amount;
        }
        else
        {
            LootChanges[1] += _amount;
        }
    }

    public List<Card> GetAllCards()
    {
        return FindObjectsOfType<Card>().ToList();
    }

    public List<Minion> GetAllMinions()
    {
        return FindObjectsOfType<Minion>().ToList();
    }
}
