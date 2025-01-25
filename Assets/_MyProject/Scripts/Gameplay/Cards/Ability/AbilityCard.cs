using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCard : CardBase
{
    [SerializeField] private AbilityEffect effect;
    [SerializeField] private GameObject activeDisplay;
    
    public AbilityCardDetails Details;
    public GameObject ActiveDisplay => activeDisplay;
    public AbilityEffect Effect => effect;
    
    public AbilityData Data => FirebaseManager.Instance.RoomHandler.BoardData.Abilities.Find(_card => _card.UniqueId == uniqueId);
    
    private string uniqueId;

    public bool My => Data.Owner == FirebaseManager.Instance.PlayerId;
    public string UniqueId => Data.UniqueId;
    public int PlaceId => Data.PlaceId;
    public bool IsActive => Data.IsActive;
    public bool IsApplied => Data.IsApplied;
    public int RemainingCooldown => Data.RemainingCooldown;
    public int Multiplayer => Data.Multiplayer;
    public int StartingRange => Data.StartingRange;
    public int StartingDamage => Data.StartingDamage;
    public bool CanExecuteThisTurn => Data.CanExecuteThisTurn;
    public int StartingHealth => Data.StartingHealth;
    public int OpponentsStartingHealth => Data.OpponentsStartingHealth;
    public bool HasMyRequiredCardDied => Data.HasMyRequiredCardDied;
    public bool HasOpponentsRequiredCardDied => Data.HasOpponentsRequiredCardDied;
    public List<string> EffectedCards => Data.EffectedCards;

    public void Setup(string _cardId)
    {
        uniqueId = _cardId;
        Display.Setup(this);
    }

    public AbilityData CreateData(string _owner)
    {
        string _name = string.Empty;
        foreach (var _char in gameObject.name)
        {
            if (!char.IsLetter(_char))
            {
                continue;
            }

            _name += _char;
        }
        
        return new AbilityData
        {
            Name = _name,
            UniqueId = Guid.NewGuid().ToString(),
            Owner = _owner,
            RemainingCooldown = 0,
            PlaceId = -100,
            Cooldown = 0,
            IsActive = false,
            Type = Details.Type,
            EffectedCards = null,
            CardPlace = CardPlace.Deck,
            CardId = Details.Id,
            IsApplied = false,
            Multiplayer = 0,
            CanExecuteThisTurn = false,
            StartingRange = 0,
            StartingDamage = 0,
            StartingHealth = 0,
            HasMyRequiredCardDied = false,
            HasOpponentsRequiredCardDied = false,
            OpponentsStartingHealth = 0,
            Color = Details.Color,
            CanBeGivenToPlayer = Details.CanBeGivenToPlayer
        };
    }

    public void SetIsMy(string _owner)
    {
        Data.Owner = _owner;
    }

    private void OnEnable()
    {
        TablePlaceHandler.OnPlaceClicked += TryActivateCard;
    }

    private void OnDisable()
    {
        TablePlaceHandler.OnPlaceClicked -= TryActivateCard;
    }

    private void TryActivateCard(TablePlaceHandler _clickedPlace)
    {
        if (!CanActivate(_clickedPlace))
        {
            return;
        }
        
        DialogsManager.Instance.ShowYesNoDialog("Are you sure that you want to activate this ability?",DoActivate);
    }

    private bool CanActivate(TablePlaceHandler _clickedPlace)
    {
        if (GetTablePlace() == null)
        {
            return false;
        }

        if (GetTablePlace()!=_clickedPlace)
        {
            return false;
        }

        if (_clickedPlace.Id is -1 or 65)
        {
            return false;
        }
        
        if (Details.Type!=AbilityCardType.CrowdControl)
        {
            return false;
        }
        
        if (!GameplayManager.Instance.IsMyTurn())
        {
            if (GameplayManager.Instance.IsKeeperResponseAction)
            {
            }
            else
            {
                return false;

            }
        }

        if (!My)
        {
            return false;
        }
        
        if (GameplayManager.Instance.IsAbilityActive<Veto>())
        {
            Veto _veto = FindObjectOfType<Veto>();
            if (_veto.IsCardEffected(UniqueId))
            {
                DialogsManager.Instance.ShowOkDialog("Ability blocked by Veto");
                return false;
            }
        }

        if (!GameplayManager.Instance.IsMyResponseAction())
        {
            if (GameplayManager.Instance.MyPlayer.Actions<=0)
            {
                DialogsManager.Instance.ShowOkDialog("You need 1 action to activate this ability");
                return false;
            }
        }

        if (GameplayManager.Instance.IsAbilityActive<Subdued>())
        {
            DialogsManager.Instance.ShowOkDialog("Activation of the ability is blocked by Subdued ability");
            return false;
        }

        return true;
    }
    
    private void DoActivate()
    {
        if (GameplayManager.Instance.IsAbilityActive<Subdued>())
        {
            DialogsManager.Instance.ShowOkDialog("Subdued is active");
            return;
        }
        
        GameplayManager.Instance.ActivateAbility(UniqueId);
    }

    public void SetParent(Transform _parent)
    {
        Parent = _parent;
        transform.SetParent(_parent);
        ResetPosition();
    }

    public void Activate()
    {
        if (IsActive)
        {
            effect.RemoveAction();
            return;
        }
        
        effect.TryToActivate();
    }

    public override bool GetIsMy()
    {
        return My;
    }
    
    public override bool IsWarrior()
    {
        return false;
    }
    
    public override bool IsLifeForce()
    {
        return false;
    }
    
    public void SetIsActive(bool _status)
    {
        Data.IsActive = _status;
    }
    
    public void SetIsLightUp(bool _status)
    {
        Data.IsLightUp = _status;
    }

    public void AddEffectedCard(string _cardUniqueId)
    {
        Data.EffectedCards.Add(_cardUniqueId);
    }
    
    public void RemoveEffectedCard(string _cardUniqueId)
    {
        Data.EffectedCards.Remove(_cardUniqueId);
    }

    public void ClearEffectedCards()
    {
        Data.EffectedCards.Clear();
    }

    public void SetIsApplied(bool _status)
    {
        Data.IsApplied = _status;
    }

    public void SetStartingDamage(int _amount)
    {
        Data.StartingDamage = _amount;
    }

    public void SetStartingRange(int _amount)
    {
        Data.StartingRange = _amount;
    }

    public void SetRemainingCooldown(int _amount)
    {
        Data.RemainingCooldown = _amount;
    }
    
    public void SetCanExecuteThisTurn(bool _status)
    {
        Data.CanExecuteThisTurn = _status;
    }
    
    public void ChangeStartingHealth(int _amount)
    {
        Data.StartingHealth += _amount;
    }

    public void SetStartingHealth(int _amount)
    {
        Data.StartingHealth = _amount;
    }
    
    public void ChangeOpponentsStartingHealth(int _amount)
    {
        Data.OpponentsStartingHealth += _amount;
    }

    public void SetOpponentsStartingHealth(int _amount)
    {
        Data.OpponentsStartingHealth = _amount;
    }
    
    public void SetHasMyRequiredCardDied(bool _status)
    {
        Data.HasMyRequiredCardDied = _status;
    }

    public void SetHasOpponentsRequiredCardDied(bool _status)
    {
        Data.HasOpponentsRequiredCardDied = _status;
    }

    public void SetMultiplayer(int _amount)
    {
        Data.Multiplayer = _amount;
    }
    
    public void ChangeMultiplayer(int _amount)
    {
        Data.Multiplayer += _amount;
    }

    public void SetPlaceId(int _placeId)
    {
        Data.PlaceId = _placeId;
    }
    
    public void ManageActiveDisplay(bool _status)
    {
        ActiveDisplay.gameObject.SetActive(_status);
    }

    public float GetBringBackPercentage()
    {
        float _minTransparency = 0.2f;
        
        var _activationFiled = GetIsMy()
            ? GameplayManager.Instance.MyPlayer.TableSideHandler.ActivationField
            : GameplayManager.Instance.OpponentPlayer.TableSideHandler.ActivationField;
        int _amountOfCardsInActivationField = _activationFiled.GetCards().Count ;
        float _amountOfCardsOnTop = _amountOfCardsInActivationField - Data.PlaceInActivationField; 
        float _percentage;
        if (_amountOfCardsOnTop == 0)
        {
            _percentage = 0;
        }
        else
        {
            _percentage = _amountOfCardsOnTop / effect.Cooldown;
            if (_percentage > 1)
            {
                _percentage = 1;
            }
        }

        if (_percentage > _minTransparency)
        {
            _percentage = _minTransparency;
        }

        return _percentage;
    }
}