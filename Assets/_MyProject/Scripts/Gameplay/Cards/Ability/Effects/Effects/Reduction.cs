using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Reduction : AbilityEffect
{
    private bool iFinished;
    private bool opponentFinished;
    private GameplayState state;
    
    public override void ActivateForOwner()
    {
        bool _skipExecute = false;
        foreach (var _lifeForce in FindObjectsOfType<LifeForce>())
        {
            if (_lifeForce.Stats.Health<=0)
            {
                _skipExecute = true;
            }
        }

        if (_skipExecute)
        {
            OnActivated?.Invoke();
            return;
        }
        MoveToActivationField();
        iFinished = false;
        opponentFinished = false;
        Activate(()=>
        {
            iFinished = true;
            CheckForEnd();
        });
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        bool _skipExecute = false;
        foreach (var _lifeForce in FindObjectsOfType<LifeForce>())
        {
            if (_lifeForce.Stats.Health<=0)
            {
                _skipExecute = true;
            }
        }

        if (_skipExecute)
        {
            OnActivated?.Invoke();
            return;
        }
        iFinished = false;
        opponentFinished = false;
        Activate(TellOpponentThatIFinishedReduction);
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public void Activate(Action _callBack)
    {
        state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.SelectingCardFromTable;
        List<Card> _cards = FindObjectsOfType<Card>().ToList().FindAll((_card) => _card.My).ToList();
        List<TablePlaceHandler> _availablePlaces = new();
        foreach (var _card in _cards)
        {
            if (!_card.IsWarrior())
            {
                continue;
            }
            TablePlaceHandler _tablePlace = _card.GetTablePlace();
            if (_tablePlace==null)
            {
                continue;
            }
            _availablePlaces.Add(_tablePlace);
        }
        
        if (_availablePlaces.Count==0)
        {
            _callBack?.Invoke();
            UIManager.Instance.ShowOkDialog("You dont have warrior to sacrifice");
            RemoveAction();
            return;
        }
        
        foreach (var _availablePlace in _availablePlaces)
        {
            _availablePlace.SetColor(Color.green);
        }

        UIManager.Instance.ShowOkDialog("Please select warrior to be sacrificed");
        TablePlaceHandler.OnPlaceClicked += CheckClickedPlace;


        void CheckClickedPlace(TablePlaceHandler _clickedPlace)
        {
            if (!_availablePlaces.Contains(_clickedPlace))
            {
                return;
            }
            
            foreach (var _availablePlace in _availablePlaces)
            {
                _availablePlace.SetColor(Color.white);
            }
            
            TablePlaceHandler.OnPlaceClicked -= CheckClickedPlace;

            CardAction _attackAction = new CardAction
            {
                StartingPlaceId = _clickedPlace.Id,
                FirstCardId = _clickedPlace.GetCardNoWall().Details.Id,
                FinishingPlaceId = _clickedPlace.Id,
                SecondCardId = _clickedPlace.GetCardNoWall().Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = false,
                Damage = 10,
                CanCounter = false
            };
            GameplayManager.Instance.ExecuteCardAction(_attackAction);
            _callBack?.Invoke();
            RemoveAction();
        }
    }

    private void TellOpponentThatIFinishedReduction()
    {
        GameplayManager.Instance.GameState = state;
        GameplayManager.Instance.FinishedReductionAction();
    }

    public void OpponentFinishedAction()
    {
        opponentFinished = true;
        UIManager.Instance.ShowOkDialog("Opponent selected warrior");
        CheckForEnd();
    }

    private void CheckForEnd()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        if (!opponentFinished)
        {
            UIManager.Instance.ShowOkDialog("Waiting for opponent to select a warrior");
            return;
        }

        if (!iFinished)
        {
            return;
        }
        
        GameplayManager.Instance.GameState = state;
        OnActivated?.Invoke();
    }
}
