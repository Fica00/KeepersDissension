using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableActionsHandler : MonoBehaviour
{
    private TableHandler tableHandler;
    private List<CardAction> possibleActions = new();

    private void OnEnable()
    {
        TablePlaceHandler.OnPlaceClicked += CheckForAction;
        TablePlaceHandler.OnPlaceClicked += ClearPossibleActions;
    }

    private void OnDisable()
    {
        TablePlaceHandler.OnPlaceClicked -= CheckForAction;
        TablePlaceHandler.OnPlaceClicked -= ClearPossibleActions;
    }

    private void ClearPossibleActions(TablePlaceHandler _clickedPlace)
    {
        Debug.Log("Clearing possible actions");
        ClearPossibleActions();
    }

    public void Setup()
    {
        tableHandler = GameplayManager.Instance.TableHandler;
    }

    public bool ContinueWithShowingPossibleActions(int _placeId)
    {
        bool _isCardInSelectedActions = false;
        Debug.Log(possibleActions.Count);
        foreach (var _action in possibleActions)
        {
            Debug.Log($"{_action.FinishingPlaceId} == {_placeId}");
            if (_action.FinishingPlaceId == _placeId)
            {
                _isCardInSelectedActions = true;
            }
        }
        
        Debug.Log("Is card in selected actions: "+_isCardInSelectedActions);

        if (_isCardInSelectedActions)
        {
            CheckForAction(tableHandler.GetPlace(_placeId));
            ClearPossibleActions();
            Debug.Log("aaaaaaa");
            return false;
        }

            Debug.Log("bbbb");
        return true;
    }

    public void ShowPossibleActions(TablePlaceHandler _clickedPlace, Card _card, CardActionType _type)
    {
        ClearPossibleActions();

        if (!_card.IsWarrior())
        {
            CardActionsDisplay.Instance.Close();
            return;
        }

        int _placeId = _clickedPlace.Id;

        CardMovementType _movementType = _card.MovementType;
        switch (_type)
        {
            case CardActionType.Attack:
                int _attackingRange = _card.Range != 1 ? _card.Range : 1;
                List<TablePlaceHandler> _attackablePlaces = tableHandler.GetPlacesAround(_placeId, _card.MovementType, _attackingRange, true, true);
                AddAttackAction(_attackablePlaces, _card);
                break;
            case CardActionType.Move:
                int _movingSpace = _card.Speed != 0 ? _card.Speed : 1;
                var _movablePlaces = tableHandler.GetPlacesAround(_placeId, _movementType, _movingSpace);
                AddSwitchActions(_movablePlaces, _card, _movingSpace);
                // AddRamAbility(_movablePlaces, _card);
                AddMovementActions(_card, _movingSpace);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_type), _type, null);
        }

        HandleChainedGuardian(_card);
        foreach (var _action in possibleActions)
        {
            _action.StartingPlaceId = _placeId;
            if (_card.Speed != 0)
            {
                _action.Cost = _action.Type == CardActionType.SwitchPlace ? 2 : 1;
            }
        }
    }

    private void AddRamAbility(List<TablePlaceHandler> _movablePlaces, CardBase _cardBase)
    {
        Card _card = _cardBase as Card;
        int _actionCost = 1;
        TablePlaceHandler _startingPlace = _cardBase.GetTablePlace();
        if (_card == null)
        {
            return;
        }

        bool _hasRam = false;

        foreach (var _specialAbility in _card.SpecialAbilities)
        {
            if (_specialAbility is BlockaderRam)
            {
                _hasRam = true;
                break;
            }
        }

        if (!_hasRam)
        {
            return;
        }

        foreach (var _placeAround in _movablePlaces)
        {
            bool _skip = true;
            if (_placeAround.IsOccupied)
            {
                _skip = false;
                if (_placeAround.ContainsMarker || _placeAround.ContainsPortal || _placeAround.ContainsWall)
                {
                    _skip = true;
                }
            }

            if (_skip)
            {
                continue;
            }

            Card _cardAtPlace = _placeAround.GetCard();

            if (_cardAtPlace == null)
            {
                continue;
            }

            int _distance = tableHandler.DistanceBetweenPlaces(_startingPlace, _placeAround);
            if (_distance > 1)
            {
                continue;
            }

            if (GameplayManager.Instance.MyPlayer.Actions < _actionCost)
            {
                if (!GameplayManager.Instance.IsMyResponseAction2())
                {
                    continue;
                }
            }

            possibleActions.Add(new CardAction()
            {
                FirstCardId = _card.UniqueId,
                StartingPlaceId = _startingPlace.Id,
                FinishingPlaceId = _placeAround.Id,
                SecondCardId = _cardAtPlace.UniqueId,
                Type = CardActionType.RamAbility,
                Cost = _actionCost,
            });

            _placeAround.SetColor(Color.red);
        }
    }

    private void HandleChainedGuardian(Card _card)
    {
        if (_card is Guardian { IsChained: true })
        {
            foreach (var _possibleAction in possibleActions.ToList())
            {
                if (tableHandler.GetPlace(_possibleAction.FinishingPlaceId).ContainsPortal)
                {
                    continue;
                }

                if (_possibleAction.Type == CardActionType.Move || _possibleAction.Type == CardActionType.SwitchPlace)
                {
                    bool _foundLifeForce = false;
                    List<TablePlaceHandler> _placesAround =
                        tableHandler.GetPlacesAround(_possibleAction.FinishingPlaceId, CardMovementType.EightDirections);
                    foreach (var _placeAround in _placesAround)
                    {
                        CardBase _cardAround = _placeAround.GetCard();
                        if (_cardAround == null)
                        {
                            continue;
                        }

                        if (_cardAround is LifeForce)
                        {
                            _foundLifeForce = true;
                            break;
                        }

                    }

                    if (!_foundLifeForce)
                    {
                        ClearPossibleAction(_possibleAction);
                        possibleActions.Remove(_possibleAction);
                    }
                }
            }
        }
    }

    

    public void ClearPossibleActions()
    {
        foreach (var _possibleAction in possibleActions)
        {
            ClearPossibleAction(_possibleAction);
        }

        possibleActions.Clear();
    }

    private void ClearPossibleAction(CardAction _possibleAction)
    {
        TablePlaceHandler _place = tableHandler.GetPlace(_possibleAction.FinishingPlaceId);
        _place.SetColor(Color.white);
    }

    private void AddMovementActions(CardBase _cardBase, int _speed)
    {
        Card _warriorCard = (_cardBase as Card);
        TablePlaceHandler _startingPlace = _cardBase.GetTablePlace();

        if (_warriorCard == null || _speed <= 0)
        {
            return;
        }

        HashSet<TablePlaceHandler> _processedPlaces = new HashSet<TablePlaceHandler>();
        ProcessMovement(_startingPlace, _warriorCard, _speed, _processedPlaces);
    }

    private void ProcessMovement(TablePlaceHandler _currentPlace, Card _warriorCard, int _remainingSpeed, HashSet<TablePlaceHandler> _processedPlaces)
    {
        int _actionCost = 1;
        if (_remainingSpeed <= 0 || !_processedPlaces.Add(_currentPlace))
        {
            return;
        }

        List<TablePlaceHandler> _neighborPlaces = tableHandler.GetPlacesAround(_currentPlace.Id, _warriorCard.MovementType, 1);

        foreach (var _placeAround in _neighborPlaces)
        {
            bool _skip = false;
            if (_placeAround.IsOccupied)
            {
                _skip = true;

                // if (_placeAround.ContainsWall && _warriorCard.CanMoveOnWall && !_placeAround.ContainsWarrior())
                // {
                //     _skip = false;
                // }
                // else 
                if (_placeAround.ContainsMarker || _placeAround.ContainsPortal)
                {
                    _skip = false;
                }
            }

            int _movementCost = CalculatePathCost(_currentPlace, _placeAround, _warriorCard.MovementType, 1, CardActionType.Move);
            if (_remainingSpeed >= _movementCost && !_skip)
            {
                possibleActions.Add(new CardAction()
                {
                    FirstCardId = _warriorCard.UniqueId, FinishingPlaceId = _placeAround.Id, Type = CardActionType.Move, Cost = _actionCost,
                });

                _placeAround.SetColor(Color.blue);

                ProcessMovement(_placeAround, _warriorCard, _remainingSpeed - _movementCost, _processedPlaces);
            }

            if (_skip)
            {
                // AddLeapfrogMovement(_currentPlace, _placeAround, _warriorCard, _remainingSpeed, _processedPlaces);
            }
            
            else if (_placeAround.ContainsWall)
            {
                foreach (var _ability in _warriorCard.SpecialAbilities)
                {
                    if (_ability is ScalerLeapfrog)
                    {
                        // AddLeapfrogMovement(_currentPlace, _placeAround, _warriorCard, _remainingSpeed, _processedPlaces);
                    }
                }
            }
        }
    }

    private void AddLeapfrogMovement(TablePlaceHandler _currentPlace, TablePlaceHandler _placeAround, Card _warriorCard, int _remainingSpeed,
        HashSet<TablePlaceHandler> _processedPlaces)
    {
        foreach (var _special in _warriorCard.SpecialAbilities)
        {
            if (_special is ScalerLeapfrog)
            {
                Vector2 _cordsInFront = tableHandler.GetFrontIndex(_currentPlace.Id, _placeAround.Id);
                AddCardInFront(_warriorCard, _cordsInFront, 1, _dontAddIfItIsAWall: true);
            }
        }
    }

    private void AddCardInFront(Card _warriorCard, Vector2 _cordsInFront, int _actionCost, bool _dontAddIfItIsAWall = false)
    {
        TablePlaceHandler _placeInFront = tableHandler.GetPlace(_cordsInFront);
        if (_placeInFront == null)
        {
            return;
        }

        if (_placeInFront != default && !_placeInFront.ContainsWarrior() && !_placeInFront.IsAbility)
        {
            if (_dontAddIfItIsAWall && _placeInFront.ContainsWall)
            {
                return;
            }

            possibleActions.Add(new CardAction()
            {
                FirstCardId = _warriorCard.UniqueId,
                StartingPlaceId = _warriorCard.GetTablePlace().Id,
                FinishingPlaceId = _placeInFront.Id,
                Type = CardActionType.Move,
                Cost = _actionCost,
            });
            _placeInFront.SetColor(Color.magenta);
        }
    }

    private int CalculatePathCost(TablePlaceHandler _startingPlace, TablePlaceHandler _placeAround, CardMovementType _movementType,
        int _startingPathCost, CardActionType _type)
    {
        int _pathCost = _startingPathCost;
        var _path = tableHandler.FindPath(_startingPlace, _placeAround, _movementType);
        if (_path.Count >= 1)
        {
            foreach (var _step in _path)
            {
                if (_step.IsOccupied)
                {
                    _pathCost += 2;
                }
                else
                {
                    _pathCost += 1;
                }
            }
        }

        for (int _i = 0; _i < _path.Count; _i++)
        {
            if (_path[_i].IsOccupied)
            {
                _pathCost = 99;
                break;
            }
        }

        return _pathCost;
    }

    private void AddSwitchActions(List<TablePlaceHandler> _movablePlaces, Card _card, int _speed)
    {
        int _actionCost = 2;
        if (GameplayManager.Instance.MyPlayer.Actions < _actionCost)
        {
            return;
        }

        foreach (var _placeAround in _movablePlaces)
        {
            if (!_placeAround.IsOccupied)
            {
                continue;
            }

            List<CardBase> _cardsAtPlace = _placeAround.GetCards();

            foreach (var _cardAtPlace in _cardsAtPlace)
            {
                if (!_cardAtPlace.GetIsMy())
                {
                    continue;
                }

                if (!_cardAtPlace.IsWarrior())
                {
                    continue;
                }

                if (_cardAtPlace.GetTablePlace().ContainsWall && !_card.CanMoveOnWall)
                {
                    continue;
                }

                if (_card.Speed == 0)
                {
                    continue;
                }
                
                int _speedCost = CalculatePathCost(_card.GetTablePlace(), _placeAround, _card.MovementType, 2, CardActionType.SwitchPlace) - 1;
                if (_speed < _speedCost)
                {
                    continue;
                }

                possibleActions.Add(new CardAction
                {
                    FirstCardId = _card.UniqueId,
                    SecondCardId = ((Card)_cardAtPlace).UniqueId,
                    FinishingPlaceId = _placeAround.Id,
                    Type = CardActionType.SwitchPlace,
                    Cost = _actionCost,
                });

                _placeAround.SetColor(Color.yellow);
            }
        }
    }

    private void AddAttackAction(List<TablePlaceHandler> _attackablePlaces, CardBase _card)
    {
        TablePlaceHandler _attackingCardPlace = _card.GetTablePlace();
        Card _attackingCard = _card as Card;

        foreach (var _attackablePlace in _attackablePlaces)
        {
            if (_attackablePlace.ContainsPortal)
            {
                TablePlaceHandler _placeOfExitPortal = FindObjectsOfType<PortalCard>().ToList().Find(_portal =>
                    _portal.GetTablePlace().Id != _attackablePlace.Id).GetTablePlace();
                int _distanceBetween = tableHandler.DistanceBetweenPlaces(_attackingCardPlace, _attackablePlace);
                int _newRange = _attackingCard.Range - (_distanceBetween-1);
                List<TablePlaceHandler> _placesAroundPortal = tableHandler.GetPlacesAround(_placeOfExitPortal.Id, _attackingCard.MovementType, _newRange);

                foreach (var _placeAroundPortal in _placesAroundPortal)
                {
                    int _newDistance = tableHandler.DistanceBetweenPlaces(_placeOfExitPortal, _placeAroundPortal);
                    TablePlaceHandler _newPlaceWithWall = tableHandler.GetPlaceWithWallInPath(_placeOfExitPortal, _placeAroundPortal);
                    if (tableHandler.GetDirection(_attackablePlace.Id, _attackingCardPlace.Id) ==
                        tableHandler.GetDirection(_placeOfExitPortal.Id, _placeAroundPortal.Id))
                    {
                        continue;
                    }

                    CheckAttackingPlace(_attackingCard, _placeAroundPortal, _newDistance, _placeOfExitPortal, _newPlaceWithWall);
                }

                continue;
            }

            int _distance = tableHandler.DistanceBetweenPlaces(_attackingCardPlace, _attackablePlace);
            TablePlaceHandler _placeWithWall = tableHandler.GetPlaceWithWallInPath(_attackingCardPlace, _attackablePlace);
            CheckAttackingPlace(_attackingCard, _attackablePlace, _distance, _attackingCardPlace, _placeWithWall);
        }
    }

    private void CheckAttackingPlace(Card _attackingCard, TablePlaceHandler _attackablePlace, int _distance, TablePlaceHandler _attackingCardPlace,
        TablePlaceHandler _placeWithWall)
    {
        int _actionCost = 1;
        if (_attackingCard == null)
        {
            return;
        }

        if (!_attackablePlace.IsOccupied)
        {
            if (!_attackablePlace.ContainsMarker)
            {
                return;
            }
        }
        else if (_attackablePlace.ContainsVoid)
        {
            return;
        }

        List<CardBase> _attackedCards = _attackablePlace.GetCards();

        foreach (var _attackedCard in _attackedCards)
        {
            if (!_attackedCard.IsAttackable())
            {
                continue;
            }

            if (_placeWithWall != null && _placeWithWall.Id != _attackingCardPlace.Id)
            {
                if (_distance != 1)
                {
                    if (_attackingCard.Range < _distance)
                    {
                        continue;
                    }

                }

                if (_placeWithWall.GetCards().Count > 1 && _attackedCard is Minion && _attackingCard.Range <= 1)
                {
                    continue;
                }
            }
            
            if (_attackedCard.IsLifeForce())
            {
                if (_distance != 1)
                {
                    if (_attackingCard.Range < _distance)
                    {
                        continue;
                    }
                }
            }

            if (_attackingCardPlace.ContainsWall)
            {
                _distance--;
            }

            if (_attackingCard.Range != 0)
            {
                if (_attackingCard.Range <
                    CalculatePathCost(_attackingCardPlace, _attackablePlace, _attackingCard.MovementType, 1, CardActionType.Attack))
                {
                    continue;
                }
            }
            else if (_attackingCard.Range < _distance)
            {
                continue;
            }

            if (GameplayManager.Instance.MyPlayer.Actions < _actionCost)
            {
                if (!GameplayManager.Instance.IsMyResponseAction2())
                {
                    continue;
                }
            }

            
            possibleActions.Add(new CardAction()
            {
                FirstCardId = _attackingCard.UniqueId,
                SecondCardId = ((Card)_attackedCard).UniqueId,
                FinishingPlaceId = _attackablePlace.Id,
                Type = CardActionType.Attack,
                Cost = _actionCost,
            });

            _attackablePlace.SetColor(Color.green);
        }
    }

    private void CheckForAction(TablePlaceHandler _placeClicked)
    {
        List<CardAction> _triggeredActions = new List<CardAction>();
        foreach (var _possibleAction in possibleActions)
        {
            if (_possibleAction.FinishingPlaceId == _placeClicked.Id)
            {
                _triggeredActions.Add(_possibleAction);
            }
        }

        if (_triggeredActions.Count == 0)
        {
            return;
        }

        List<CardAction> _uniqueActions = new List<CardAction>();
        foreach (var _triggeredAction in _triggeredActions)
        {
            bool _skip = false;
            foreach (var _uniq in _uniqueActions)
            {
                if (_triggeredAction.CompareTo(_uniq))
                {
                    _skip = true;
                }
            }

            if (_skip)
            {
                continue;
            }

            _uniqueActions.Add(_triggeredAction);
        }

        if (_uniqueActions.Count == 1)
        {
            ExecuteAction(_uniqueActions[0]);
        }
        else
        {
            ResolveMultipleActions.Instance.Show(_uniqueActions.ToList(), ExecuteAction);
        }
    }

    private void ExecuteAction(CardAction _action)
    {
        CardBase _attackingCard = tableHandler.GetPlace(_action.StartingPlaceId).GetComponentInChildren<CardBase>();
        CardBase _defendingCard = tableHandler.GetPlace(_action.FinishingPlaceId).GetComponentInChildren<CardBase>();
        if (_attackingCard != null && _defendingCard != null)
        {
            if (_action.Type == CardActionType.Attack)
            {
                _attackingCard = tableHandler.GetPlace(_action.StartingPlaceId).GetCards().Cast<Card>().ToList()
                    .Find(_card => _card.UniqueId == _action.FirstCardId);
                _defendingCard = tableHandler.GetPlace(_action.FinishingPlaceId).GetCards().Cast<Card>().ToList()
                    .Find(_card => _card.UniqueId == _action.SecondCardId);
                string _question = string.Empty;
                if (_attackingCard.GetIsMy() && _defendingCard.GetIsMy())
                {
                    _question += $"Attacking your own {((Card)_defendingCard).Details.Type}, continue?";
                }
                else
                {
                    _question += $"Attacking {((Card)_defendingCard).Details.Type}, continue?";
                }

                if (_defendingCard is Wall _wall)
                {
                    if (_wall.Details.Faction == FactionSO.Get(0)) //snow faction
                    {
                        _question += "\n(You wont be able to move this card until next turn)";
                    }
                    else if (_wall.Details.Faction == FactionSO.Get(1))
                    {
                        _question += "\n(Your card will be pushed back 1 tile)";
                    }
                }

                DialogsManager.Instance.ShowYesNoDialog(_question, () => { YesExecute(_action); });
                return;
            }
        }

        YesExecute(_action);
    }

    private void YesExecute(CardAction _action)
    {
        if (_action.Type == CardActionType.RamAbility)
        {
            TablePlaceHandler _place = tableHandler.GetPlace(_action.StartingPlaceId);
            Card _cardAtPlace = null;

            foreach (var _cardBase in _place.GetCards())
            {
                if (_cardBase is Card _possibleCard && _possibleCard.UniqueId == _action.FirstCardId)
                {
                    _cardAtPlace = _possibleCard;
                    break;
                }
            }

            if (_cardAtPlace == null)
            {
                return;
            }

            foreach (var _specialAbility in _cardAtPlace.SpecialAbilities)
            {
                if (_specialAbility is BlockaderRam _ramAbility)
                {
                    _ramAbility.TryAndPush(_action.StartingPlaceId, _action.FinishingPlaceId, _action.FirstCardId, _action.SecondCardId);
                }
            }

            return;
        }

        // New logic to handle portals and occupied destinations
        if (_action.Type == CardActionType.Move)
        {
            TablePlaceHandler _currentPlace = tableHandler.GetPlace(_action.StartingPlaceId);
            TablePlaceHandler _destinationPlace = tableHandler.GetPlace(_action.FinishingPlaceId);

            if (_destinationPlace.ContainsPortal)
            {
                TablePlaceHandler _exitPlace = _destinationPlace.GetComponentInChildren<PortalCard>()
                    .GetExitPlace(_action.StartingPlaceId, _action.FinishingPlaceId);
                
                if (_exitPlace != null && _exitPlace.IsOccupied)
                {
                    Card _cardAtExitPlace = _exitPlace.GetCard();
                    if (_cardAtExitPlace != null)
                    {
                        Vector2 direction = tableHandler.GetDirection(_currentPlace.Id, _action.FinishingPlaceId);
                        Vector2 coordsInFront = tableHandler.GetIndexOfPlace(_exitPlace) + direction;
                        TablePlaceHandler placeInFront = tableHandler.GetPlace(coordsInFront);
                        if (placeInFront != null && !placeInFront.IsOccupied)
                        {
                            CardAction _pushAction = new CardAction
                            {
                                StartingPlaceId = _exitPlace.Id,
                                FirstCardId = _cardAtExitPlace.UniqueId,
                                FinishingPlaceId = placeInFront.Id,
                                Type = CardActionType.Move,
                                Cost = 0,
                                CanTransferLoot = false,
                                Damage = 0,
                                CanCounter = false,
                                GiveLoot = false
                            };
                            GameplayManager.Instance.ExecuteCardAction(_pushAction);
                            // Now move the moving card into the exit place
                            Card _movingCard = _currentPlace.GetCards().Cast<Card>().ToList().Find(_card => _card.UniqueId == _action.FirstCardId);

                            CardAction _moveAction = new CardAction
                            {
                                StartingPlaceId = _movingCard.GetTablePlace().Id,
                                FirstCardId = _movingCard.UniqueId,
                                FinishingPlaceId = _exitPlace.Id,
                                Type = CardActionType.Move,
                                Cost = 0,
                                CanTransferLoot = false,
                                Damage = 0,
                                CanCounter = false,
                                GiveLoot = false,
                                DidTeleport = true
                            };
                            GameplayManager.Instance.ExecuteCardAction(_moveAction);

                            return; // Action handled, so exit the function
                        }
                        else
                        {
                            int _cardsPlace = _cardAtExitPlace.GetTablePlace().Id;
                            Card _movingCard = _currentPlace.GetCards().Cast<Card>().ToList().Find(_card => _card.UniqueId == _action.FirstCardId);

                            CardAction _damageOtherCard = new CardAction
                            {
                                StartingPlaceId = _cardAtExitPlace.GetTablePlace().Id,
                                FirstCardId = _cardAtExitPlace.UniqueId,
                                FinishingPlaceId = _cardAtExitPlace.GetTablePlace().Id,
                                SecondCardId = _cardAtExitPlace.UniqueId,
                                Type = CardActionType.Attack,
                                Cost = 0,
                                CanTransferLoot = false,
                                Damage = 1,
                                CanCounter = false,
                                GiveLoot = false
                            };
                            GameplayManager.Instance.ExecuteCardAction(_damageOtherCard);

                            if (_cardAtExitPlace == null || _cardAtExitPlace.Health <= 0)
                            {
                                CardAction _moveAction = new CardAction
                                {
                                    StartingPlaceId = _currentPlace.Id,
                                    FirstCardId = _movingCard.UniqueId,
                                    FinishingPlaceId = _cardsPlace,
                                    Type = CardActionType.Move,
                                    Cost = 0,
                                    CanTransferLoot = false,
                                    Damage = 0,
                                    CanCounter = false,
                                    GiveLoot = false,
                                    DidTeleport = true
                                };
                                GameplayManager.Instance.ExecuteCardAction(_moveAction);
                            }
                            return;
                        }
                    }
                }

                if (_exitPlace==null)
                {
                    return;
                }

                if (_exitPlace.IsActivationField)
                {
                    return;
                }                
                
                if (_exitPlace.IsAbility)
                {
                    return;
                }

            }
        }

        if (_action.Type is CardActionType.Move or CardActionType.SwitchPlace or CardActionType.RamAbility)
        {
            if (GameplayManager.Instance.IsAbilityActive<Hinder>())
            {
                foreach (var _placeInRange in GameplayManager.Instance.TableHandler.GetPlacesAround(_action.StartingPlaceId,
                             CardMovementType.EightDirections))
                {
                    if (!_placeInRange.IsOccupied || _placeInRange.GetCard() is not Keeper)
                    {
                        continue;
                    }

                    if (_placeInRange.GetCard().My)
                    {
                        continue;
                    }

                    DialogsManager.Instance.ShowOkDialog("This card can't move due to Hinder effect.");
                    return;
                }
            }
        }

        CardAction _newAction = new CardAction
        {
            StartingPlaceId = _action.StartingPlaceId,
            FinishingPlaceId = _action.FinishingPlaceId,
            Type = _action.Type,
            Cost = _action.Cost,
            CanTransferLoot = _action.CanTransferLoot,
            Damage = _action.Damage,
            CanCounter = _action.CanCounter,
            FirstCardId = _action.FirstCardId,
            SecondCardId = _action.SecondCardId
        };

        if (_newAction.Type == CardActionType.SwitchPlace)
        {
            Card _cardOne = GameplayManager.Instance.TableHandler.GetPlace(_newAction.StartingPlaceId).GetComponentInChildren<Card>();

            Card _cardTwo = GameplayManager.Instance.TableHandler.GetPlace(_newAction.FinishingPlaceId).GetComponentInChildren<Card>();

            if (!_cardOne.CanMove || !_cardTwo.CanMove)
            {
                DialogsManager.Instance.ShowOkDialog("Cannot swap, one of the cards cannot move.");
                return;
            }
        }
        else if (_newAction.Type == CardActionType.Move)
        {
            Card _cardOne = GameplayManager.Instance.TableHandler.GetPlace(_newAction.StartingPlaceId).GetComponentInChildren<Card>();

            if (!_cardOne.CanMove)
            {
                DialogsManager.Instance.ShowOkDialog("Cannot move this card.");
                return;
            }
        }

        TablePlaceHandler _tablePlace = GameplayManager.Instance.TableHandler.GetPlace(_action.StartingPlaceId);
        Card _card = null;
        foreach (var _cardBase in _tablePlace.GetCards())
        {
            if (_cardBase is Card _cardOnPlace)
            {
                if (_cardOnPlace.UniqueId == _newAction.FirstCardId)
                {
                    _card = _cardOnPlace;
                    break;
                }
            }
        }

        if (_card != null)
        {
            _card.SetSpeed(0);
        }

        Card _card1 = GameplayManager.Instance.TableHandler.GetPlace(_newAction.StartingPlaceId).GetComponentInChildren<Card>();

        Card _card2 = GameplayManager.Instance.TableHandler.GetPlace(_newAction.FinishingPlaceId).GetComponentInChildren<Card>();

        if ((_card1 != null && !_card1.CanBeUsed) || _card2 != null && !_card2.CanBeUsed)
        {
            DialogsManager.Instance.ShowOkDialog("One of the cards cannot be used!");
            CardActionsDisplay.Instance.Close();
            ClearPossibleActions();
            return;
        }

        if (GameplayManager.Instance.IsAbilityActiveForOpponent<Tar>())
        {
            if (_newAction.Type == CardActionType.Move || _newAction.Type == CardActionType.SwitchPlace)
            {
                if (_card1 != null)
                {
                    if (_card1 is Guardian)
                    {
                        DialogsManager.Instance.ShowOkDialog("Action blocked due to Tar ability.");
                        return;
                    }
                }

                if (_card2 != null)
                {
                    if (_card2 is Guardian)
                    {
                        DialogsManager.Instance.ShowOkDialog("Action blocked due to Tar ability.");
                        return;
                    }
                }
            }
        }

        GameplayManager.Instance.ExecuteCardAction(_newAction);
        CardActionsDisplay.Instance.Close();
        ClearPossibleActions();
    }
}