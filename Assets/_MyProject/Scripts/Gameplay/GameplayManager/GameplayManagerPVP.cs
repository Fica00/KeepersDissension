using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using FirebaseMultiplayer.Room;
using GameplayActions;

public class GameplayManagerPVP : GameplayManager
{
    private Action<List<int>> opponentCheckForMarkerCallback;
    private RoomHandler roomHandler;
    [SerializeField] private GameObject inputBlocker;
    private bool wasLastBomberMine;


    protected override void Awake()
    {
        base.Awake();
        roomHandler = FirebaseManager.Instance.RoomHandler;

        if (roomHandler.IsTestingRoom)
        {
            AmountOfAbilitiesPlayerCanBuy = 1000;
        }
    }

    private void OnEnable()
    {
        DataManager.Instance.PlayerData.CurrentRoomId = FirebaseManager.Instance.RoomHandler.RoomData.Id;
        RoomHandler.OnNewAction += ProcessAction;
        RoomHandler.OnPlayerLeft += OpponentLeftRoom;
    }

    private void OnDisable()
    {
        RoomHandler.OnNewAction -= ProcessAction;
        RoomHandler.OnPlayerLeft -= OpponentLeftRoom;
        MyPlayer.UpdatedStrangeMatter -= TellOpponentThatIUpdatedWhiteStrangeMatter;
        WhiteStrangeMatter.UpdatedAmountInEconomy -= TellOpponentToUpdateWhiteStrangeMatterReserves;
    }

    protected override IEnumerator ExecuteOldActions()
    {
        IsExecutingOldActions = true;
        inputBlocker.SetActive(true);
        List<string> _executedActions = new List<string>();
        bool _finished = false;
        StartCoroutine(ExecuteActions());
        yield return new WaitUntil(() => _finished);

        IEnumerator ExecuteActions()
        {
            int _amountOfExecutedActions = 0;
            foreach (var _action in FirebaseManager.Instance.RoomHandler.RoomData.Actions.Values.ToList())
            {
                if (_executedActions.Contains(_action.Id))
                {
                    continue;
                }

                if (_action.IsMine && _action.Data.Type==ActionType.OpponentSaidThatBombExploded)
                {
                    continue;
                }

                ProcessAction(_action);
                if (_action.Data.Type is not (ActionType.OpponentSaidToPlayAudio or ActionType.OpponentsBlockaderPassive))
                {
                    lastAction = _action;
                }
                _executedActions.Add(_action.Id);
                yield return new WaitForSeconds(GetWaitTime(_action.Data.Type));
                if (_action.Data.Type == ActionType.FinishedPlacingStartingCards)
                {
                    ShowGuardianChains();
                }
                _amountOfExecutedActions++;
            }

            if (_amountOfExecutedActions==0)
            {
                inputBlocker.SetActive(false);
                IsExecutingOldActions = false;
                _finished = true;
            }
            else
            {
                StartCoroutine(ExecuteActions());
            }
        }
    }

    private float GetWaitTime(ActionType _type)
    {
        switch (_type)
        {
            case ActionType.None:
                return 0;
            case ActionType.PlaceCard:
                return 1;
            case ActionType.ExecuteCardAction:
                return 1;
            case ActionType.OpponentBoughtMinion:
                return 1;
            case ActionType.OpponentBuiltWall:
                return 1;
            case ActionType.OpponentActivatedAbility:
                return 1;
            case ActionType.OpponentReturnedAbilityToPlace:
                return 1;
            case ActionType.OpponentReturnAbilityToHand:
                return 1;
            case ActionType.OpponentWantsToDestroyBombWithoutActivatingIt:
                return 1;
            case ActionType.OpponentSaidToUseStealth:
                return 1;
            case ActionType.OpponentPlacedVetoedCard:
                return 1;
            default:
                return 0;
        }
    }

    private void ProcessAction(ActionData _action)
    {
        switch (_action.Data.Type)
        {
            case ActionType.None:
                break;
            case ActionType.PlaceCard:
                PlaceCard _placeCard = JsonConvert.DeserializeObject<PlaceCard>(_action.JsonData);
                if (_action.IsMine)
                {
                    var _card = GetCard(_placeCard.CardId, true);
                    PlaceCard(_card, _placeCard.PositionId, _placeCard.DontCheckIfPlayerHasIt, false);
                }
                else
                {
                    OpponentPlacedCard(_placeCard.CardId, _placeCard.PositionId, _placeCard.DontCheckIfPlayerHasIt);
                }

                break;
            case ActionType.Resign:
                StopGame(!_action.IsMine);
                break;
            case ActionType.FinishedPlacingLifeForce:
                HasOpponentPlacedStartingCards = true;
                break;
            case ActionType.FinishedPlacingStartingCards:
                HasOpponentPlacedStartingCards = true;
                break;
            case ActionType.AddAbilityToPlayer:
                AddAbilityCardToPlayer _addAbilityToPlayer = JsonConvert.DeserializeObject<AddAbilityCardToPlayer>(_action.JsonData);
                if (_action.IsMine)
                {
                    AddAbilityToPlayer(true, _addAbilityToPlayer.AbilityId, false);
                }
                else
                {
                    MasterAddedAbilityToPlayer(_addAbilityToPlayer.IsMyPlayer, _addAbilityToPlayer.AbilityId);
                }

                break;
            case ActionType.AddAbilityToShop:
                AddAbilityToShop _addAbilityToShop = JsonConvert.DeserializeObject<AddAbilityToShop>(_action.JsonData);
                if (_action.IsMine)
                {
                    AddAbilityToShop(_addAbilityToShop.AbilityId, false);
                }
                else
                {
                    MasterAddedAbilityToShop(_addAbilityToShop.AbilityId);
                }

                break;
            case ActionType.ExecuteCardAction:
                ExecuteCardAction _executeCardAction = JsonConvert.DeserializeObject<ExecuteCardAction>(_action.JsonData);

                if (_action.IsMine)
                {
                    ExecuteCardAction(JsonConvert.DeserializeObject<CardAction>(_executeCardAction.JsonData), false);
                }
                else
                {
                    OpponentExecutedAction(_executeCardAction.JsonData);
                }

                break;
            case ActionType.OpponentTookLoot:
                if (_action.IsMine)
                {
                    TakeLoot(false);
                }
                else
                {
                    OpponentLootedMe();
                }

                break;
            case ActionType.GiveLoot:
                GiveLoot _giveLoot = JsonConvert.DeserializeObject<GiveLoot>(_action.JsonData);
                if (_action.IsMine)
                {
                    GiveLoot(false);
                }
                else
                {
                    OpponentGiveYouLoot(_giveLoot.Amount);
                }

                break;
            case ActionType.OpponentFinishedAttackResponse:
                if (_action.IsMine)
                {
                    EndTurn(false);
                }
                else
                {
                    OpponentFinishedAttackResponse();
                }

                break;
            case ActionType.OpponentEndedTurn:
                if (_action.IsMine)
                {
                    EndTurn(false);
                }
                else
                {
                    OpponentFinishedHisMove();
                }

                break;
            case ActionType.OpponentUpdatedHisStrangeMatter:
                OpponentUpdateWhiteMatter _opponentUpdatedMatter = JsonConvert.DeserializeObject<OpponentUpdateWhiteMatter>(_action.JsonData);
                if (_action.IsMine)
                {
                    TellOpponentThatIUpdatedWhiteStrangeMatter(false);
                }
                else
                {
                    OpponentUpdatedWhiteStrangeMatter(_opponentUpdatedMatter.Amount);
                }

                break;
            case ActionType.OpponentUpdatedStrangeMatterInReserve:
                OpponentUpdatedWhiteMatterInReserve _opponentUpdatedMatterInReserve =
                    JsonConvert.DeserializeObject<OpponentUpdatedWhiteMatterInReserve>(_action.JsonData);
                if (_action.IsMine)
                {
                    TellOpponentToUpdateWhiteStrangeMatterReserves(false);
                }
                else
                {
                    UpdateWhiteStrangeMatterInReserve(_opponentUpdatedMatterInReserve.Amount);
                }

                break;
            case ActionType.ForceUpdateOpponentAction:
                ForceUpdateOpponentAction _forceUpdateOpponentAction = JsonConvert.DeserializeObject<ForceUpdateOpponentAction>(_action.JsonData);
                if (_action.IsMine)
                {
                    ForceUpdatePlayerActions(false);
                }
                else
                {
                    OpponentForcedActionsUpdate(_forceUpdateOpponentAction.Amount);
                }

                break;
            case ActionType.OpponentBoughtMinion:
                OpponentBoughtMinion _opponentBoughtMinion = JsonConvert.DeserializeObject<OpponentBoughtMinion>(_action.JsonData);

                if (_action.IsMine)
                {
                    BuyMinion(GetCard(_opponentBoughtMinion.CardId, false), _opponentBoughtMinion.Cost, null, _opponentBoughtMinion.PlaceMinion,
                        false);
                }
                else
                {
                    OpponentBoughtMinion(_opponentBoughtMinion.CardId, _opponentBoughtMinion.Cost, _opponentBoughtMinion.PositionId,
                        _opponentBoughtMinion.PlaceMinion);
                }

                break;
            case ActionType.OpponentBuiltWall:
                OpponentBuiltWall _opponentBuiltWall = JsonConvert.DeserializeObject<OpponentBuiltWall>(_action.JsonData);
                if (_action.IsMine)
                {
                    BuildWall(GetCard(_opponentBuiltWall.CardId, false), _opponentBuiltWall.Cost, false);
                }
                else
                {
                    OpponentBuiltWall(_opponentBuiltWall.CardId, _opponentBuiltWall.Cost, _opponentBuiltWall.PositionId);
                }

                break;
            case ActionType.OpponentUnchainedGuardian:
                if (_action.IsMine)
                {
                    UnchainGuardian(false);
                }
                else
                {
                    OpponentUnchainedGuardian();
                }

                break;
            case ActionType.OpponentsBlockaderPassive:
                OpponentsBlockaderPassive _blockaderPassive = JsonConvert.DeserializeObject<OpponentsBlockaderPassive>(_action.JsonData);
                if (_action.IsMine)
                {
                    ManageBlockaderAbility(_blockaderPassive.Status, false);
                }
                else
                {
                    OpponentsBlockaderPassive(_blockaderPassive.Status);
                }

                break;
            case ActionType.TellOpponentSomething:
                TellOpponentSomething _tellOpponentSomething = JsonConvert.DeserializeObject<TellOpponentSomething>(_action.JsonData);
                if (_action.IsMine)
                {
                    TellOpponentSomething(_tellOpponentSomething.Message, false);
                }
                else
                {
                    OpponentSaidSomething(_tellOpponentSomething.Message);
                }

                break;
            case ActionType.ChangeOwner:
                ChangeOwner _changeOwner = JsonConvert.DeserializeObject<ChangeOwner>(_action.JsonData);
                if (_action.IsMine)
                {
                    ChangeOwnerOfCard(_changeOwner.PlaceId, false);
                }
                else
                {
                    OpponentRequestedChangeOfCardOwner(_changeOwner.PlaceId);
                }

                break;
            case ActionType.MyCardDiedInOpponentPossession:
                MyCardDiedInOpponentsPossession _cardDiedInMyPossession =
                    JsonConvert.DeserializeObject<MyCardDiedInOpponentsPossession>(_action.JsonData);
                if (_action.IsMine)
                {
                    OpponentCardDiedInMyPosition(_cardDiedInMyPossession.CardId, false);
                }
                else
                {
                    OpponentSaidThatTheMyCardInHisPositionDied(_cardDiedInMyPossession.CardId);
                }

                break;
            case ActionType.OpponentChangedMovement:
                OpponentChangedMovementForCard _changedMovement = JsonConvert.DeserializeObject<OpponentChangedMovementForCard>(_action.JsonData);
                if (_action.IsMine)
                {
                    ChangeMovementForCard(_changedMovement.PlaceId, _changedMovement.Status, false);
                }
                else
                {
                    OpponentChangedMovementForCard(_changedMovement.PlaceId, _changedMovement.Status);
                }

                break;
            case ActionType.OpponentChangedCanFlyToDodge:
                OpponentChangedCanFlyToDodge _flyToDodge = JsonConvert.DeserializeObject<OpponentChangedCanFlyToDodge>(_action.JsonData);
                if (_action.IsMine)
                {
                    HandleChangeCanFlyToDodge(_flyToDodge.CardId, _flyToDodge.Status, _flyToDodge.IsEffectedCardMy);
                }
                else
                {
                    HandleChangeCanFlyToDodge(_flyToDodge.CardId, _flyToDodge.Status, !_flyToDodge.IsEffectedCardMy);
                }

                break;
            case ActionType.OpponentWantsToTryAndDestroyMarkers:
                OpponentWantsToTryAndDestroyMarkers _opponentWantsToTryAndDestroyMarkers =
                    JsonConvert.DeserializeObject<OpponentWantsToTryAndDestroyMarkers>(_action.JsonData);
                if (_action.IsMine)
                {
                    List<int> _places = JsonConvert.DeserializeObject<List<int>>(_opponentWantsToTryAndDestroyMarkers.JsonData);
                    TryDestroyMarkers(_places, false);
                }
                else
                {
                    OpponentWantsToTryAndDestroyMarkers(_opponentWantsToTryAndDestroyMarkers.JsonData);
                }

                break;
            case ActionType.OpponentMarkedBomb:
                OpponentMarkedBomb _opponentMarkedBomb = JsonConvert.DeserializeObject<OpponentMarkedBomb>(_action.JsonData);
                if (_action.IsMine)
                {
                    MarkMarkerAsBomb(_opponentMarkedBomb.PlaceId, false);
                }
                else
                {
                    OpponentMarkedBomb(_opponentMarkedBomb.PlaceId);
                }

                break;
            case ActionType.OpponentFinishedReductionAction:
                if (_action.IsMine)
                {
                    FinishedReductionAction(false);
                }
                else
                {
                    OpponentFinishedReductionAction();
                }

                break;
            case ActionType.OpponentSaidThatBombExploded:
                OpponentSaidThatBombExploded _opponentSaidThatBombExploded =
                    JsonConvert.DeserializeObject<OpponentSaidThatBombExploded>(_action.JsonData);
                if (_action.IsMine)
                {
                    BombExploded(_opponentSaidThatBombExploded.PlaceId, _opponentSaidThatBombExploded.IncludeCenter, false);
                }
                else
                {
                    OpponentSaidThatBombExploded(_opponentSaidThatBombExploded.PlaceId, _opponentSaidThatBombExploded.IncludeCenter);
                }

                break;
            case ActionType.OpponentUsedSnowUltimate:
                OpponentUsedSnowUltimate _opponentUsedSnowUltimate = JsonConvert.DeserializeObject<OpponentUsedSnowUltimate>(_action.JsonData);
                if (_action.IsMine)
                {
                    HandleSnowUltimate(_opponentUsedSnowUltimate.Status, false);
                }
                else
                {
                    OpponentUsedSnowUltimate(_opponentUsedSnowUltimate.Status);
                }

                break;
            case ActionType.OpponentActivatedAbility:
                OpponentActivatedAbility _opponentActivatedAbility = JsonConvert.DeserializeObject<OpponentActivatedAbility>(_action.JsonData);
                if (_action.IsMine)
                {
                    ActivateAbility(_opponentActivatedAbility.CardId, false);
                }
                else
                {
                    OpponentActivatedAbility(_opponentActivatedAbility.CardId, _opponentActivatedAbility.Place);
                }

                break;
            case ActionType.OpponentBoughtAbilityFromShop:
                OpponentBoughtAbilityFromShop _opponentBoughtAbilityFromShop =
                    JsonConvert.DeserializeObject<OpponentBoughtAbilityFromShop>(_action.JsonData);
                if (_action.IsMine)
                {
                    BuyAbilityFromShop(_opponentBoughtAbilityFromShop.AbilityId, false);
                }
                else
                {
                    OpponentBoughtAbilityFromShop(_opponentBoughtAbilityFromShop.AbilityId);
                }

                break;
            case ActionType.OpponentBoughtAbilityFromHand:
                OpponentBoughtAbilityFromHand _opponentBoughtAbilityFromHand =
                    JsonConvert.DeserializeObject<OpponentBoughtAbilityFromHand>(_action.JsonData);
                if (_action.IsMine)
                {
                    BuyAbilityFromHand(_opponentBoughtAbilityFromHand.AbilityId, false);
                }
                else
                {
                    OpponentBoughtAbilityFromHand(_opponentBoughtAbilityFromHand.AbilityId);
                }

                break;
            case ActionType.OpponentChangedBomberExplode:
                OpponentChangedBomberExplode _opponentChangedBomberExplode =
                    JsonConvert.DeserializeObject<OpponentChangedBomberExplode>(_action.JsonData);
                if (_action.IsMine)
                {
                    ManageBombExplosion(_opponentChangedBomberExplode.State, false);
                }
                else
                {
                    OpponentChangedBomberExplode(_opponentChangedBomberExplode.State);
                }

                break;
            case ActionType.OpponentChangedOrgesDamage:
                OpponentChangedOrgesDamage _opponentChangedOrgesDamage = JsonConvert.DeserializeObject<OpponentChangedOrgesDamage>(_action.JsonData);
                if (_action.IsMine)
                {
                    ManageChangeOrgAttack(_opponentChangedOrgesDamage.Amount, false);
                }
                else
                {
                    OpponentChangedOrgesDamage(_opponentChangedOrgesDamage.Amount);
                }

                break;
            case ActionType.OpponentReturnedAbilityToPlace:
                OpponentReturnedAbilityToPlace _opponentReturnedAbilityToPlace =
                    JsonConvert.DeserializeObject<OpponentReturnedAbilityToPlace>(_action.JsonData);
                if (_action.IsMine)
                {
                    PlaceAbilityOnTable(_opponentReturnedAbilityToPlace.AbilityId, _opponentReturnedAbilityToPlace.PlaceId, false);
                }
                else
                {
                    OpponentReturnedAbilityToPlace(_opponentReturnedAbilityToPlace.AbilityId, _opponentReturnedAbilityToPlace.PlaceId);
                }

                break;
            case ActionType.OpponentReturnAbilityToHand:
                OpponentReturnAbilityToHand _opponentReturnAbilityToHand =
                    JsonConvert.DeserializeObject<OpponentReturnAbilityToHand>(_action.JsonData);
                if (_action.IsMine)
                {
                    ReturnAbilityFromActivationField(_opponentReturnAbilityToHand.AbilityId, false);
                }
                else
                {
                    OpponentReturnAbilityToHand(_opponentReturnAbilityToHand.AbilityId);
                }

                break;
            case ActionType.OpponentBoughtStrangeMatter:
                if (_action.IsMine)
                {
                    BuyMatter(false);
                }
                else
                {
                    OpponentBoughtStrangeMatter();
                }

                break;
            case ActionType.OpponentAskedIfThereIsBombInMarkers:
                OpponentAskedIfThereIsBombInMarkers _opponentAskedIfThereIsBombInMarkers =
                    JsonConvert.DeserializeObject<OpponentAskedIfThereIsBombInMarkers>(_action.JsonData);
                if (_action.IsMine)
                {
                    List<int> _markers = JsonConvert.DeserializeObject<List<int>>(_opponentAskedIfThereIsBombInMarkers.Data);
                    CheckForBombInMarkers(_markers, null, false);
                }
                else
                {
                    OpponentAskedIfThereIsBombInMarkers(_opponentAskedIfThereIsBombInMarkers.Data);
                }

                break;
            case ActionType.OpponentSaidToRemoveStrangeMatter:
                OpponentSaidToRemoveStrangeMatter _opponentSaidToRemoveStrangeMatter =
                    JsonConvert.DeserializeObject<OpponentSaidToRemoveStrangeMatter>(_action.JsonData);
                if (_action.IsMine)
                {
                    TellOpponentToRemoveStrangeMatter(_opponentSaidToRemoveStrangeMatter.Amount, false);
                }
                else
                {
                    OpponentSaidToRemoveStrangeMatter(_opponentSaidToRemoveStrangeMatter.Amount);
                }

                break;
            case ActionType.OpponentToldYouToVetoCardOnField:
                OpponentToldYouToVetoCardOnField _opponentToldYouToVetoCardOnField =
                    JsonConvert.DeserializeObject<OpponentToldYouToVetoCardOnField>(_action.JsonData);
                if (_action.IsMine)
                {
                    AbilityCard _abilityCard = FindObjectsOfType<AbilityCard>().ToList()
                        .Find(_card => _card.Details.Id == _opponentToldYouToVetoCardOnField.CardId);
                    VetoCard(_abilityCard, false);
                }
                else
                {
                    OpponentToldYouToVetoCardOnField(_opponentToldYouToVetoCardOnField.CardId);
                }

                break;
            case ActionType.OpponentWantsMeToActivateFirstAbilityCasters:
                if (_action.IsMine)
                {
                    TellOpponentToPlaceFirstCardCasters(false);
                }
                else
                {
                    OpponentWantsMeToActivateFirstAbilityCasters();
                }

                break;
            case ActionType.TellOpponentThatIPlacedFirstCardForCasters:
                if (_action.IsMine)
                {
                    OpponentPlacedFirstAbilityForCasters(false);
                }
                else
                {
                    TellOpponentThatIPlacedFirstCardForCasters();
                }

                break;
            case ActionType.OpponentSaidFinishCasters:
                if (_action.IsMine)
                {
                    FinishCasters(false);
                }
                else
                {
                    OpponentSaidFinishCasters();
                }

                break;
            case ActionType.OpponentUpdatedHealth:
                OpponentUpdatedHealth _opponentUpdatedHealth = JsonConvert.DeserializeObject<OpponentUpdatedHealth>(_action.JsonData);
                if (_action.IsMine)
                {
                    UpdateHealth(_opponentUpdatedHealth.CardId, _opponentUpdatedHealth.Status, _opponentUpdatedHealth.Health, false);
                }
                else
                {
                    OpponentUpdatedHealth(_opponentUpdatedHealth.CardId, _opponentUpdatedHealth.Status, _opponentUpdatedHealth.Health);
                }

                break;
            case ActionType.OpponentWantsToDestroyBombWithoutActivatingIt:
                OpponentWantsToDestroyBombWithoutActivatingIt _opponentWantsToDestroyBombWithoutActivatingIt =
                    JsonConvert.DeserializeObject<OpponentWantsToDestroyBombWithoutActivatingIt>(_action.JsonData);
                if (_action.IsMine)
                {
                    DestroyBombWithoutActivatingIt(_opponentWantsToDestroyBombWithoutActivatingIt.CardId,
                        _opponentWantsToDestroyBombWithoutActivatingIt.IsMy, false);
                }
                else
                {
                    OpponentWantsToDestroyBombWithoutActivatingIt(_opponentWantsToDestroyBombWithoutActivatingIt.CardId,
                        _opponentWantsToDestroyBombWithoutActivatingIt.IsMy);
                }

                break;
            case ActionType.OpponentSaidToUseStealth:
                OpponentSaidToUseStealth _opponentSaidToUseStealth = JsonConvert.DeserializeObject<OpponentSaidToUseStealth>(_action.JsonData);
                if (_action.IsMine)
                {
                    TellOpponentToUseStealth(_opponentSaidToUseStealth.CardId, _opponentSaidToUseStealth.StealthFromPlace,
                        _opponentSaidToUseStealth.PlaceMinionsFrom, false);
                }
                else
                {
                    OpponentSaidToUseStealth(_opponentSaidToUseStealth.CardId, _opponentSaidToUseStealth.StealthFromPlace,
                        _opponentSaidToUseStealth.PlaceMinionsFrom);
                }

                break;
            case ActionType.OpponentSaidToChangeSprite:
                OpponentSaidToChangeSprite _opponentSaidToChangeSprite = JsonConvert.DeserializeObject<OpponentSaidToChangeSprite>(_action.JsonData);
                if (_action.IsMine)
                {
                    ChangeSprite(_opponentSaidToChangeSprite.CardPlace, _opponentSaidToChangeSprite.CardId, _opponentSaidToChangeSprite.SpriteId,
                        _opponentSaidToChangeSprite.ShowPlaceAnimation, false);
                }
                else
                {
                    OpponentSaidToChangeSprite(_opponentSaidToChangeSprite.CardPlace, _opponentSaidToChangeSprite.CardId,
                        _opponentSaidToChangeSprite.SpriteId, _opponentSaidToChangeSprite.ShowPlaceAnimation);
                }

                break;
            case ActionType.OpponentGotResponseAction:
                if (_action.IsMine)
                {
                    RequestResponseAction(IdOfCardWithResponseAction, false);
                }
                else
                {
                    OpponentGotResponseAction();
                }

                break;
            case ActionType.OpponentSaidToPlayAudio:
                OpponentSaidToPlayAudio _opponentSaidToPlayAudio = JsonConvert.DeserializeObject<OpponentSaidToPlayAudio>(_action.JsonData);
                if (_action.IsMine)
                {
                    PlayAudioOnBoth(_opponentSaidToPlayAudio.Key, GetCard(_opponentSaidToPlayAudio.CardId, false), false);
                }
                else
                {
                    OpponentSaidToPlayAudio(_opponentSaidToPlayAudio.Key, _opponentSaidToPlayAudio.CardId);
                }

                break;
            case ActionType.OpponentUsedHisUltimate:
                if (_action.IsMine)
                {
                    TellOpponentThatIUsedUltimate(false);
                }
                else
                {
                    OpponentUsedHisUltimate();
                }

                break;
            case ActionType.OpponentRespondedForBombQuestion:
                OpponentRespondedForBombQuestion _opponentRespondedForBombQuestion =
                    JsonConvert.DeserializeObject<OpponentRespondedForBombQuestion>(_action.JsonData);
                if (_action.IsMine)
                {
                    //OpponentAskedIfThereIsBombInMarkers(, false);
                }
                else
                {
                    OpponentRespondedForBombQuestion(_opponentRespondedForBombQuestion.BombPlaces);
                }

                break;
            case ActionType.OpponentPlacedVetoedCard:
                OpponentPlacedVetoedCard _opponentPlacedVetoedCard = JsonConvert.DeserializeObject<OpponentPlacedVetoedCard>(_action.JsonData);
                if (_action.IsMine)
                {
                    OpponentToldYouToVetoCardOnField(_opponentPlacedVetoedCard.CardId, false);
                }
                else
                {
                    OpponentPlacedVetoedCard(_opponentPlacedVetoedCard.CardId);
                }

                break;
            case ActionType.SetTaxCard:
                TaxSelectedCard _taxCard = JsonConvert.DeserializeObject<TaxSelectedCard>(_action.JsonData);
                if (!_action.IsMine)
                {
                    FindObjectOfType<Tax>().SetTaxedCard(_taxCard.CardId);
                }

                break;
            case ActionType.ActivatedTaxCard:
                MyPlayer.StrangeMatter++;
                break;
            case ActionType.ForceSetOpponentStrangeMatter:
                if (_action.IsMine)
                {
                    return;
                }
                ForceSetOpponentStrangeMatter _forceSetOpponentStrange =
                    JsonConvert.DeserializeObject<ForceSetOpponentStrangeMatter>(_action.JsonData);
                OpponentPlayer.StrangeMatter = _forceSetOpponentStrange.Amount;
                Debug.Log("Setting opponents strange matter: " + OpponentPlayer.StrangeMatter);
                break;
            case ActionType.TellOpponentToGetResponseAction:
                if (_action.IsMine)
                {
                    return;
                }

                TellOpponentToGetResponseAction _tellOpponentToGetResponse =
                    JsonConvert.DeserializeObject<TellOpponentToGetResponseAction>(_action.JsonData);
                ForceResponseAction(_tellOpponentToGetResponse.CardId);
                break;
            default:
                throw new ArgumentOutOfRangeException("Type: " + _action.Data.Type);
        }

        Card GetCard(int _id, bool _isMy)
        {
            return FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id == _id && _card.My == _isMy);
        }
    }

    private void OpponentLeftRoom(RoomPlayer _obj)
    {
        if (HasGameEnded)
        {
            return;
        }

        UIManager.Instance.ShowOkDialog("Opponent resigned");
        StopGame(true);
    }

    protected override void SetupPlayers()
    {
        List<GameplayPlayerData> _playerData = new();
        
        foreach (var _player in roomHandler.RoomData.RoomPlayers)
        {
            _playerData.Add(new GameplayPlayerData
            {
                PlayerId = _player.Id,
                WhiteMatter = 0
            });
        }

        roomHandler.SetStartingPlayerData(_playerData);
    }

    protected override void SetupTable()
    {
        MyPlayer.Setup(DataManager.Instance.PlayerData.FactionId, true);
        RoomPlayer _opponent = roomHandler.GetOpponent();
        OpponentPlayer.Setup(_opponent.FactionId,false);
        GameplayUI.Instance.SetupTableBackground();
        GameplayUI.Instance.SetupActionAndTurnDisplay();
    }

    protected override bool DecideWhoPlaysFirst()
    {
        RoomPlayer _opponent = roomHandler.GetOpponent();
        int _opponentMatchesPlayed =
            Convert.ToInt32(_opponent.MatchesPlayed);
        if (_opponentMatchesPlayed < DataManager.Instance.PlayerData.MatchesPlayed)
        {
            return false;
        }

        if (_opponentMatchesPlayed > DataManager.Instance.PlayerData.MatchesPlayed)
        {
            return true;
        }

        DateTime _opponentDateCreated = _opponent.DateCrated;
        if (_opponentDateCreated < DataManager.Instance.PlayerData.DateCreated)
        {
            return false;
        }

        if (_opponentDateCreated > DataManager.Instance.PlayerData.DateCreated)
        {
            return true;
        }

        return roomHandler.IsOwner;
    }

    protected override IEnumerator WaitUntilTheEndOfTurn()
    {
        if (IsMyTurn)
        {
            Finished = false;
            GameState = GameplayState.Playing;
            MyPlayer.NewTurn();
            if (lastAction != null)
            {
                MyPlayer.Actions = lastAction.ActionsLeft;
                lastAction = null;
            }
            
            if (MyPlayer.Actions == 0)
            {
                Finished = true;
                IsMyTurn = false;
            }
            yield return new WaitUntil(() => Finished);
            MyPlayer.EndedTurn();
        }
        else
        {
            OpponentFinished = false;
            GameState = GameplayState.Waiting;
            OpponentPlayer.NewTurn();
            if (lastAction != null)
            {
                OpponentPlayer.Actions = lastAction.ActionsLeft;
                lastAction = null;
            }

            if (OpponentPlayer.Actions == 0)
            {
                OpponentFinished = true;
                IsMyTurn = true;
            }
            yield return new WaitUntil(() => OpponentFinished);
            OpponentPlayer.EndedTurn();
        }
        CloseAllPanels();
    }

    protected override void LastPreparation()
    {
        ShowGuardianChains();
        MyPlayer.UpdatedStrangeMatter += TellOpponentThatIUpdatedWhiteStrangeMatter;
        WhiteStrangeMatter.UpdatedAmountInEconomy += TellOpponentToUpdateWhiteStrangeMatterReserves;
    }

    protected void ShowGuardianChains()
    {
        foreach (var _guardian in FindObjectsOfType<Guardian>())
        {
            _guardian.ShowChain();
        }
    }

    private void TellOpponentToUpdateWhiteStrangeMatterReserves()
    {
        TellOpponentToUpdateWhiteStrangeMatterReserves(true);
    }

    private void TellOpponentThatIUpdatedWhiteStrangeMatter()
    {
        TellOpponentThatIUpdatedWhiteStrangeMatter(true);
    }

    public override void Resign()
    {
        StopGame(false);
        roomHandler.AddAction(ActionType.Resign, string.Empty);
    }

    public override void StopGame(bool _didIWin)
    {
        HasGameEnded = true;
        DataManager.Instance.PlayerData.CurrentRoomId=string.Empty;
        StopAllCoroutines();

        GameplayUI.Instance.ShowResult(_didIWin);
    }

    protected override IEnumerator HandlePlacingLifeForceAndGuardian()
    {
        yield return PlaceLifeForceAndGuardian();
        roomHandler.AddAction(ActionType.FinishedPlacingLifeForce, string.Empty);
        yield return new WaitUntil(() => HasOpponentPlacedStartingCards);
        HasOpponentPlacedStartingCards = false;
        yield return new WaitForSeconds(0.5f);
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

    protected override IEnumerator HandlePlaceRestOfTheCards()
    {
        if (IsMyTurn)
        {
            yield return PlaceRestOfStartingCards();
            roomHandler.AddAction(ActionType.FinishedPlacingStartingCards, string.Empty);
            yield return new WaitUntil(() => HasOpponentPlacedStartingCards);
        }
        else
        {
            yield return new WaitUntil(() => HasOpponentPlacedStartingCards);
            yield return PlaceRestOfStartingCards();
            roomHandler.AddAction(ActionType.FinishedPlacingStartingCards, string.Empty);
        }
    }

    private IEnumerator PlaceRestOfStartingCards()
    {
        yield return PlaceKeeper();
        UIManager.Instance.ShowOkBigDialog("Now pick your minions to go into battle alongside you. Each minion has their own attributes and abilities. You can hold down on any card anytime to zoom in on that card and then you can tap that card to flip it over to see more details.");
        yield return RequestCardToBePlaced(14, CardType.Minion);
        yield return RequestCardToBePlaced(13, CardType.Minion);
        yield return RequestCardToBePlaced(12, CardType.Minion);
        yield return RequestCardToBePlaced(10, CardType.Minion);
        yield return RequestCardToBePlaced(9, CardType.Minion);
        yield return RequestCardToBePlaced(8, CardType.Minion);
        MyPlayer.HideCards();

        IEnumerator PlaceKeeper()
        {
            UIManager.Instance.ShowOkDialog("Select which side of your Lifeforce that you, the Keeper, will start.");
            List<TablePlaceHandler> _availablePlaces = new List<TablePlaceHandler>
            {
                TableHandler.GetPlace(10),
                TableHandler.GetPlace(12)
            };

            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.green);
            }

            CardTableInteractions.OnPlaceClicked += SelectPlace;
            bool _hasSelectedPlace = false;
            int _selectedPlaceId = 0;

            yield return new WaitUntil(() => _hasSelectedPlace);

            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.white);
            }
            
            Card _keeperCard = FindObjectsOfType<Keeper>().ToList().Find(_guardian => _guardian.My);
            PlaceCard(_keeperCard,_selectedPlaceId);

            void SelectPlace(TablePlaceHandler _place)
            {
                if (!_availablePlaces.Contains(_place))
                {
                    return;
                }

                CardTableInteractions.OnPlaceClicked -= SelectPlace;
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
        Place(29);
        Place(30);
        Place(31);
        Place(32);
        Place(33);
        Place(34);
        Place(35);

        void Place(int _positionId)
        {
            TablePlaceHandler _tablePlaceHandler = TableHandler.GetPlace(_positionId);
            if (_tablePlaceHandler.IsOccupied)
            {
                return;
            }
            CardBase _selectedCard = MyPlayer.GetCard(CardType.Wall);
            PlaceCard(_selectedCard, _positionId);
        }
    }

    public override void PlaceCard(CardBase _card, int _positionId, bool _dontCheckIfPlayerHasIt=false, bool _tellRoom=true)
    {
        int _cardId;
        if (_card is AbilityCard _abilityCard)
        {
            _cardId = _abilityCard.Details.Id;
        }
        else
        {
            _cardId = ((Card)_card).Details.Id;
        }

        if (_card is AbilityCard)
        {
            _card.Display.Setup(_card as AbilityCard);
        }
        else
        {
            _card.Display.Setup(_card as Card);
        }

        PlaceCard _placeCard = new PlaceCard
        {
            CardId = _cardId, PositionId = _positionId, DontCheckIfPlayerHasIt = _dontCheckIfPlayerHasIt
        };
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.PlaceCard, JsonConvert.SerializeObject(_placeCard));
        }
        
        PlaceCard(MyPlayer, _cardId, _positionId,_dontCheckIfPlayerHasIt);
    }
    
    private void PlaceCard(GameplayPlayer _player, int _cardId, int _positionId, bool _dontCheckIfPlayerHasIt = false)
    {
        CardBase _card;
        if (!_dontCheckIfPlayerHasIt)
        {
            _card = _player.GetCard(_cardId);
        }
        else
        {
            _card = FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id == _cardId);
            if (_card==null)
            {
                _card = CardsManager.Instance.CreateCard(_cardId, false);
            }
        }
        
        if (_card==null)
        {
            return;
        }

        if (!_dontCheckIfPlayerHasIt)
        {
            _player.RemoveCardFromDeck(_cardId);
        }
        
        _card.PositionOnTable(TableHandler.GetPlace(_positionId));
        OnPlacedCard?.Invoke(_card);
    }
    
    public override void AddAbilityToPlayer(bool _isMyPlayer, int _abilityId,bool _tellRoom=true)
    {
        HandleAddAbilityToPlayer(_isMyPlayer,_abilityId);
        AddAbilityCardToPlayer _abilityData = new AddAbilityCardToPlayer { IsMyPlayer = _isMyPlayer, AbilityId = _abilityId };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.AddAbilityToPlayer, JsonConvert.SerializeObject(_abilityData));
        }
    }

    private void HandleAddAbilityToPlayer(bool _isMyPlayer, int _abilityId)
    {
        GameplayPlayer _player = _isMyPlayer ? MyPlayer : OpponentPlayer;
        AbilityCard _ability = AbilityCardsManagerBase.Instance.DrawAbilityCard(_abilityId);
        if (_ability==null)
        {
            return;
        }

        _player.AddCardToDeck(_ability);
        _ability.SetIsMy(_isMyPlayer);
        AbilityCardsManagerBase.Instance.RemoveAbility(_ability);
    }

    public override void AddAbilityToShop(int _abilityId,bool _tellRoom=true)
    {
        AddAbilityToShop _data = new AddAbilityToShop { AbilityId = _abilityId };
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.AddAbilityToShop, JsonConvert.SerializeObject(_data));
        }
        MasterAddedAbilityToShop(_abilityId);
    }

    public override void ExecuteCardAction(CardAction _action, bool _tellOpponent = true)
    {
        PlayCardAction(_action);
        if (_tellOpponent)
        {
            ExecuteCardAction _execute = new ExecuteCardAction { JsonData = JsonConvert.SerializeObject(_action) };
            roomHandler.AddAction(ActionType.ExecuteCardAction, JsonConvert.SerializeObject(_execute));
        }
    }
    
    private void PlayCardAction(CardAction _action)
    {
        StartCoroutine(ClosePanelRoutine());
        LastAction = _action;
        switch (_action.Type)
        {
            case CardActionType.Attack:
                ExecuteAttack(_action);
                break;
            case CardActionType.Move:
                ExecuteMove(_action);
                break;
            case CardActionType.SwitchPlace:
                ExecuteSwitchPlace(_action);
                break;
            case CardActionType.MoveAbility:
                ExecuteMoveAbility(_action);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        IEnumerator ClosePanelRoutine()
        {
            yield return new WaitForSeconds(.01f);
            CardActionsDisplay.Instance.Close();
        }
    }

    private void ExecuteMove(CardAction _action)
    {
        TablePlaceHandler _destination = TableHandler.GetPlace(_action.FinishingPlaceId);
        TablePlaceHandler _startingDestination = TableHandler.GetPlace(_action.StartingPlaceId);
        CardBase _movingCard = null;
        foreach (var _possibleCard in _startingDestination.GetCards())
        {
            Card _card = _possibleCard as Card;
            if (_card==null)
            {
                continue;
            }
            if (_action.FirstCardId==_card.Details.Id)
            {
                _movingCard = _possibleCard;
                break;
            }
        }

        if (_movingCard==null)
        {
            return;
        }
        
        GameplayPlayer _player = _action.IsMy ? MyPlayer : OpponentPlayer;

        if (_action.ResetSpeed)
        {
            (_movingCard as Card).Speed = 0;
        }

        if (_destination.ContainsMarker)
        {
            Card _marker = _destination.GetMarker();
            if (!_marker.IsVoid)
            {
                SniperStealth.ReturnDiscoveryCardTo = _action.StartingPlaceId;
                CardBase _cardBase = _destination.GetCardNoWall();
                GameplayPlayer _markerOwner = _cardBase.My ? MyPlayer : OpponentPlayer;
                _markerOwner.DestroyCard(_cardBase);
            }
        }

        _movingCard.MoveToPosition(_destination);
        OnCardMoved?.Invoke(_movingCard,_action.StartingPlaceId,_action.FinishingPlaceId, _action.DidTeleport);
        if (_action.Cost!=0)
        {
            _player.Actions -= _action.Cost;
        }

        PlayMovingSoundEffect(_movingCard);
    }

    private void ExecuteSwitchPlace(CardAction _action)
    {
        TablePlaceHandler _destination = TableHandler.GetPlace(_action.FinishingPlaceId);
        TablePlaceHandler _startingDestination = TableHandler.GetPlace(_action.StartingPlaceId);
        GameplayPlayer _player = _action.IsMy ? MyPlayer : OpponentPlayer;

        List<CardBase> _firstCards = _startingDestination.GetCards();
        List<CardBase> _secondCards = _destination.GetCards();
        CardBase _firstCard = null;
        CardBase _secondCard = null;

        foreach (var _cardBase in _firstCards)
        {
            Card _card = _cardBase as Card;
            if (_card.Details.Id==_action.FirstCardId)
            {
                _firstCard = _card;
                break;
            }
        }
        
        foreach (var _cardBase in _secondCards)
        {
            Card _card = _cardBase as Card;
            if (_card.Details.Id==_action.SecondCardId)
            {
                _secondCard = _card;
                break;
            }
        }
        
        OnGonnaSwitchPlace?.Invoke(_firstCard,_secondCard);

        _firstCard.MoveToPosition(_destination);
        _secondCard.MoveToPosition(_startingDestination);

        if (_action.Cost!=0)
        {
            _player.Actions -= _action.Cost;
        }
        
        OnSwitchedPlace?.Invoke(_firstCard,_secondCard);
    }

    private void ExecuteAttack(CardAction _action)
    {
        if (Truce.IsActive&& _action.CanBeBlocked)
        {
            GameplayPlayer _player = _action.IsMy ? MyPlayer : OpponentPlayer;
            _player.Actions--;
            UIManager.Instance.ShowOkDialog("Attacks are blocked by Truce");
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
            _attackingCard = _attackingCards.Find(_card=> ((Card)_card).Details.Id== _action.FirstCardId) as Card;
            _defendingCard = _defendingCards.Find(_card=> ((Card)_card).Details.Id== _action.SecondCardId) as Card;
        }
        catch
        {
            return;
        }
        
        Vector3 _positionOfDefendingCard = _defendingCard.transform.position;

        if (_attackingCard==null || _defendingCard == null)
        {
            return;
        }

        foreach (var _ability in _defendingCard.SpecialAbilities)
        {
            if (_ability is BomberCard)
            {
                wasLastBomberMine = _defendingCard.My;
            }
        }

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
            
            float _damage = _action.Damage != -1 ? _action.Damage : _attackingCard.Stats.Damage;
            Grounded _grounded = FindObjectOfType<Grounded>();
            bool _usedGrounded = false;
            if (_grounded!=null && _grounded.IsActive(_attackingCard,_defendingCard) && _action.CanBeBlocked)
            {
                _damage = 0;
                _usedGrounded = true;
                if (_defendingCard.My)
                {
                    ChangeMovementForCard(_defendingCard.GetTablePlace().Id,false);
                }
            }
            
            if (_dealDamage)
            {
                //hunter ability
                if (_attackingCard.My)
                {
                    if (Hunter.IsActive && _attackingCard is Keeper _keeper && _keeper.My && _defendingCard is Guardian _guardian && 
                        !_guardian.My)
                    {
                        _damage *= 2;
                    }
                }
                else
                {
                    if (Hunter.IsActiveForOpponent && _attackingCard is Keeper _keeper && !_keeper.My && _defendingCard is Guardian 
                    _guardian && _guardian.My)
                    {
                        _damage *= 2;
                    }
                }
                //hunter ability ends
                
                if (_defendingCard is Keeper)
                {
                    if (Invincible.IsActive && _defendingCard.My&& _action.CanBeBlocked)
                    {
                        UIManager.Instance.ShowOkDialog("Damage blocked by Invincible ability");
                        _damage = 0;
                    }
                    else if (Invincible.IsActiveForOpponent && !_defendingCard.My&& _action.CanBeBlocked)
                    {
                        UIManager.Instance.ShowOkDialog("Damage blocked by Invincible ability");
                        _damage = 0;
                    }
                    else if (Steadfast.IsActive && _defendingCard.My && _attackingCard is Minion && !_attackingCard.My&& _action.CanBeBlocked)
                    {
                        UIManager.Instance.ShowOkDialog("Damage blocked by Steadfast ability");
                        _damage = 0;
                    }
                    else if (Steadfast.IsActiveForOpponent && !_defendingCard.My && _attackingCard is Minion && 
                    _attackingCard.My && _action.CanBeBlocked)
                    {
                        UIManager.Instance.ShowOkDialog("Damage blocked by Steadfast ability");
                        _damage = 0;
                    }

                    if (_action.CanBeBlocked && !_usedGrounded)
                    {
                        Armor _abilityEffect = FindObjectsOfType<AbilityEffect>().ToList().Find(_abilityEffect =>
                            _abilityEffect is Armor &&
                            _abilityEffect.AbilityCard.My == _defendingCard.My) as Armor;
                        if (_abilityEffect !=null && _abilityEffect.IsActive)
                        {
                            _abilityEffect.IsActive = false;
                            _damage--;
                        }
                    }
                }

                if (HighStakes.IsActive && _action.CanBeBlocked)
                {
                    _damage = 8;
                    HighStakes.IsActive = false;
                }

                if (_defendingCard is Minion)
                {
                    foreach (var _ability in _defendingCard.SpecialAbilities)
                    {
                        if (_ability is BlockaderCard _blockaderAbility && _blockaderAbility.CanBlock)
                        {
                            _damage--;
                            _blockaderAbility.CanBlock = false;
                        }
                    }
                }
                
                _defendingCard.Stats.Health -= _damage;
            }

            if (_defendingCard.CanFlyToDodgeAttack&& _action.CanBeBlocked)
            {
                _defendingCard.CanFlyToDodgeAttack = false;
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
                
                UseDelivery(_defendingCard.Details.Id,_defendingCard.GetTablePlace().Id);
                return;
            }

            OnCardAttacked?.Invoke(_attackingCard,_defendingCard, (int)_damage);
            CheckForResponseAction();
            CheckIfDefenderIsDestroyed();

            if (_action.Cost!=0)
            {
                _attackingPlayer.Actions -= _action.Cost;
            }
        }

        void CheckForResponseAction()
        {
            int _idOfDefendingCard = _defendingCard.Details.Id;
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

                    _idOfDefendingCard = _cardOnWall.Details.Id;

                    if (_cardOnWall.My)
                    {
                        ForceResponseAction(_idOfDefendingCard);
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

            if (GameState is not (GameplayState.Playing or GameplayState.Waiting))
            {
                return;
            }

            if (!(_defendingCard.Stats.Health > 0) || _attackingCard.My == _defendingCard.My)
            {
                return;
            }

            if ((Ambush.IsActiveForMe && !_defendingCard.My) || (Ambush.IsActiveForOpponent && _defendingCard.My))
            {
                UIManager.Instance.ShowOkDialog("Ignore next response action activated!");
                Ambush.IsActiveForMe = false;
                Ambush.IsActiveForOpponent = false;
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

            if (_defendingCard.Stats.Health > 0)
            {
                return;
            }


            if (_defendingCard is Keeper _keeper)
            {
                if (Subdued.IsActive)
                {
                    Subdued.IsActive = false;
                }
                if (Explode.IsActive)
                {
                    BombExploded(_defendingCard.GetTablePlace().Id, false);
                }
                
                if (_defendingCard.My)
                {
                    MyPlayer.AddCardToDeck(_defendingCard);
                    PlaceKeeperOnTable(_defendingCard);
                }
                else
                {
                    OpponentPlayer.AddCardToDeck(_defendingCard);
                }

                float _healthToRecover = (_keeper.Details.Stats.Health *_keeper.PercentageOfHealthToRecover)/100;
                int _heal = Mathf.RoundToInt(_healthToRecover + .3f);
                if (Minionized.IsActive)
                {
                    _heal = 1;
                }
                OnKeeperDied?.Invoke(_defendingCard as Keeper);
                _defendingCard.Stats.Health = _heal;
                FindObjectsOfType<Card>().ToList()
                    .Find(_element => _element is LifeForce && _element.My == _defendingCard.My).Stats.Health -= _heal;
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
                    int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChanges[0] : LootChanges[1];
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
                    int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChanges[0] : LootChanges[1];
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
                if (wasLastBomberMine)
                {
                    int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChanges[0] : LootChanges[1];
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
                    int _additionalMatter = FirebaseManager.Instance.RoomHandler.IsOwner ? LootChanges[0] : LootChanges[1];
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
                    GiveLoot();
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
                TakeLoot();
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

    private void TellOpponentToGetResponseAction(int _cardId)
    {
        TellOpponentToGetResponseAction _data = new TellOpponentToGetResponseAction { CardId = _cardId };
        roomHandler.AddAction(ActionType.TellOpponentToGetResponseAction, JsonConvert.SerializeObject(_data));
    }

    private void ExecuteMoveAbility(CardAction _action)
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
            if (_action.FirstCardId==_card.Details.Id)
            {
                _movingCard = _possibleCard;
                break;
            }
        }

        if (_movingCard==null)
        {
            return;
        }
        
        GameplayPlayer _player = _action.IsMy ? MyPlayer : OpponentPlayer;

        if (_destination.ContainsMarker)
        {
            SniperStealth.ReturnDiscoveryCardTo = _action.StartingPlaceId;
            CardBase _cardBase = _destination.GetCardNoWall();
            GameplayPlayer _markerOwner = _cardBase.My ? MyPlayer : OpponentPlayer;
            _markerOwner.DestroyCard(_cardBase);
        }

        _movingCard.MoveToPosition(_destination);
        if (_action.Cost!=0)
        {
            _player.Actions -= _action.Cost;
        }
    }
    
    public override void TakeLoot(bool _tellRoom=true)
    {
        MyPlayer.StrangeMatter += OpponentPlayer.StrangeMatter;
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentTookLoot,string.Empty);
        }
    }

    private void GiveLoot(bool _tellRoom=true)
    {
        StartCoroutine(GiveLootRoutine());
        IEnumerator GiveLootRoutine()
        {
            yield return new WaitForSeconds(2);
            MyPlayer.StrangeMatter = 0;
            GiveLoot _giveLoot = new GiveLoot { Amount = MyPlayer.StrangeMatter };
            if (_tellRoom)
            {
                roomHandler.AddAction(ActionType.GiveLoot, JsonConvert.SerializeObject(_giveLoot));
            }
        }
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

        CardTableInteractions.OnPlaceClicked += SelectPlace;
        bool _hasSelectedPlace = false;
        int _selectedPlaceId = 0;

        yield return new WaitUntil(() => _hasSelectedPlace);

        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.white);
        }

        _callBack?.Invoke(_selectedPlaceId);

        void SelectPlace(TablePlaceHandler _place)
        {
            if (!_availablePlaces.Contains(_place))
            {
                return;
            }

            CardTableInteractions.OnPlaceClicked -= SelectPlace;
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
            if (Comrade.IsActive)
            {
                List<CardBase> _cards = MyPlayer.GetCardsInDeck(CardType.Minion).Cast<CardBase>().ToList();
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

    public override void EndTurn(bool _tellRoom = true)
    {
        if (Casters.IsActive)
        {
            return;
        }
        CloseAllPanels();
        if (GameState == GameplayState.WaitingForAttackResponse)
        {
            return;
        }

        TableHandler.ActionsHandler.ClearPossibleActions();

        if (GameState == GameplayState.AttackResponse)
        {
            GameState = GameplayState.Waiting;
            MyPlayer.Actions=0;
            if (_tellRoom)
            {
                roomHandler.AddAction(ActionType.OpponentFinishedAttackResponse,string.Empty);
            }
            GameplayUI.Instance.ForceActionUpdate(OpponentPlayer.Actions == 0? 1:OpponentPlayer.Actions,false,false);
            Debug.Log("Finished response action");
            AudioManager.Instance.PlaySoundEffect("EndTurn");
            return;
        }

        if (!IsMyTurn)
        {
            return;
        }

        if (GameState != GameplayState.Playing)
        {
            return;
        }

        Finished = true;
        IsMyTurn = false;

        NotificationSender.Instance.SendNotificationToUser(roomHandler.GetOpponent().Id, "Your turn!", "Come back to game!");
        AudioManager.Instance.PlaySoundEffect("EndTurn");
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentEndedTurn,string.Empty);
        }
    }
    
    private void TellOpponentThatIUpdatedWhiteStrangeMatter(bool _tellRoom=true)
    {
        OpponentUpdateWhiteMatter _data = new OpponentUpdateWhiteMatter { Amount = MyPlayer.StrangeMatter };
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentUpdatedHisStrangeMatter, JsonConvert.SerializeObject(_data));
        }
    }

    private void TellOpponentToUpdateWhiteStrangeMatterReserves(bool _tellRoom=true)
    {
        OpponentUpdatedWhiteMatterInReserve _data = new OpponentUpdatedWhiteMatterInReserve { Amount = WhiteStrangeMatter.AmountInEconomy };
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentUpdatedStrangeMatterInReserve, JsonConvert.SerializeObject(_data));
        }
    }

    public override void ForceUpdatePlayerActions(bool _tellRoom = true)
    {
        ForceUpdateOpponentAction _data = new ForceUpdateOpponentAction { Amount = MyPlayer.Actions };
        roomHandler.AddAction(ActionType.ForceUpdateOpponentAction, JsonConvert.SerializeObject(_data));
    }
    
    public override void BuyMinion(CardBase _cardBase, int _cost, Action _callBack=null, bool _placeMinion=true, bool _tellRoom = true)
    {
        GameplayState _gameState = GameState;
        GameState = GameplayState.BuyingMinion;
        int _cardId = _cardBase is Card _card ? _card.Details.Id : ((AbilityCard)_cardBase).Details.Id;
        StartCoroutine(SelectPlace());

        IEnumerator SelectPlace()
        {
            if (_placeMinion)
            {
                yield return StartCoroutine(GetPlaceOnTable(FinishRevive));
            }
            else
            {
                //50 is just a place holder, wont be used anyway
                OpponentBoughtMinion _boughtData = new OpponentBoughtMinion { CardId = _cardId, Cost = _cost, PositionId = 50, PlaceMinion = _placeMinion };
                
                if (_tellRoom)
                {
                    roomHandler.AddAction(ActionType.OpponentBoughtMinion, JsonConvert.SerializeObject(_boughtData));
                }
                
                HandleBoughtMinion(_cardBase, _cost, 50, _cardId,_placeMinion);
                GameState = _gameState;
                _callBack?.Invoke();
                if (_cost>0)
                {
                    MyPlayer.Actions--;
                }
            }

            void FinishRevive(int _positionId)
            {
                OpponentBoughtMinion _boughtData = new OpponentBoughtMinion { CardId = _cardId, Cost = _cost, PositionId = _positionId, PlaceMinion = 
                _placeMinion };
                roomHandler.AddAction(ActionType.OpponentBoughtMinion, JsonConvert.SerializeObject(_boughtData));
                HandleBoughtMinion(_cardBase, _cost, _positionId, _cardId,_placeMinion);
                GameState = _gameState;
                _callBack?.Invoke();
                if (_cost>0)
                {
                    MyPlayer.Actions--;
                }
            }
        }
    }

    private void HandleBoughtMinion(CardBase _cardBase, int _cost, int _positionId, int _cardId, bool _placeMinion)
    {
        GameplayPlayer _player = _cardBase.My ? MyPlayer : OpponentPlayer;
        if (_player.IsMy)
        {
            _player.RemoveStrangeMatter(_cost);
        }

        if (_placeMinion)
        {
            PlaceCard(_player, _cardId, _positionId);
        }
        
        _cardBase.HasDied = false;
    }

    public override void BuildWall(CardBase _cardBase, int _cost , bool _tellRoom = true)
    {
        GameplayState _state = GameState;
        GameState = GameplayState.BuildingWall;
        int _cardId = _cardBase is Card _card ? _card.Details.Id : ((AbilityCard)_cardBase).Details.Id;
        StartCoroutine(SelectPlace());

        IEnumerator SelectPlace()
        {
            yield return StartCoroutine(GetPlaceOnTable(FinishRevive, true));

            void FinishRevive(int _positionId)
            {
                OpponentBuiltWall _data = new OpponentBuiltWall { CardId = _cardId, Cost = _cost, PositionId = _positionId };
                if (_tellRoom)
                {
                    roomHandler.AddAction(ActionType.OpponentBuiltWall, JsonConvert.SerializeObject(_data));
                }
                
                HandleBuildWall(_cardBase, _cost, _positionId, _cardId);
                GameState = _state;
                if (_cost>0)
                {
                    MyPlayer.Actions--;
                }
            }
        }
    }

    private void HandleBuildWall(CardBase _cardBase, int _cost, int _positionId, int _cardId)
    {
        GameplayPlayer _player = _cardBase.My ? MyPlayer : OpponentPlayer;
        if (_player.IsMy)
        {
            _player.RemoveStrangeMatter(_cost);
        }
        PlaceCard(_player, _cardId, _positionId);
    }

    public override void UnchainGuardian(bool _tellRoom = true)
    {
        HandleUnchainGuardian(true);

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentUnchainedGuardian,string.Empty);
        }
    }

    private void HandleUnchainGuardian(bool _isMy)
    {
        Guardian _guardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My == _isMy);
        _guardian.Unchain();
        if (_isMy)
        {
            OnUnchainedGuardian?.Invoke();
        }
    }

    public override void ManageBlockaderAbility(bool _status, bool _tellRoom = true)
    {
        ChangeBlockaderAbility(true,_status);
        OpponentsBlockaderPassive _data = new OpponentsBlockaderPassive { Status = _status };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentsBlockaderPassive, JsonConvert.SerializeObject(_data));
        }
    }

    private void ChangeBlockaderAbility(bool _isMy, bool _status)
    {
        FindObjectsOfType<BlockaderCard>().ToList().Find(_blockader => _blockader.IsMy==_isMy).CanBlock = _status;
    }
    
    public override void SelectPlaceForSpecialAbility(int _startingPosition, int _range, PlaceLookFor _lookForPlace, CardMovementType _movementType,
        bool _includeSelf, LookForCardOwner _lookFor, Action<int> _callBack, bool _ignoreMarkers = true, bool _ignoreWalls = false)
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
                        if (_cardAtPlace.My && _lookFor == LookForCardOwner.Enemy)
                        {
                            _availablePlaces.Remove(_availablePlace);
                            break;
                        }

                        if (!_cardAtPlace.My && _lookFor == LookForCardOwner.My)
                        {
                            _availablePlaces.Remove(_availablePlace);
                            break;
                        }
                    }
                }
            }

            StartCoroutine(SelectPlace(_availablePlaces, _ignoreWalls, _callBack));
    }

    private IEnumerator SelectPlace(List<TablePlaceHandler> _availablePlaces, bool _ignoreWalls, Action<int> _callBack)
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

        CardTableInteractions.OnPlaceClicked += SelectPlace;
        bool _hasSelectedPlace = false;
        int _selectedPlaceId = 0;

        yield return new WaitUntil(() => _hasSelectedPlace);

        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.white);
        }

        _callBack?.Invoke(_selectedPlaceId);

        void SelectPlace(TablePlaceHandler _place)
        {
            if (!_availablePlaces.Contains(_place))
            {
                return;
            }

            CardTableInteractions.OnPlaceClicked -= SelectPlace;
            _selectedPlaceId = _place.Id;
            _hasSelectedPlace = true;
        }
    }


    public override void TellOpponentSomething(string _text, bool _tellRoom = true)
    {
        TellOpponentSomething _message = new TellOpponentSomething { Message = _text };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.TellOpponentSomething, JsonConvert.SerializeObject(_message));
        }
    }

    public override void ChangeOwnerOfCard(int _placeId, bool _tellRoom = true)
    {
        HandleChangeOwnerOfCard(_placeId);
        ChangeOwner _data = new ChangeOwner { PlaceId = _placeId };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.ChangeOwner, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleChangeOwnerOfCard(int _placeId)
    {
        CardBase _card = TableHandler.GetPlace(_placeId).GetComponentInChildren<CardBase>();
        _card.ChangeOwner();
    }

    public override void OpponentCardDiedInMyPosition(int _cardId, bool _tellRoom = true)
    {
        HandleOpponentCardDiedInMyPosition(_cardId, true);
        MyCardDiedInOpponentsPossession _myCardDiedInOpponentsPossession = new MyCardDiedInOpponentsPossession { CardId = _cardId };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.MyCardDiedInOpponentPossession, JsonConvert.SerializeObject(_myCardDiedInOpponentsPossession));
        }
    }

    private void HandleOpponentCardDiedInMyPosition(int _cardId, bool _isCardMy)
    {
        Card _card = FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id==_cardId && _card.My == _isCardMy);
        GameplayPlayer _currentCardOwner = _card.My ? MyPlayer : OpponentPlayer;
        GameplayPlayer _originalOwner = !_card.My ? MyPlayer : OpponentPlayer;

        RemoveCardFromDestroyedCards(_currentCardOwner);
        RemoveCardFromDestroyedCards(_originalOwner);
        
        _originalOwner.DestroyCard(_card);
        
        void RemoveCardFromDestroyedCards(GameplayPlayer _player)
        {
            _player.TryRemoveCardFromDeck(_card);
        }
    }

    public override void ChangeMovementForCard(int _placeId, bool _status, bool _tellRoom = true)
    {
        OpponentChangedMovementForCard _data = new OpponentChangedMovementForCard { PlaceId = _placeId, Status = _status };
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentChangedMovement, JsonConvert.SerializeObject(_data));
        }
        
        HandleChangeMovementForCard(_placeId,_status);
    }
    
    private void HandleChangeMovementForCard(int _placeId,bool _status)
    {
        CardBase _cardAtPlace = TableHandler.GetPlace(_placeId).GetCardNoWall();
        if (_cardAtPlace==null)
        {
            Debug.Log(1111);
            return;
        }
        
        Debug.Log(_cardAtPlace.gameObject.name,_cardAtPlace.gameObject);
        Debug.Log(_status);
        _cardAtPlace.CanMove = _status;
    }
    
    public override void ChangeCanFlyToDodge(int _cardId, bool _status, bool _isCardMy, bool _tellRoom = true)
    {
        HandleChangeCanFlyToDodge(_cardId,_status,_isCardMy);
        OpponentChangedCanFlyToDodge _data = new OpponentChangedCanFlyToDodge { CardId = _cardId, Status = _status, IsEffectedCardMy = _isCardMy};
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentChangedCanFlyToDodge, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleChangeCanFlyToDodge(int _cardId, bool _status, bool _isMy)
    {
        Card _card = FindObjectsOfType<Card>().ToList()
            .Find(_card => _card.Details.Id == _cardId && _card.My == _isMy);

        _card.CanFlyToDodgeAttack = _status;
    }

    public override void ForceResponseAction(int _cardId)
    {
        IdOfCardWithResponseAction = _cardId;
        MyPlayer.Actions = 1;
        GameState = GameplayState.AttackResponse;
        GameplayUI.Instance.ForceActionUpdate(MyPlayer.Actions, true,true);
        UIManager.Instance.ShowOkDialog("Your warrior survived attack, you get 1 response action");
    }

    public override void TryDestroyMarkers(List<int> _places, bool _tellRoom = true)
    {
        HandleTryToDestroyMarkers(_places);
        OpponentWantsToTryAndDestroyMarkers _data = new OpponentWantsToTryAndDestroyMarkers { JsonData = JsonConvert.SerializeObject(_places) };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentWantsToTryAndDestroyMarkers, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleTryToDestroyMarkers(List<int> _places)
    {
        UsingVisionToDestroyMarkers = true;
        foreach (var _placeId in _places)
        {
            TablePlaceHandler _place = TableHandler.GetPlace(_placeId);
            if (!_place.IsOccupied)
            {
                continue;
            }

            List<CardBase> _cards = _place.GetCards();
            CardBase _cardBase = null;
            foreach (var _card in _cards)
            {
                if (_card is not Marker)
                {
                    continue;
                }
                _cardBase = _card;
                break;
            }

            if (_cardBase==null)
            {
                continue;
            }
            
            GameplayPlayer _player = _cardBase.My ? MyPlayer : OpponentPlayer;
            _player.DestroyCard(_cardBase);
        }
        
        UsingVisionToDestroyMarkers = false;
    }

    public override void MarkMarkerAsBomb(int _placeId, bool _tellRoom = true)
    {
        HandleMarkMarkerAsBomb(_placeId);
        OpponentMarkedBomb _data = new OpponentMarkedBomb { PlaceId = _placeId };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentMarkedBomb, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleMarkMarkerAsBomb(int _placeId)
    {
        TablePlaceHandler _place = TableHandler.GetPlace(_placeId);
        if (!_place.IsOccupied)
        {
            return;
        }

        List<CardBase> _cards = _place.GetCards();
        CardBase _cardBase = null;
        foreach (var _card in _cards)
        {
            if (_card is not Marker)
            {
                continue;
            }
            _cardBase = _card;
            break;
        }

        if (_cardBase==null)
        {
            return;
        }

        Sprite _bombSprite = _cardBase.My ? MyPlayer.FactionSO.BombSprite : OpponentPlayer.FactionSO.BombSprite;
        _cardBase.Display.ChangeSprite(_bombSprite);
        OnFoundBombMarker?.Invoke(_cardBase);
    }
    
    public override void FinishedReductionAction(bool _tellRoom = true)
    {
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentFinishedReductionAction,string.Empty);
        }
    }

    public override void BombExploded(int _placeId, bool _includeCenter=true, bool _tellRoom = true)
    {
        StartCoroutine(BombExplodedRoutine());
        
        IEnumerator BombExplodedRoutine()
        {
            yield return new WaitForSeconds(0.3f);
            HandleBombExploded(_placeId,_includeCenter);
            OpponentSaidThatBombExploded _data = new OpponentSaidThatBombExploded { PlaceId = _placeId , IncludeCenter = _includeCenter};

            if (_tellRoom)
            {
                roomHandler.AddAction(ActionType.OpponentSaidThatBombExploded, JsonConvert.SerializeObject(_data));
            }
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
                if (_cardOnPlace is not Card)
                {
                    continue;
                }

                if (_amountOfCardsOnPlace>1)
                {
                    bool _isScaler = false;
                    foreach (var _ability in _cardOnPlace.SpecialAbilities)
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
                
                Card _card = _cardOnPlace as Card;

                CardAction _action = new CardAction
                {
                    FirstCardId = _card.Details.Id,
                    SecondCardId = _card.Details.Id,
                    StartingPlaceId = _availablePlace.Id,
                    FinishingPlaceId = _availablePlace.Id,
                    Type = CardActionType.Attack,
                    Cost = 0,
                    CanTransferLoot = false,
                    IsMy = false,
                    Damage = 3,
                    CanCounter = false,
                    DiedByBomb = true
                };

                ExecuteCardAction(_action, false);
            }
        }
        
        TableHandler.ActionsHandler.ClearPossibleActions();
    }

    public override void SpawnBombEffect(int _placeId)
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

    public override void HandleSnowUltimate(bool _status, bool _tellRoom = true)
    {
        HandleSnowUltimatePvp(_status,true);
        OpponentUsedSnowUltimate _data = new OpponentUsedSnowUltimate { Status = _status };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentUsedSnowUltimate, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleSnowUltimatePvp(bool _status, bool _didIUse)
    {
        foreach (var _card in FindObjectsOfType<CardBase>().ToList().FindAll(_element=>_element.My!=_didIUse))
        {
            if (_card is not Wall or Marker)
            {
                _card.CanMove = _status;
            }
        }
    }

    public override void ActivateAbility(int _cardId, bool _tellRoom = true)
    {
        AbilityCard _ability =
            FindObjectsOfType<AbilityCard>().ToList().Find(_ability => _ability.Details.Id == _cardId);
        if (_ability.GetTablePlace()==null)
        {
            TableHandler.GetAbilityPosition(PlaceAbility);
        }
        else
        {
            int _placeId = _ability.GetTablePlace().Id;
            HandleActivateAbility(_cardId,true,_placeId);
            OpponentActivatedAbility _data = new OpponentActivatedAbility { CardId = _cardId, Place = _placeId };

            if (_tellRoom)
            {
                roomHandler.AddAction(ActionType.OpponentActivatedAbility, JsonConvert.SerializeObject(_data));
            }
        }
        
        void PlaceAbility(int _placeId)
        {
            if (_placeId==-1)
            {
                UIManager.Instance.ShowOkDialog("There are no empty spaces in ability row");
                return;
            }
            HandleActivateAbility(_cardId,true,_placeId);
            OpponentActivatedAbility _data = new OpponentActivatedAbility { CardId = _cardId, Place = _placeId };
            
            if (_tellRoom)
            {
                roomHandler.AddAction(ActionType.OpponentActivatedAbility, JsonConvert.SerializeObject(_data));
            }
        }
    }

    private void HandleActivateAbility(int _cardId, bool _isMy, int _placeId)
    {
        AbilityCard _ability = FindObjectsOfType<AbilityCard>().ToList().Find(_ability =>
            _ability.Details.Id == _cardId && _ability.My == _isMy);
        _ability.Activate();
        OnActivatedAbility?.Invoke(_ability);
        AudioManager.Instance.PlaySoundEffect("AbilityCardUsed");
    }

    public override void BuyAbilityFromShop(int _abilityId, bool _tellRoom = true)
    {
        HandleBuyAbilityFromShop(_abilityId,true);
        if (_abilityId!=1031)
        {
            if (!(_abilityId==1005 && MyPlayer.Actions==1))
            {
                MyPlayer.Actions--;
            }
        }

        OpponentBoughtAbilityFromShop _data = new OpponentBoughtAbilityFromShop { AbilityId = _abilityId };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentBoughtAbilityFromShop, JsonConvert.SerializeObject(_data));
        }
    
    }

    private void HandleBuyAbilityFromShop(int _abilityId, bool _didIBuy)
    {
        AbilityCard _ability = AbilityCardsManagerBase.Instance.RemoveAbilityFromShop(_abilityId);
        _ability.SetIsMy(_didIBuy);
        GameplayPlayer _player = _didIBuy ? MyPlayer : OpponentPlayer;
        _player.AddOwnedAbility(_abilityId);
        _player.AmountOfAbilitiesPlayerCanBuy--;
        AudioManager.Instance.PlaySoundEffect("AbilityCardPurchased");
    }

    public override void BuyAbilityFromHand(int _abilityId, bool _tellRoom = true)
    {
        HandleBuyAbilityFromHand(_abilityId,true);
        if (_abilityId!=1031)
        {
            if (!(_abilityId==1005 && MyPlayer.Actions==1))
            {
                MyPlayer.Actions--;
            }
        }

        OpponentBoughtAbilityFromHand _data = new OpponentBoughtAbilityFromHand { AbilityId = _abilityId };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentBoughtAbilityFromHand, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleBuyAbilityFromHand(int _abilityId, bool _didIBuy)
    {
        GameplayPlayer _player = _didIBuy ? MyPlayer : OpponentPlayer;
        _player.RemoveAbilityFromDeck(_abilityId);
        _player.AddOwnedAbility(_abilityId);
        _player.AmountOfAbilitiesPlayerCanBuy--;
        AudioManager.Instance.PlaySoundEffect("AbilityCardPurchased");
    }
    
    public override int PushCardForward(int _startingPlace, int _endingPlace,int _chanceForPush=100)
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
            StartCoroutine(DamagePushedCard());
            return -1;
        }

        if (_placeInFrontOfPushedCard.IsAbility)
        {
            StartCoroutine(DamagePushedCard());
            return -1;
        }
        if (_placeInFrontOfPushedCard.GetCard() == null)
        {
            CardBase _pushedCardBase = _pushedCardPlace.GetCard();
            CardAction _moveCardInFront = new CardAction
            {
                FirstCardId = ((Card)_pushedCardBase).Details.Id,
                StartingPlaceId = _pushedCardPlace.Id,
                FinishingPlaceId = _placeInFrontOfPushedCard.Id,
                Type = CardActionType.Move,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false,
                ResetSpeed = true
            };

            ExecuteCardAction(_moveCardInFront);
            return _pushedCardPlace.Id;
        }

        StartCoroutine(DamagePushedCard());
        return -1;

        IEnumerator DamagePushedCard()
        {
            yield return new WaitForSeconds(0.5f);
            CardAction _damage = new CardAction()
            {
                FirstCardId = TableHandler.GetPlace(_endingPlace).GetCard().Details.Id,
                SecondCardId = _pushedCardPlace.GetCard().Details.Id,
                StartingPlaceId = _endingPlace,
                FinishingPlaceId = _pushedCardPlace.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = 1,
                CanCounter = false,
            };
            
            ExecuteCardAction(_damage);
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
            CardBase _pushedCardBase = _pushedCardPlace.GetCard();
            if (!_pushedCardBase.CanMove || _placeBehindOfPushedCard.IsAbility)
            {
                StartCoroutine(DamagePushedCard());
                return;
            }
            CardAction _moveCardInFront = new CardAction
            {
                FirstCardId = _pushedCardPlace.GetCardNoWall().Details.Id,
                StartingPlaceId = _pushedCardPlace.Id,
                FinishingPlaceId = _placeBehindOfPushedCard.Id,
                Type = CardActionType.Move,
                Cost = 0,
                IsMy = true,
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
                FirstCardId = _pushedCardPlace.GetCard().Details.Id,
                SecondCardId = _pushedCardPlace.GetCard().Details.Id,
                StartingPlaceId = _pushedCardPlace.Id,
                FinishingPlaceId = _pushedCardPlace.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = 1,
                CanCounter = false,
            };
            
            ExecuteCardAction(_damage);
        }
    }
    
    public override void ManageBombExplosion(bool _state, bool _tellRoom = true)
    {
        HandleBombExplosion(true, _state);
        OpponentChangedBomberExplode _data = new OpponentChangedBomberExplode { State = _state };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentChangedBomberExplode, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleBombExplosion(bool _isMy, bool _state)
    {
        FindObjectsOfType<BomberCard>().ToList().Find(_bomb => _bomb.IsMy==_isMy).ExplodeOnDeath = _state;
    }

    public override void ManageChangeOrgAttack(int _amount, bool _tellRoom = true)
    {
        HandleChangeOrgAttack(_amount,true);
        OpponentChangedOrgesDamage _data = new OpponentChangedOrgesDamage { Amount = _amount };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentChangedOrgesDamage, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleChangeOrgAttack(int _amount, bool _isMy)
    {
        FindObjectsOfType<OrgCard>().ToList().Find(_org => _org.IsMy==_isMy).Card.Stats.Damage += _amount;
    }

    public override void PlaceAbilityOnTable(int _abilityId)
    {
        TableHandler.GetAbilityPosition((_placeId)=>
        {
            PlaceAbilityOnTable(_abilityId,_placeId);
        });
    }
    
    public override void PlaceAbilityOnTable(int _abilityId,int _placeId, bool _tellRoom = true)
    {
        PlaceAbilityOnTable(_abilityId,true,_placeId);
        OpponentReturnedAbilityToPlace _data = new OpponentReturnedAbilityToPlace { AbilityId = _abilityId, PlaceId = _placeId };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentReturnedAbilityToPlace, JsonConvert.SerializeObject(_data));
        }
    }

    private void PlaceAbilityOnTable(int _cardId, bool _isMy, int _placeId)
    {
        AbilityCard _ability = Resources.FindObjectsOfTypeAll<AbilityCard>().ToList().Find(_ability => _ability.Details.Id == _cardId);
        TablePlaceHandler _tablePlace = TableHandler.GetPlace(_placeId);
        _ability.PositionOnTable(_tablePlace);
    }

    public override void ReturnAbilityFromActivationField(int _abilityId, bool _tellRoom = true)
    {
        OpponentReturnAbilityToHand _data = new OpponentReturnAbilityToHand { AbilityId = _abilityId };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentReturnAbilityToHand, JsonConvert.SerializeObject(_data));
        }
        
        HandleReturnAbilityFromActivationField(_abilityId);
    }

    private void HandleReturnAbilityFromActivationField(int _abilityId)
    {
        StartCoroutine(Handle());
        IEnumerator Handle()
        {
            yield return new WaitForSeconds(1);
            PlaceAbilityOnTable(_abilityId);
        }
    }

    public override void BuyMatter(bool _tellRoom = true)
    {
        HandleBuyMatter(true);
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentBoughtStrangeMatter,string.Empty);
        }
    }

    private void HandleBuyMatter(bool _didIBuy)
    {
        EconomyPanelHandler.Instance.ShowBoughtMatter(_didIBuy);
    }
    
    public override void CheckForBombInMarkers(List<int> _markers,Action<List<int>> _callBack, bool _tellRoom = true)
    {
        opponentCheckForMarkerCallback = _callBack;
        OpponentAskedIfThereIsBombInMarkers _data = new OpponentAskedIfThereIsBombInMarkers { Data = JsonConvert.SerializeObject(_markers) };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentAskedIfThereIsBombInMarkers, JsonConvert.SerializeObject(_data));
        }
    }

    private List<int> CheckForBomb(List<int> _markers)
    {
        List<int> _placesWithBomb = new List<int>();
        foreach (var _markerPlaceId in _markers)
        {
            foreach (var _ in FindObjectsOfType<BomberMinefield>())
            {
                int _placeWithBomb= Check(BomberMinefield.BombMarkers,_markerPlaceId);
                if (_placeWithBomb!=-1)
                {
                    _placesWithBomb.Add(_placeWithBomb);
                }
            }
        }

        return _placesWithBomb;

        int Check(List<CardBase> _possibleMarkers, int _markerPlaceId)
        {
            foreach (var _bombMarkerCard in _possibleMarkers)
            {
                TablePlaceHandler _tablePlace = _bombMarkerCard.GetTablePlace();
                if (_tablePlace==null)
                {
                    continue;
                }

                if (_tablePlace.Id==_markerPlaceId)
                {
                    return _markerPlaceId;
                }
            }

            return -1;
        }
    }

    public override void TellOpponentToRemoveStrangeMatter(int _amount, bool _tellRoom = true)
    {
        OpponentSaidToRemoveStrangeMatter _data = new OpponentSaidToRemoveStrangeMatter { Amount = _amount };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentSaidToRemoveStrangeMatter, JsonConvert.SerializeObject(_data));
        }
    }

    public override void VetoCard(AbilityCard _card, bool _tellRoom = true)
    {
        OpponentToldYouToVetoCardOnField _data = new OpponentToldYouToVetoCardOnField { CardId = _card.Details.Id };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentToldYouToVetoCardOnField, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleVetoCard(AbilityCard _card)
    {
        StartCoroutine(HandleVetoRoutine());
        IEnumerator HandleVetoRoutine()
        {
            yield return new WaitForSeconds(2);
            _card.RotateToBeVertical();
            _card.IsVetoed = true;
            _card.Effect.CancelEffect();
            _card.Effect.AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        }
    }

    public override void TellOpponentToPlaceFirstCardCasters(bool _tellRoom = true)
    {
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentWantsMeToActivateFirstAbilityCasters,string.Empty);
        }
    }

    public override void OpponentPlacedFirstAbilityForCasters(bool _tellRoom = true)
    {
        if (_tellRoom)
        {
             roomHandler.AddAction(ActionType.TellOpponentThatIPlacedFirstCardForCasters,string.Empty);
        }
    }
    
    public override void FinishCasters(bool _tellRoom = true)
    {
        if (_tellRoom)
        {
          roomHandler.AddAction(ActionType.OpponentSaidFinishCasters,string.Empty);  
        }
    }

    public override void UpdateHealth(int _cardId, bool _status, int _health, bool _tellRoom = true)
    {
        OpponentUpdatedHealth _data = new OpponentUpdatedHealth { CardId = _cardId, Status = _status, Health = _health };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentUpdatedHealth, JsonConvert.SerializeObject(_data));
        }
    }

    public override void DestroyBombWithoutActivatingIt(int _cardId, bool _isMy, bool _tellRoom = true)
    {
        HandleDestroyBombWithoutActivatingIt(_cardId,_isMy);
        OpponentWantsToDestroyBombWithoutActivatingIt _data = new OpponentWantsToDestroyBombWithoutActivatingIt { CardId = _cardId, IsMy = _isMy };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentWantsToDestroyBombWithoutActivatingIt, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleDestroyBombWithoutActivatingIt(int _cardId,bool _isMy)
    {
       Card _bomber = FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id == _cardId && _card.My == _isMy);
       GameplayPlayer _player = _bomber.My ? MyPlayer : OpponentPlayer;
       _player.DestroyWithoutNotify(_bomber);
    }
    
    public override void TellOpponentToUseStealth(int _cardId, int _stealthFromPlace, int _placeMinionsFrom, bool _tellRoom = true)
    {
        OpponentSaidToUseStealth _data =
            new OpponentSaidToUseStealth { CardId = _cardId, StealthFromPlace = _stealthFromPlace, PlaceMinionsFrom = _placeMinionsFrom };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentSaidToUseStealth, JsonConvert.SerializeObject(_data));
        }
    }

    public override void ChangeSprite(int _cardPlace, int _cardId, int _spriteId,bool _showPlaceAnimation=false, bool _tellRoom = true)
    {
        HandleChangeSprite(_cardPlace,_cardId,_spriteId,_showPlaceAnimation);
        OpponentSaidToChangeSprite _data = new OpponentSaidToChangeSprite
        {
            CardPlace = _cardPlace, CardId = _cardId, SpriteId = _spriteId, ShowPlaceAnimation = _showPlaceAnimation
        };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentSaidToChangeSprite, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandleChangeSprite(int _cardPlace, int _cardId, int _spriteId,bool _showPlaceAnimation)
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
                    _card.IsVoid = true;
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

    public override void RequestResponseAction(int _cardId, bool _tellRoom = true)
    {
        ForceResponseAction(_cardId);
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentGotResponseAction,string.Empty);
        }
    }

    public override void PlayAudioOnBoth(string _key, CardBase _cardBase, bool _tellRoom = true)
    {
        if (string.IsNullOrEmpty(_key))
        {
            return;
        }

        Card _card = _cardBase as Card;
        
        HandlePlayAudio(_key,_card.Details.Id,true);
        OpponentSaidToPlayAudio _data = new OpponentSaidToPlayAudio { Key = _key, CardId = _card.Details.Id };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentSaidToPlayAudio, JsonConvert.SerializeObject(_data));
        }
    }

    private void HandlePlayAudio(string _key, int _cardId, bool _isMy)
    {
        if (AudioManager.Instance.IsPlayingSoundEffect)
        {
            return;
        }
        Card _card = FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id == _cardId && _card.My == _isMy);
        if (_card!=null)
        {
            _card.Display.ShowWhiteBox();
        }
        AudioManager.Instance.PlaySoundEffect(_key);
    }

    public override void TellOpponentThatIUsedUltimate(bool _tellRoom = true)
    {
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentUsedHisUltimate,string.Empty);
        }
    }

    private void OpponentUsedHisUltimate()
    {
        Keeper _keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        _keeper.SpecialAbilities[0].CanUseAbility = false;
    }

    private void OpponentPlacedCard(int _cardId, int _positionId, bool _dontCheckIfPlayerHasIt)
    {
        _positionId = ConvertOpponentsPosition(_positionId);
        PlaceCard(OpponentPlayer, _cardId, _positionId,_dontCheckIfPlayerHasIt);
    }

    private void MasterAddedAbilityToPlayer(bool _isMyPlayer, int _abilityId)
    {
       HandleAddAbilityToPlayer(!_isMyPlayer, _abilityId);
    }

    private void MasterAddedAbilityToShop(int _abilityId)
    {
        AbilityCard _ability = AbilityCardsManagerBase.Instance.DrawAbilityCard(_abilityId);
        if (_ability==null)
        {
            return;
        }
        AbilityCardsManagerBase.Instance.RemoveAbility(_ability);
        AbilityCardsManagerBase.Instance.AddAbilityToShop(_ability);
    }

    private void OpponentExecutedAction(string _actionJson)
    {
        CardAction _action = JsonConvert.DeserializeObject<CardAction>(_actionJson);
        if (_action==null)
        {
            return;
        }
        _action.StartingPlaceId = ConvertOpponentsPosition(_action.StartingPlaceId);
        _action.FinishingPlaceId = ConvertOpponentsPosition(_action.FinishingPlaceId);
        _action.IsMy = !_action.IsMy;
        PlayCardAction(_action);
    }

    private void OpponentFinishedHisMove()
    {
        OpponentFinished = true;
        IsMyTurn = true;
    }

    private void OpponentUpdatedWhiteStrangeMatter(int _amount)
    {
        OpponentPlayer.StrangeMatter = _amount;
        strangeMatterTracker.ShowOpponentStrangeMatter();
    }

    private void UpdateWhiteStrangeMatterInReserve(int _amount)
    {
        WhiteStrangeMatter.SetAmountInEconomyWithoutNotify(_amount);
        strangeMatterTracker.ShowAmountInEconomy();
    }

    private int ConvertOpponentsPosition(int _position)
    {
        int _totalAmountOfFields = 64;
        return _totalAmountOfFields - _position;
    }

    private void OpponentForcedActionsUpdate(int _amount)
    {
        OpponentPlayer.Actions = _amount;
    }

    private void OpponentFinishedAttackResponse()
    {
        StartCoroutine(OpponentFinished());

        IEnumerator OpponentFinished()
        {
            yield return new WaitForSeconds(1); // wait for opponents move to pass
            GameState = GameplayState.Playing;
            OpponentPlayer.SetActionsWithoutNotify(0);

            if (MyPlayer.Actions == 0)
            {
                EndTurn();
            }
            else
            {
                UIManager.Instance.ShowOkDialog(
                    $"Opponent finished response action, you still have {MyPlayer.Actions} actions left");
                GameplayUI.Instance.ForceActionUpdate(MyPlayer.Actions,true,false);
            }
        }
    }

    private void OpponentBoughtMinion(int _cardId, int _price, int _positionId, bool _placeMinion)
    {
        CardBase _revivedCard = FindObjectsOfType<Card>().ToList().Find(
            _destroyedCard => _destroyedCard.Details.Id == _cardId && _destroyedCard.My == false);

        _positionId = ConvertOpponentsPosition(_positionId);

        HandleBoughtMinion(_revivedCard, _price, _positionId, _cardId,_placeMinion);
    }

    private void OpponentBuiltWall(int _cardId, int _price, int _positionId)
    {
        CardBase _builtWall = FindObjectsOfType<Card>().ToList().Find(
            _wall => _wall.Details.Id == _cardId && _wall.My == false);

        _positionId = ConvertOpponentsPosition(_positionId);

        HandleBuildWall(_builtWall, _price, _positionId, _cardId);
    }

    private void OpponentLootedMe()
    {
        MyPlayer.StrangeMatter = 0;
    }

    private void OpponentUnchainedGuardian()
    {
        HandleUnchainGuardian(false);
    }

    private void OpponentsBlockaderPassive(bool _status)
    {
        ChangeBlockaderAbility(false,_status);
    }

    private void OpponentRequestedChangeOfCardOwner(int _placeId)
    {
        _placeId = ConvertOpponentsPosition(_placeId);
        HandleChangeOwnerOfCard(_placeId);
    }

    private void OpponentSaidThatTheMyCardInHisPositionDied(int _cardId)
    {
        HandleOpponentCardDiedInMyPosition(_cardId, false);
    }

    private void OpponentChangedMovementForCard(int _placeId, bool _status)
    {
        _placeId = ConvertOpponentsPosition(_placeId);
        HandleChangeMovementForCard(_placeId,_status);
    }
    
    private void OpponentGotResponseAction()
    {
        TableHandler.ActionsHandler.ClearPossibleActions();
        OpponentPlayer.Actions = 1;
        GameState = GameplayState.WaitingForAttackResponse;
        GameplayUI.Instance.ForceActionUpdate(OpponentPlayer.Actions, false,true);
        UIManager.Instance.ShowOkDialog("Opponents warrior survived, he gets 1 response action");
    }

    private void OpponentWantsToTryAndDestroyMarkers(string _placesString)
    {
        List<int> _places = JsonConvert.DeserializeObject<List<int>>(_placesString);
        if (_places==null)
        {
            return;
        }
        for (int _i = 0; _i < _places.Count; _i++)
        {
            _places[_i] = ConvertOpponentsPosition(_places[_i]);
        }

        HandleTryToDestroyMarkers(_places);
    }

    private void OpponentSaidThatBombExploded(int _placeId, bool _includeCenter)
    {
        _placeId = ConvertOpponentsPosition(_placeId);
        HandleBombExploded(_placeId,_includeCenter);
    }

    private void OpponentUsedSnowUltimate(bool _status)
    {
        HandleSnowUltimatePvp(_status,false);
    }

    private void OpponentActivatedAbility(int _abilityId, int _placeId)
    {
        _placeId = ConvertOpponentsPosition(_placeId);
        HandleActivateAbility(_abilityId, false, _placeId);
    }

    private void OpponentBoughtAbilityFromShop(int _abilityId)
    {
        HandleBuyAbilityFromShop(_abilityId,false);
        UIManager.Instance.ShowOkDialog("Opponent Bought from shared hand");
        CloseAllPanels();
    }

    private void OpponentBoughtAbilityFromHand(int _abilityId)
    {
        HandleBuyAbilityFromHand(_abilityId,false);
        UIManager.Instance.ShowOkDialog("Opponent bought ability from his hand");
    }

    private void OpponentChangedBomberExplode(bool _state)
    {
        HandleBombExplosion(false,_state);
    }

    private void OpponentChangedOrgesDamage(int _amount)
    {
        HandleChangeOrgAttack(_amount,false);
    }

    private void OpponentSaidSomething(string _text)
    {
        UIManager.Instance.ShowOkDialog(_text);
    }

    private void OpponentReturnedAbilityToPlace(int _abilityId, int _placeId)
    {
        PlaceAbilityOnTable(_abilityId, false, ConvertOpponentsPosition(_placeId));
    }

    private void OpponentReturnAbilityToHand(int _abilityId)
    {
        AbilityCard _ability = FindObjectsOfType<AbilityCard>().ToList()
            .Find(_ability => _ability.Details.Id == _abilityId);
        _ability.transform.SetParent(null);
        _ability.PositionInHand();
        UIManager.Instance.ShowOkDialog("Opponent took card from activation field");
    }

    private void OpponentBoughtStrangeMatter()
    {
        HandleBuyMatter(false);
    }

    private void OpponentFinishedReductionAction()
    {
        Reduction _reduction = FindObjectOfType<Reduction>();
        _reduction.OpponentFinishedAction();
    }

    private void OpponentGiveYouLoot(int _amount)
    {
        MyPlayer.StrangeMatter += _amount;
    }

    private void OpponentAskedIfThereIsBombInMarkers(string _markerIds, bool _tellRoom = true)
    {
        List<int> _markerPlaces = JsonConvert.DeserializeObject<List<int>>(_markerIds);
        for (int _i = 0; _i < _markerPlaces.Count; _i++)
        {
            _markerPlaces[_i] = ConvertOpponentsPosition(_markerPlaces[_i]);
        }
        List<int> _hasBomb = CheckForBomb(_markerPlaces);
        OpponentRespondedForBombQuestion _data = new OpponentRespondedForBombQuestion { BombPlaces = _hasBomb };
        
        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentRespondedForBombQuestion, JsonConvert.SerializeObject(_data));
        }
    }

    private void OpponentRespondedForBombQuestion(List<int> _markerId)
    {
        List<int> _places = new List<int>();
        foreach (var _marker in _markerId)
        {
            _places.Add(ConvertOpponentsPosition(_marker));
        }
        opponentCheckForMarkerCallback?.Invoke(_places);
    }

    private void OpponentMarkedBomb(int _placeId)
    {
        _placeId = ConvertOpponentsPosition(_placeId);
        HandleMarkMarkerAsBomb(_placeId);
    }

    private void OpponentSaidToRemoveStrangeMatter(int _amount)
    {
        MyPlayer.RemoveStrangeMatter(_amount);
    }

    private void OpponentToldYouToVetoCardOnField(int _cardId, bool _tellRoom = true)
    {
        AbilityCard _abilityCard = FindObjectsOfType<AbilityCard>().ToList().Find(_card => _card.Details.Id == _cardId);
        float _delay = 1;
        if (_abilityCard.GetTablePlace()==null)
        {
            PlaceAbilityOnTable(_cardId);
        }
        else
        {
            TablePlaceHandler _tablePlace = _abilityCard.GetTablePlace();
            if (_tablePlace.IsActivationField)
            {
                ReturnAbilityFromActivationField(_cardId);
                _delay++;
            }
            else
            {
                PlaceAbilityOnTable(_cardId,_tablePlace.Id);
            }
        }
        
        HandleVetoCard(_abilityCard);
        OpponentPlacedVetoedCard _data = new OpponentPlacedVetoedCard { CardId = _cardId };

        if (_tellRoom)
        {
            roomHandler.AddAction(ActionType.OpponentPlacedVetoedCard, JsonConvert.SerializeObject(_data));
        }
        StartCoroutine(Rotate());
        
        IEnumerator Rotate()
        {
            yield return new WaitForSeconds(_delay);
            FindObjectOfType<Veto>().AbilityCard.RotateToBeVertical();
        }
    }

    private void OpponentPlacedVetoedCard(int _cardId)
    {
        HandleVetoCard(FindObjectsOfType<AbilityCard>().ToList().Find(_card => _card.Details.Id==_cardId));
    }

    private void OpponentWantsMeToActivateFirstAbilityCasters()
    {
        Casters _casters = FindObjectOfType<Casters>();
        _casters.ActivateForOpponentFirst();
    }

    private void TellOpponentThatIPlacedFirstCardForCasters()
    {
        Casters _casters = FindObjectOfType<Casters>();
        _casters.ActivateForMe();
    }

    private void OpponentSaidFinishCasters()
    {
        Casters _casters = FindObjectOfType<Casters>();
        _casters.FinishCasters();
    }

    private void OpponentUpdatedHealth(int _cardId, bool _isMy, int _health)
    {
        FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id == _cardId && _card.My == !_isMy).Stats
            .Health = _health;
    }

    private void OpponentWantsToDestroyBombWithoutActivatingIt(int _cardId,bool _isMy)
    {
        HandleDestroyBombWithoutActivatingIt(_cardId, !_isMy);
    }

    private void OpponentSaidToUseStealth(int _cardId, int _placeId, int _placeMinionsFrom)
    {
        _placeId = ConvertOpponentsPosition(_placeId);
        _placeMinionsFrom = ConvertOpponentsPosition(_placeMinionsFrom);
        Card _card = FindObjectsOfType<Card>().ToList().Find(_card => _card.Details.Id == _cardId && _card.My &&
                                                                       _card.GetTablePlace() != null);
        _card.Hide();
        StartCoroutine(UnHide());
        foreach (var _ability in _card.SpecialAbilities)
        {
            if (_ability is SniperStealth _stealth)
            {
                _stealth.UseStealth(_placeId,_placeMinionsFrom);
                return;
            }
        }

        IEnumerator UnHide()
        {
            yield return new WaitForSeconds(2);
            _card.UnHide();
        }
    }

    private void OpponentSaidToChangeSprite(int _cardPlace, int _cardId, int _spriteId, bool _showPlaceAnimation)
    {
        HandleChangeSprite(ConvertOpponentsPosition(_cardPlace),_cardId,_spriteId,_showPlaceAnimation);   
    }

    private void OpponentSaidToPlayAudio(string _key, int _cardId)
    {
        HandlePlayAudio(_key,_cardId,false);
    }
    
    private void UseDelivery(int _defendingCardId, int _startingPlace)
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
            List<TablePlaceHandler> _emptyPlaces = new List<TablePlaceHandler>();
            foreach (var _placeId in _places)
            {
                TablePlaceHandler _place = TableHandler.GetPlace(_placeId);
                if (_place.IsOccupied)
                {
                    continue;
                }
                _emptyPlaces.Add(_place);
            }

            return _emptyPlaces;
        }
        
        bool PlaceAnywhere(int _index, int _defendingCardId, int _startingPlace)
            {
                TablePlaceHandler _place = TableHandler.GetPlace(_index);
                if (_place.IsOccupied)
                {
                    return false;
                }

                DoPlace(_index,_defendingCardId,_startingPlace);
                return true;
            }
        
        void DoPlaceOnTable(int _placeId)
        { 
            DoPlace(_placeId,_defendingCardId,_startingPlace);
        }

        void DoPlace(int _index, int _defendingCardId, int _startingPlace)
        {
            CardAction _actionMove = new CardAction
            {
                FirstCardId = _defendingCardId,
                StartingPlaceId = _startingPlace,
                FinishingPlaceId = _index,
                Type = CardActionType.Move,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false
            };
            ExecuteCardAction(_actionMove);
        }
    }

    public override void SetTaxCard(int _id)
    {
        TaxSelectedCard _taxDetails = new TaxSelectedCard { CardId =  _id};
        FirebaseManager.Instance.RoomHandler.AddAction(ActionType.SetTaxCard, JsonConvert.SerializeObject(_taxDetails));
    }

    public override void ActivatedTaxedCard()
    {
        FirebaseManager.Instance.RoomHandler.AddAction(ActionType.ActivatedTaxCard, string.Empty);
    }

    public override void TellOpponentToUpdateMyStrangeMatter()
    {
        ForceSetOpponentStrangeMatter _data = new ForceSetOpponentStrangeMatter { Amount = MyPlayer.StrangeMatter };
        roomHandler.AddAction(ActionType.ForceSetOpponentStrangeMatter,JsonConvert.SerializeObject(_data));
    }
}