using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityEffect : MonoBehaviour
{
    public Action OnActivated;
    [SerializeField] private int cooldown;
    private AbilityCard AbilityCard => GetComponentInParent<AbilityCard>();

    public int Cooldown => cooldown;
    public bool IsActive => AbilityCard.IsActive;
    public bool IsMy => AbilityCard.My;
    protected bool IsApplied => AbilityCard.IsApplied;
    protected int StartingRange => AbilityCard.StartingRange;
    protected int StartingDamage => AbilityCard.StartingDamage;
    protected int RemainingCooldown => AbilityCard.RemainingCooldown;
    protected int StartingHealth => AbilityCard.StartingHealth;
    protected int Multiplayer => AbilityCard.Multiplayer;
    protected int OpponentsStartingHealth => AbilityCard.OpponentsStartingHealth;
    protected bool HasMyRequiredCardDied => AbilityCard.HasMyRequiredCardDied;
    protected bool HasOpponentsRequiredCardDied => AbilityCard.HasOpponentsRequiredCardDied;
    protected string UniqueId => AbilityCard.UniqueId;
    protected int PlaceId => AbilityCard.PlaceId;
    public bool CanExecuteThisTurn => AbilityCard.CanExecuteThisTurn;


    public void TryToActivate()
    {
        if (IsActive)
        {
            RemoveAction();
            if (cooldown!=0)
            {
                MoveToActivationField();
                OnActivated?.Invoke();
            }
            return;
        }
        
        ActivateForOwner();
    }

    protected virtual void ActivateForOwner()
    {
        
    }

    private void OnDisable()
    {
        TryToCancel();
    }

    public void TryToCancel()
    {
        if (!IsActive)
        {
            return;
        }
        
        CancelEffect();
    }

    protected virtual void CancelEffect()
    {
        throw new Exception();
    }

    public virtual void SetupAfterGameReset()
    {
        throw new Exception();
    }

    protected void MoveToActivationField()
    {
        GameplayPlayer _player = AbilityCard.My ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;

        var _tablePlace = AbilityCard.GetTablePlace();
        if (_tablePlace!=null)
        {
            if (_tablePlace.Id is 0 or 64)
            {
                return;
            }
        }
        
        CardAction _action = new CardAction
        {
            StartingPlaceId = AbilityCard == null ? 1 : AbilityCard.GetTablePlace().Id,
            FirstCardId = AbilityCard.Details.Id,
            FinishingPlaceId = _player.TableSideHandler.ActivationField.Id,
            Type = CardActionType.MoveAbility,
            Cost = 0,
            IsMy = true,
            CanTransferLoot = false,
            Damage = 0,
            CanCounter = false
        };
        GameplayManager.Instance.ExecuteCardAction(_action);
    }

    protected void RemoveAction()
    {
        if (!AbilityCard.My)
        {
            return;
        }

        if (AbilityCard.Details.Type!=AbilityCardType.Passive)
        {
            GameplayManager.Instance.MyPlayer.Actions--;
        }
    }

    protected void SetIsActive(bool _status)
    {
        AbilityCard.SetIsActive(_status);
    }

    protected void ManageActiveDisplay(bool _status)
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(_status);
    }

    protected void AddEffectedCard(string _uniqueId)
    {
        AbilityCard.AddEffectedCard(_uniqueId);
    }

    protected void SetCanExecuteThisTurn(bool _status)
    {
        AbilityCard.SetCanExecuteThisTurn(_status);
    }

    protected void RemoveEffectedCard(string _uniqueId)
    {
        AbilityCard.RemoveEffectedCard(_uniqueId);
    }

    protected void ClearEffectedCards()
    {
        AbilityCard.ClearEffectedCards();
    }

    protected void SetIsApplied(bool _status)
    {
        AbilityCard.SetIsApplied(_status);
    }
    
    protected void SetStartingDamage(int _amount)
    {
        AbilityCard.SetStartingDamage(_amount);
    }

    protected void SetStartingRange(int _amount)
    {
        AbilityCard.SetStartingRange(_amount);
    }
    
    protected void SetRemainingCooldown(int _amount)
    {
        AbilityCard.SetRemainingCooldown(_amount);
    }
    
    protected void ChangeStartingHealth(int _amount)
    {
        AbilityCard.ChangeStartingHealth(_amount);
    }

    protected void SetStartingHealth(int _amount)
    {
        AbilityCard.SetStartingHealth(_amount);
    }
    
    protected void ChangeOpponentsStartingHealth(int _amount)
    {
        AbilityCard.ChangeOpponentsStartingHealth(_amount);
    }
    
    protected void SetMultiplayer(int _amount)
    {
        AbilityCard.SetMultiplayer(_amount);
    }
    
    protected void ChangeMultiplayer(int _amount)
    {
        AbilityCard.ChangeMultiplayer(_amount);
    }

    protected void SetOpponentsStartingHealth(int _amount)
    {
        AbilityCard.SetOpponentsStartingHealth(_amount);
    }
    
    protected void SetHasMyRequiredCardDied(bool _status)
    {
        AbilityCard.SetHasMyRequiredCardDied(_status);
    }

    protected void SetHasOpponentsRequiredCardDied(bool _status)
    {
        AbilityCard.SetHasOpponentsRequiredCardDied(_status);
    }
    
    public bool IsEffected(string _uniqueId)
    {
        foreach (var _effectedCard in GetEffectedCards())
        {
            if (_effectedCard.UniqueId == _uniqueId)
            {
                return true;
            }
        }

        return false;
    }
    
    protected List<Card> GetEffectedCards()
    {
        List<Card> _effectedCards = new();
        List<Card> _allCards = FindObjectsOfType<Card>().ToList();
        
        foreach (var _cardOnField in _allCards)
        {
            if (!AbilityCard.EffectedCards.Contains(_cardOnField.UniqueId))
            {
                continue;
            }
            
            _effectedCards.Add(_cardOnField);
        }

        return _effectedCards;
    }
}