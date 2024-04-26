using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class TableActionsHandler : MonoBehaviour
{
    private TableHandler tableHandler;
    private List<CardAction> possibleActions = new();
    private GameplayPlayer player;

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
        
        if (_placeId==0)
        {
            TablePlaceHandler _tablePlace = tableHandler.GetPlace(0);
            Card _cardAtStealth = _tablePlace.GetCardNoWall();
            if (_cardAtStealth)
            {
                UIManager.Instance.ShowYesNoDialog("To interact with card in stealth, you have to leave stealth, continue??", YesLeaveStealth, ClearPossibleActions);
            }
        
            return false;
        }

        if (_isCardInSelectedActions)
        {
            CheckForAction(tableHandler.GetPlace(_placeId));
            ClearPossibleActions();
            return false;
        }

        return true;
    }

    public void ShowPossibleActions(GameplayPlayer _player,TablePlaceHandler _clickedPlace, Card _card, 
    CardActionType _type)
    {
        player = _player;
        ClearPossibleActions();
        
        if (!_card.IsWarrior())
        {
            CardActionsDisplay.Instance.Close();
            return;
        }

        int _placeId = _clickedPlace.Id;

        CardMovementType _movementType = _card.Speed != 0 ? CardMovementType.EightDirections : _card.MovementType;
        int _range = _card.Speed != 0 ? _card.Speed : 1;
        List<TablePlaceHandler> _movablePlaces;
        if (_range==1)
        {
            _movablePlaces = tableHandler.GetPlacesAround(_placeId, _movementType, _range);
        }
        else
        {
            _movablePlaces = tableHandler.GetPlacesAroundNoCorners(_placeId, _movementType, _range);
        }
        // List<TablePlaceHandler> _attackablePlaces = tableHandler.GetPlacesAround(_placeId, _card.MovementType,GetRange(_card),true);
        if (_card.Stats.Range!=1)
        {
            _range = _card.Stats.Range;
        }
        List<TablePlaceHandler> _attackablePlaces = tableHandler.GetPlacesAround(_placeId, _card.MovementType,_range,true);
        switch (_type)
        {
            case CardActionType.Attack:
                AddAttackAction(_attackablePlaces, _card);
                break;
            case CardActionType.Move:
                if (SlowDown.IsActive)
                {
                    SlowDown _slowDown = FindObjectOfType<SlowDown>();
                    if (!_slowDown.CanMoveCard(_card))
                    {
                        UIManager.Instance.ShowOkDialog("Movement is blocked by SlowDown ability");
                        ClearPossibleActions();
                        return;
                    }
                }
                AddSwitchActions(_movablePlaces, _card,_range);
                AddRamAbility(_movablePlaces, _card);
                AddMovementActions(_movablePlaces, _card,_range);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_type), _type, null);
        }
           
        HandleChainedGuardian(_card);
        foreach (var _action in possibleActions)
        {
            _action.StartingPlaceId = _placeId;
            _action.IsMy = true;
            if (_card.Speed!=0)
            {
                _action.Cost = _action.Type == CardActionType.SwitchPlace ? 2 : 1;
            }
        }

        
    }

    private void AddRamAbility(List<TablePlaceHandler> _movablePlaces,CardBase _cardBase)
    {
        Card _card = _cardBase as Card;
        int _actionCost = 1;
        TablePlaceHandler _startingPlace= _cardBase.GetTablePlace();
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
                if (_placeAround.ContainsMarker || _placeAround.ContainsPortal||_placeAround.ContainsWall)
                {
                    _skip = true;
                }
            }

            if (_skip)
            {
                continue;
            }

            Card _cardAtPlace = _placeAround.GetCard();

            if (_cardAtPlace==null)
            {
                continue;
            }
            int _distance = tableHandler.DistanceBetweenPlaces(_startingPlace, _placeAround);
            if (_distance>1)
            {
                continue;
            }
           
            if (player.Actions<_actionCost)
            {
                continue;
            }

            possibleActions.Add( new CardAction()
            {
                FirstCardId = _card.Details.Id,
                StartingPlaceId = _startingPlace.Id,
                FinishingPlaceId=_placeAround.Id,
                SecondCardId = _cardAtPlace.Details.Id,
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
                        tableHandler.GetPlacesAround(_possibleAction.FinishingPlaceId,
                            CardMovementType.EightDirections);
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
                else if (_possibleAction.Type == CardActionType.Attack)
                {
                    continue;
                }
            }
        }
    }

    private void YesLeaveStealth()
    {
        TablePlaceHandler _stealthPlace = tableHandler.GetPlace(0);
        foreach (var _specialAbility in _stealthPlace.GetCardNoWall().SpecialAbilities)
        {
            if (_specialAbility is SniperStealth _sniper)
            {
                _sniper.LeaveStealth();
                break;
            }
        }
        
        ClearPossibleActions();
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

    private void AddMovementActions(List<TablePlaceHandler> _movablePlaces,CardBase _cardBase, int _speed)
    {
        int _actionCost = 1;
        Card _warriorCard = (_cardBase as Card);
        TablePlaceHandler _startingPlace= _cardBase.GetTablePlace();
        if (_warriorCard==null)
        {
            return;
        }
        foreach (var _placeAround in _movablePlaces)
        {
            bool _skip = false;
            if (_placeAround.IsOccupied)
            {
                _skip = true;
                
                if (_placeAround.ContainsWall && _warriorCard.CanMoveOnWall && !_placeAround.ContainsWarrior())
                {
                    _skip = false;
                }
                else if (_placeAround.ContainsMarker || _placeAround.ContainsPortal)
                {
                    _skip = false;
                }

                bool _hasLeapfrog = false;
                foreach (var _ability in _warriorCard.SpecialAbilities)
                {
                    if (_ability is ScalerLeapfrog)
                    {
                        _hasLeapfrog = true;
                        break;
                    }
                }

                if (_hasLeapfrog && _skip)
                {
                    AddCardInFront();
                    continue;
                }

                if (_hasLeapfrog && _placeAround.ContainsWall)
                {
                    AddCardInFront(true);
                }
                
            }

            //added this part to handle moving over the wall
            TablePlaceHandler _wallInPathPlace = tableHandler.GetPlaceWithWallInPath(_startingPlace, _placeAround);
            if (_wallInPathPlace!=null && _placeAround!=_wallInPathPlace)
            {
                int _distance = tableHandler.DistanceBetweenPlaces(_startingPlace, _placeAround);
                if (_distance>1)
                {
                    continue;
                }
            }
            //
            
           
            if (_skip)
            {
                continue;
            }

            if ((_cardBase is Card _card) && _card.Speed!=0)
            {
                if (_speed<CalculatePathCost(_startingPlace,_placeAround,_cardBase.MovementType,1, CardActionType.Move))
                {
                    continue;
                }
            }
            else if (player.Actions<_actionCost)
            {
                continue;
            }

            possibleActions.Add( new CardAction()
            {
                FirstCardId = _warriorCard.Details.Id,
                FinishingPlaceId=_placeAround.Id,
                Type = CardActionType.Move,
                Cost = _actionCost,
            });
            
            _placeAround.SetColor(Color.blue);
            
            void AddCardInFront(bool _dontAddIfItIsAWall=false)
            {
                var _cordsInFront = tableHandler.GetFrontIndex(_warriorCard.GetTablePlace().Id, _placeAround.Id);
                TablePlaceHandler _placeInFront = tableHandler.GetPlace(_cordsInFront);
                if (_placeInFront!=default && !_placeInFront.ContainsWarrior() && !_placeInFront.IsAbility)
                {
                    if (_dontAddIfItIsAWall&& _placeInFront.ContainsWall)
                    {
                        return;
                    }
                    possibleActions.Add( new CardAction()
                    {
                        FirstCardId = _warriorCard.Details.Id,
                        StartingPlaceId = _warriorCard.GetTablePlace().Id,
                        FinishingPlaceId=_placeInFront.Id,
                        Type = CardActionType.Move,
                        Cost = _actionCost,
                    });
                    _placeInFront.SetColor(Color.blue);
                }
                
            }
        }
    }

    private int CalculatePathCost(TablePlaceHandler _startingPlace, TablePlaceHandler _placeAround, CardMovementType 
    _movementType, int _startingPathCost, CardActionType _type)
    {
        int _pathCost = _startingPathCost;
        var _path = tableHandler.FindPath(_startingPlace, _placeAround, _movementType);
        if (_path.Count>=1)
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
        int _actionCost=2;
        
        foreach (var _placeAround in _movablePlaces)
        {
            if (!_placeAround.IsOccupied)
            {
                continue;
            }

            List<CardBase> _cardsAtPlace = _placeAround.GetCards();

            foreach (var _cardAtPlace in _cardsAtPlace)
            {
                if (!_cardAtPlace.My)
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

                if (_card.Speed!=0)
                {
                    if (_speed<CalculatePathCost(_card.GetTablePlace(),_placeAround,_card.MovementType,2, CardActionType.SwitchPlace))
                    {
                        continue;
                    }
                    if (player.Actions<_actionCost)
                    {
                        continue;
                    }
                }
                else if (player.Actions<_actionCost)
                {
                    continue;
                }
            
                possibleActions.Add( new CardAction
                {
                    FirstCardId = _card.Details.Id,
                    SecondCardId = ((Card)_cardAtPlace).Details.Id,
                    FinishingPlaceId=_placeAround.Id,
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
                int _newRange = tableHandler.DistanceBetweenPlaces(_attackingCardPlace, _attackablePlace);
                List<TablePlaceHandler> _placesAroundPortal = tableHandler.GetPlacesAround(_placeOfExitPortal.Id, _card.MovementType,
                _newRange);

                foreach (var _placeAroundPortal in _placesAroundPortal)
                {
                    int _newDistance = tableHandler.DistanceBetweenPlaces(_placeOfExitPortal, _placeAroundPortal);
                    TablePlaceHandler _newPlaceWithWall = tableHandler.GetPlaceWithWallInPath(_placeOfExitPortal, _placeAroundPortal);
                    if (tableHandler.GetDirection(_attackingCardPlace.Id,_attackablePlace.Id)!=
                        tableHandler.GetDirection(_placeOfExitPortal.Id,_placeAroundPortal.Id))
                    {
                        continue;
                    }
                    CheckAttackingPlace(_attackingCard,_placeAroundPortal,_newDistance, _attackingCardPlace,_newPlaceWithWall);
                }
                continue;
            }

            int _distance = tableHandler.DistanceBetweenPlaces(_attackingCardPlace, _attackablePlace);
            TablePlaceHandler _placeWithWall = tableHandler.GetPlaceWithWallInPath(_attackingCardPlace, _attackablePlace);
            CheckAttackingPlace(_attackingCard,_attackablePlace,_distance,_attackingCardPlace,_placeWithWall);
        }
    }

    private void CheckAttackingPlace(Card _attackingCard, TablePlaceHandler _attackablePlace,int _distance, 
    TablePlaceHandler _attackingCardPlace, TablePlaceHandler _placeWithWall)
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
                    if (_attackingCard.Stats.Range < _distance)
                    {
                        continue;
                    }

                    if (_placeWithWall.GetCards().Count != 1)
                    {
                        continue;
                    }
                }

                if (_placeWithWall.GetCards().Count > 1 && _attackedCard is Minion && _attackingCard.Stats.Range <= 1)
                {
                    continue;
                }
            }

            if (_attackingCardPlace.ContainsWall)
            {
                _distance--;
            }
            if (_attackingCard.Speed!=0)
            {
                if (_attackingCard.Speed<CalculatePathCost(_attackingCardPlace,_attackablePlace,_attackingCard
                        .MovementType,1, CardActionType.Attack))
                {
                    continue;
                }
            }
            else  if (_attackingCard.Stats.Range < _distance)
            {
                continue;
            }

            if (player.Actions < _actionCost)
            {
                continue;
            }

            possibleActions.Add(new CardAction()
            {
                FirstCardId = _attackingCard.Details.Id,
                SecondCardId = ((Card)_attackedCard).Details.Id,
                FinishingPlaceId = _attackablePlace.Id,
                Type = CardActionType.Attack,
                Cost = _actionCost,
            });

            _attackablePlace.SetColor(Color.green);
        }
    }

    private void CheckForAction(TablePlaceHandler _placeClicked)
    {
        List<CardAction> _triggeredActions= new List<CardAction>();
        foreach (var _possibleAction in possibleActions)
        {
            if (_possibleAction.FinishingPlaceId==_placeClicked.Id)
            {
                _triggeredActions.Add(_possibleAction);
            }
        }
        
        if (_triggeredActions.Count==0)
        {
            return;
        }
        
        if (_triggeredActions.Count==1)
        {
            ExecuteAction(_triggeredActions[0]);
        }
        else
        {
            ResolveMultipleActions.Instance.Show(_triggeredActions.ToList(),ExecuteAction);
        }
        
    }

    private void ExecuteAction(CardAction _action)
    {
        CardBase _attackingCard = tableHandler.GetPlace(_action.StartingPlaceId).GetComponentInChildren<CardBase>();
        CardBase _defendingCard = tableHandler.GetPlace(_action.FinishingPlaceId).GetComponentInChildren<CardBase>();
        if (_attackingCard != null && _defendingCard != null)
        {
            if (_action.Type==CardActionType.Attack)
            {
                _attackingCard = tableHandler.GetPlace(_action.StartingPlaceId).GetCards().Cast<Card>().ToList()
                    .Find(_card => _card.Details.Id == _action.FirstCardId);
                _defendingCard = tableHandler.GetPlace(_action.FinishingPlaceId).GetCards().Cast<Card>().ToList()
                    .Find(_card => _card.Details.Id == _action.SecondCardId);
                string _question = string.Empty;
                if (_attackingCard.My && _defendingCard.My)
                {
                    _question += $"Attacking your own {((Card)_defendingCard).Details.Type}, continue?";
                }
                else
                {
                    _question +=  $"Attacking {((Card)_defendingCard).Details.Type}, continue?";
                }

                if (_defendingCard is Wall _wall)
                {
                    if (_wall.Details.Faction==FactionSO.Get(0))//snow faction
                    {
                        _question += "\n(You wont be able to move this card until next turn)";
                    }
                    else if (_wall.Details.Faction==FactionSO.Get(1))
                    {
                        _question += "\n(Your card will be pushed back 1 tile)";
                    }
                }
                
                UIManager.Instance.ShowYesNoDialog(_question, () =>
                {
                    YesExecute(_action);
                });
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
                if (_cardBase is Card _possibleCard && _possibleCard.Details.Id==_action.FirstCardId)
                {
                    _cardAtPlace = _possibleCard;
                    break;
                }
            }

            if (_cardAtPlace==null)
            {
                return;
            }

            foreach (var _specialAbility in _cardAtPlace.SpecialAbilities)
            {
                if (_specialAbility is BlockaderRam _ramAbility)
                {
                    _ramAbility.TryAndPush(_action.StartingPlaceId,_action.FinishingPlaceId,
                        _action.FirstCardId, _action.SecondCardId);
                }
            }
            
            return;
        }

        if (_action.Type is CardActionType.Move or CardActionType.SwitchPlace or CardActionType.RamAbility)
        {
            if (Hinder.IsActive)
            {
                foreach (var _placeInRange in GameplayManager.Instance.TableHandler.GetPlacesAround(_action.StartingPlaceId, CardMovementType
                .EightDirections))
                {
                    if (!_placeInRange.IsOccupied || _placeInRange.GetCard() is not Keeper)
                    {
                        continue;
                    }
                    UIManager.Instance.ShowOkDialog("This card can't move due Hinder effect");
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
            IsMy = _action.IsMy,
            CanTransferLoot = _action.CanTransferLoot,
            Damage = _action.Damage,
            CanCounter = _action.CanCounter,
            FirstCardId = _action.FirstCardId,
            SecondCardId = _action.SecondCardId
        };
        
        if (_newAction.Type == CardActionType.SwitchPlace)
        {
            CardBase _cardOne = GameplayManager.Instance.TableHandler.GetPlace(_newAction.StartingPlaceId)
                .GetComponentInChildren<CardBase>();
            
            CardBase _cardTwo = GameplayManager.Instance.TableHandler.GetPlace(_newAction.FinishingPlaceId)
                .GetComponentInChildren<CardBase>();

            if (!_cardOne.CanMove || !_cardTwo.CanMove)
            {
                UIManager.Instance.ShowOkDialog("Cant swap, card cant move");
                return;
            }
        }
        else if (_newAction.Type == CardActionType.Move)
        {
            CardBase _cardOne = GameplayManager.Instance.TableHandler.GetPlace(_newAction.StartingPlaceId)
                .GetComponentInChildren<CardBase>();
           
            if (!_cardOne.CanMove)
            {
                UIManager.Instance.ShowOkDialog("Cant move this card");
                return;
            }
        }
        
        TablePlaceHandler _tablePlace = GameplayManager.Instance.TableHandler.GetPlace(_action.StartingPlaceId);
        Card _card = null;
        foreach (var _cardBase in _tablePlace.GetCards())
        {
            if (_cardBase is Card _cardOnPlace)
            {
                if (_cardOnPlace.Details.Id == _newAction.FirstCardId)
                {
                    _card = _cardOnPlace;
                    break;
                }
            }
        }
        if (_card!=null)
        {
            _card.Speed = 0;
        }
        
        CardBase _card1 = GameplayManager.Instance.TableHandler.GetPlace(_newAction.StartingPlaceId)
            .GetComponentInChildren<CardBase>();
            
        CardBase _card2 = GameplayManager.Instance.TableHandler.GetPlace(_newAction.FinishingPlaceId)
            .GetComponentInChildren<CardBase>();

        if ((_card1!=null&& !_card1.CanBeUsed) ||_card2!=null&& !_card2.CanBeUsed)
        {
            UIManager.Instance.ShowOkDialog("One of the cards cant be used!");
            CardActionsDisplay.Instance.Close();
            ClearPossibleActions();
            return;
        }

        if (Tar.IsActive)
        {
            if (_newAction.Type == CardActionType.Move || _newAction.Type == CardActionType.SwitchPlace)
            {
                if (_card1 != null)
                {
                    if (_card1 is Guardian)
                    {
                        UIManager.Instance.ShowOkDialog("Action blocked due to Tar ability");
                        return;
                    }
                }

                if (_card2 != null)
                {
                    if (_card2 is Guardian)
                    {
                        UIManager.Instance.ShowOkDialog("Action blocked due to Tar ability");
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