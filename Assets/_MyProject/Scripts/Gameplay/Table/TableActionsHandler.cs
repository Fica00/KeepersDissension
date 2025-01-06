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
        ClearPossibleActions();
    }

    public void Setup()
    {
        tableHandler = GameplayManager.Instance.TableHandler;
    }

    public bool ContinueWithShowingPossibleActions(int _placeId)
    {
        bool _isCardInSelectedActions = false;
        foreach (var _action in possibleActions)
        {
            if (_action.FinishingPlaceId == _placeId)
            {
                _isCardInSelectedActions = true;
            }
        }
        
        if (_isCardInSelectedActions)
        {
            CheckForAction(tableHandler.GetPlace(_placeId));
            ClearPossibleActions();
            return false;
        }

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
                List<TablePlaceHandler> _attackablePlaces = tableHandler.GetPlacesAround(_placeId, _card.MovementType, _attackingRange, true);
                AddAttackAction(_attackablePlaces, _card);
                break;
            case CardActionType.Move:
                int _movingSpace = _card.Speed != 0 ? _card.Speed : 1;
                var _movablePlaces = tableHandler.GetPlacesAround(_placeId, _movementType, _movingSpace);
                AddSwitchActions(_movablePlaces, _card, _movingSpace);
                AddRamAbility(_movablePlaces, _card);
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

        if (!_card.HasBlockaderRamAbility())
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
                if (!GameplayManager.Instance.IsMyResponseAction())
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
        if (_card is not Guardian { IsChained: true })
        {
            return;
        }
        
        foreach (var _possibleAction in possibleActions.ToList())
        {
            if (tableHandler.GetPlace(_possibleAction.FinishingPlaceId).ContainsPortal)
            {
                continue;
            }

            if (_possibleAction.Type != CardActionType.Move && _possibleAction.Type != CardActionType.SwitchPlace)
            {
                continue;
            }
            
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

            if (_foundLifeForce)
            {
                continue;
            }
            
            ClearPossibleAction(_possibleAction);
            possibleActions.Remove(_possibleAction);
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

                if (_placeAround.ContainsWall && _warriorCard.HasScaler() && !_placeAround.ContainsWarrior())
                {
                    _skip = false;
                }
                else if (_placeAround.ContainsMarker || _placeAround.ContainsPortal)
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
                AddLeapfrogMovement(_currentPlace, _placeAround, _warriorCard);
            }
            else if (_placeAround.ContainsWall)
            {
                foreach (var _ability in _warriorCard.SpecialAbilities)
                {
                    if (_ability is ScalerLeapfrog)
                    {
                        AddLeapfrogMovement(_currentPlace, _placeAround, _warriorCard);
                    }
                }
            }
        }
    }

    private void AddLeapfrogMovement(TablePlaceHandler _currentPlace, TablePlaceHandler _placeAround, Card _warriorCard)
    {
        if (!_warriorCard.HasScalerLeapfrog())
        {
            return;
        }
        
        Vector2 _cordsInFront = tableHandler.GetFrontIndex(_currentPlace.Id, _placeAround.Id);
        AddCardInFront(_warriorCard, _cordsInFront, 1, _dontAddIfItIsAWall: true);
    }

    private void AddCardInFront(Card _warriorCard, Vector2 _cordsInFront, int _actionCost, bool _dontAddIfItIsAWall = false)
    {
        TablePlaceHandler _placeInFront = tableHandler.GetPlace(_cordsInFront);
        if (_placeInFront == null)
        {
            return;
        }

        if (_placeInFront == default || _placeInFront.ContainsWarrior() || _placeInFront.IsAbility)
        {
            return;
        }
        
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

                if (_cardAtPlace.GetTablePlace().ContainsWall && !_card.HasScaler())
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
                if (!GameplayManager.Instance.IsMyResponseAction())
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
        bool _didAsk = TryAskForAction(_action);
        if (_didAsk)
        {
            return;
        }
        
        YesExecute(_action);
    }

    private bool TryAskForAction(CardAction _action)
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
                return true;
            }
        }

        return false;
    }

    private void YesExecute(CardAction _action)
    {
        CardAction _newAction = new CardAction
        {
            StartingPlaceId = _action.StartingPlaceId,
            FinishingPlaceId = _action.FinishingPlaceId,
            Type = _action.Type,
            Cost = _action.Cost,
            Damage = _action.Damage,
            FirstCardId = _action.FirstCardId,
            SecondCardId = _action.SecondCardId
        };
        
        if (!TryToUseRam(_newAction))
        {
            return;
        }

        if (!CheckForHinder(_newAction))
        {
            return;
        }

        if (!CheckIfCardCanMove(_newAction))
        {
            return;
        }

        if (!CheckForTar(_newAction))
        {
            return;
        }

        if (!CheckForSlowDown(_newAction))
        {
            return;
        }
        
        bool _continue =TryHandlePortalMove(_newAction);
        
        CardActionsDisplay.Instance.Close();
        ClearPossibleActions();
        
        if (_continue)
        {
            GameplayManager.Instance.ExecuteCardAction(_newAction);
        }

        CardActionsDisplay.Instance.Close();
        ClearPossibleActions();
    }

    private bool TryHandlePortalMove(CardAction _action)
    {
        if (!GameplayManager.Instance.IsAbilityActive<Portal>())
        {
            return true;
        }

        if (_action.Type != CardActionType.Move)
        {
            return true;
        }

        
        Card _cardThatMoved = GameplayManager.Instance.GetCard(_action.FirstCardId);
        int _finishingPlace = _action.FinishingPlaceId;
        int _startingPlace = _action.StartingPlaceId;
        (Card _enteredPortal, Card _exitPortal) = GameplayManager.Instance.TableHandler.GetPortals(_finishingPlace);

        if (_enteredPortal == null)
        {
            return true;
        }
        

        int _exitIndex = GameplayManager.Instance.TableHandler.GetTeleportExitIndex(_startingPlace, _enteredPortal.GetTablePlace().Id,
            _exitPortal.GetTablePlace().Id);

        if (_exitIndex == -1 || GameplayManager.Instance.TableHandler.GetPlace(_exitIndex).IsOccupied)
        {
            Debug.Log("Place on the other side is occupied, damaging my self");
            GameplayManager.Instance.DamageCardByAbility(_cardThatMoved.UniqueId, 1, null);
            return false;
        }

        Debug.Log("moving to the new place");
        GameplayManager.Instance.ExecuteMove(_startingPlace,_exitIndex, _cardThatMoved.UniqueId,null);
        return false;
    }

    private bool TryToUseRam(CardAction _action)
    {
        if (_action.Type != CardActionType.RamAbility)
        {
            return true;
        }
        
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
            return false;
        }

        foreach (var _specialAbility in _cardAtPlace.SpecialAbilities)
        {
            if (_specialAbility is BlockaderRam _ramAbility)
            {
                _ramAbility.TryAndPush(_action.FirstCardId, _action.SecondCardId);
            }
        }

        return false;
    }

    private bool CheckForHinder(CardAction _action)
    {
        if (_action.Type is not (CardActionType.Move or CardActionType.SwitchPlace or CardActionType.RamAbility))
        {
            return true;
        }

        if (!GameplayManager.Instance.IsAbilityActive<Hinder>())
        {
            return true;
        }
        
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
            return false;
        }

        return true;
    }

    private bool CheckIfCardCanMove(CardAction _action)
    {
        Card _cardOne = GameplayManager.Instance.GetCard(_action.FirstCardId);
        if (_action.Type == CardActionType.SwitchPlace)
        {
            Card _cardTwo = GameplayManager.Instance.GetCard(_action.SecondCardId);
            if (!_cardOne.CheckCanMove() || !_cardTwo.CheckCanMove())
            {
                DialogsManager.Instance.ShowOkDialog("Cannot swap, one of the cards cannot move.");
                return false;
            }
        }
        else if (_action.Type == CardActionType.Move)
        {
            if (!_cardOne.CheckCanMove())
            {
                DialogsManager.Instance.ShowOkDialog("Cannot move this card.");
                return false;
            }
        }

        return true;
    }

    private bool CheckForTar(CardAction _action)
    {
        Card _card1 = GameplayManager.Instance.GetCard(_action.FirstCardId);
        Card _card2 = GameplayManager.Instance.GetCard(_action.SecondCardId);

        if (!GameplayManager.Instance.IsAbilityActiveForOpponent<Tar>())
        {
            return true;
        }
        
        if (_action.Type == CardActionType.Move || _action.Type == CardActionType.SwitchPlace)
        {
            if (_card1 != null)
            {
                if (_card1 is Guardian)
                {
                    DialogsManager.Instance.ShowOkDialog("Action blocked due to Tar ability.");
                    return false;
                }
            }

            if (_card2 != null)
            {
                if (_card2 is Guardian)
                {
                    DialogsManager.Instance.ShowOkDialog("Action blocked due to Tar ability.");
                    return false;
                }
            }
        }

        return true;
    }
    private bool CheckForSlowDown(CardAction _action)
    {
        if (_action.Type != CardActionType.Move)
        {
            return true;
        }

        if (!GameplayManager.Instance.IsAbilityActive<SlowDown>())
        {
            return true;
        }
        
        Card _card1 = GameplayManager.Instance.GetCard(_action.FirstCardId);
        SlowDown _slowDown = FindObjectOfType<SlowDown>();

        if (!_slowDown.CanMoveCard(_card1.UniqueId))
        {
            DialogsManager.Instance.ShowOkDialog("Action blocked by SlowDown");
            return false;
        }
        return true;
    }
}