using System;
using System.Collections;
using UnityEngine;

public class Portal : AbilityEffect
{
    private int portalId = 2000;

    protected override void ActivateForOwner()
    {
        GameplayState _gameState = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;

        DialogsManager.Instance.ShowOkDialog("Select place for the portal");
        GameplayManager.Instance.SelectPlaceForSpecialAbility(10, 10, PlaceLookFor.Empty, CardMovementType.EightDirections, false,
            LookForCardOwner.Both, OnPlaceSelected);


        void OnPlaceSelected(int _selectedPlace)
        {
            Card _portal = CardsManager.Instance.CreateCard(portalId, true);
            GameplayManager.Instance.PlaceCard(_portal, _selectedPlace, true);
            AddEffectedCard(_portal.UniqueId);;
            if (GetEffectedCards().Count == 2)
            {
                GameplayManager.Instance.GameState = _gameState;
                GameplayManager.OnCardMoved += CheckCard;
                RemoveAction();
                SetIsActive(true);
                OnActivated?.Invoke();
            }
            else
            {
                DialogsManager.Instance.ShowOkDialog("Select second place for the portal");
                GameplayManager.Instance.SelectPlaceForSpecialAbility(10, 10, PlaceLookFor.Empty, CardMovementType.EightDirections, false,
                    LookForCardOwner.Both, OnPlaceSelected);
                portalId++;
            }
        }
    }

    private void CheckCard(CardBase _arg1, int _arg2, int _arg3, bool _s)
    {
        CheckCard(_arg1, _arg2, _arg3, false, null);
    }

    public void CheckCard(CardBase _cardThatMoved, int _startingPosition, int _finishingPosition, bool _isAPush, Action<bool> _didPushCallBack = null)
    {
        StartCoroutine(CheckRoutine());

        IEnumerator CheckRoutine()
        {
            yield return new WaitForSeconds(1);
            Card _enteredPortal = null;
            Card _exitPortal = null;
            if (GetEffectedCards().Count == 0)
            {
                foreach (var _portalCard in FindObjectsOfType<PortalCard>())
                {
                    AddEffectedCard(_portalCard.UniqueId);
                }
            }

            for (int _i = 0; _i < GetEffectedCards().Count; _i++)
            {
                Card _currentPortal = GetEffectedCards()[_i];
                if (_currentPortal.GetTablePlace().Id == _finishingPosition)
                {
                    _enteredPortal = _currentPortal;
                    _exitPortal = _i == 0 ? GetEffectedCards()[1] : GetEffectedCards()[0];
                }
            }

            if (_enteredPortal == null)
            {
                yield break;
            }

            if (_isAPush)
            {
                if (_cardThatMoved is Keeper)
                {
                    if (GameplayManager.Instance.IsAbilityActive<Penalty>())
                    {
                        CardAction _damageAction = new CardAction
                        {
                            StartingPlaceId = _startingPosition,
                            FirstCardId = ((Card)_cardThatMoved).Details.Id,
                            FinishingPlaceId = _startingPosition,
                            SecondCardId = ((Card)_cardThatMoved).Details.Id,
                            Type = CardActionType.Attack,
                            Cost = 1,
                            IsMy = true,
                            CanTransferLoot = false,
                            Damage = 1,
                            CanCounter = false,
                            GiveLoot = false
                        };

                        GameplayManager.Instance.ExecuteCardAction(_damageAction);
                    }
                }
            }
            
            GameplayManager.Instance.MyPlayer.Actions--;

            int _exitIndex = GameplayManager.Instance.TableHandler.GetTeleportExitIndex(_startingPosition, _enteredPortal.GetTablePlace().Id,
                _exitPortal.GetTablePlace().Id);
            if (_exitIndex != -1 && _cardThatMoved.name.ToLower().Contains("blockader") &&
                GameplayManager.Instance.TableHandler.GetPlace(_exitIndex).IsOccupied)
            {
                HandleBlockader(_exitIndex, _exitPortal.GetTablePlace().Id, (_cardThatMoved as Card), _cardThatMoved.GetTablePlace().Id, _exitIndex,
                    (_cardThatMoved as Card).Details.Id, GameplayManager.Instance.TableHandler.GetPlace(_exitIndex).GetCard().Details.Id);
                yield break;
            }


            if (_exitIndex == -1 || GameplayManager.Instance.TableHandler.GetPlace(_exitIndex).IsOccupied)
            {
                CardAction _damageSelf = new CardAction
                {
                    StartingPlaceId = _cardThatMoved.GetTablePlace().Id,
                    FirstCardId = ((Card)(_cardThatMoved)).Details.Id,
                    FinishingPlaceId = _cardThatMoved.GetTablePlace().Id,
                    SecondCardId = ((Card)(_cardThatMoved)).Details.Id,
                    Type = CardActionType.Attack,
                    Cost = 0,
                    IsMy = true,
                    CanTransferLoot = false,
                    Damage = 1,
                    CanCounter = false,
                    GiveLoot = false
                };
                GameplayManager.Instance.ExecuteCardAction(_damageSelf);
            }
            else
            {
                CardAction _moveAction = new CardAction
                {
                    StartingPlaceId = _cardThatMoved.GetTablePlace().Id,
                    FirstCardId = ((Card)(_cardThatMoved)).Details.Id,
                    FinishingPlaceId = _exitIndex,
                    SecondCardId = ((Card)(_cardThatMoved)).Details.Id,
                    Type = CardActionType.Move,
                    Cost = 0,
                    IsMy = true,
                    CanTransferLoot = false,
                    Damage = 0,
                    CanCounter = false,
                    GiveLoot = false,
                    DidTeleport = true
                };
                GameplayManager.Instance.ExecuteCardAction(_moveAction);
                _didPushCallBack?.Invoke(true);
            }
        }

        void HandleBlockader(int _exitIndex, int _exitPortalIndex, Card _card, int _placeIdOfFirstCard, int _placeIdOfSecondCard, int _firstCardId,
            int _secondCardId)
        {
            Card _cardInFrontOfSecondCard = GameplayManager.Instance.TableHandler.CheckForCardInFront(_exitIndex, _exitPortalIndex);

            if (_cardInFrontOfSecondCard == null)
            {
                //push second card
                int _pushedCardPlaceId = GameplayManager.Instance.PushCardForward(_exitPortalIndex, _placeIdOfSecondCard, 100);
                if (_pushedCardPlaceId != -1)
                {
                    StartCoroutine(MoveSelfRoutine());
                }
                else
                {
                    CardAction _moveBack = new CardAction
                    {
                        StartingPlaceId = _placeIdOfFirstCard,
                        FirstCardId = _firstCardId,
                        FinishingPlaceId = _startingPosition,
                        Type = CardActionType.Move,
                        Cost = 1,
                        IsMy = true,
                        CanTransferLoot = false,
                        Damage = 0,
                        CanCounter = false,
                        GiveLoot = false
                    };

                    GameplayManager.Instance.ExecuteCardAction(_moveBack);
                }

                GameplayManager.Instance.MyPlayer.Actions--;
            }
            else
            {
                CardAction _damageAction = new CardAction
                {
                    StartingPlaceId = _placeIdOfFirstCard,
                    FirstCardId = _firstCardId,
                    FinishingPlaceId = _placeIdOfSecondCard,
                    SecondCardId = _secondCardId,
                    Type = CardActionType.Attack,
                    Cost = 1,
                    IsMy = true,
                    CanTransferLoot = false,
                    Damage = 1,
                    CanCounter = false,
                    GiveLoot = false
                };

                GameplayManager.Instance.ExecuteCardAction(_damageAction);
            }


            IEnumerator MoveSelfRoutine()
            {
                yield return new WaitForSeconds(0.5f);
                CardAction _moveSelf = new CardAction()
                {
                    FirstCardId = _card.Details.Id,
                    StartingPlaceId = _placeIdOfFirstCard,
                    FinishingPlaceId = _placeIdOfSecondCard,
                    Type = CardActionType.Move,
                    Cost = 0,
                    IsMy = true,
                    CanTransferLoot = false,
                    Damage = -1,
                    CanCounter = false,
                };

                GameplayManager.Instance.ExecuteCardAction(_moveSelf);
            }
        }
    }

    protected override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        SetIsActive(false);
        portalId = 2000;
        
        foreach (var _portal in GetEffectedCards())
        {
            RemoveEffectedCard(_portal.UniqueId);
            Destroy(_portal.gameObject);
        }

        GameplayManager.OnCardMoved -= CheckCard;
    }
}