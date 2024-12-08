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
    
    private AbilityData abilityData;

    public bool My => abilityData.Owner == FirebaseManager.Instance.PlayerId;
    public string UniqueId => abilityData.UniqueId;
    public bool IsVetoed => abilityData.IsVetoed;
    public int PlaceId => abilityData.PlaceId;
    public bool IsActive => abilityData.IsActive;
    public bool IsApplied => abilityData.IsApplied;
    public int RemainingCooldown => abilityData.RemainingCooldown;
    public int Multiplayer => abilityData.Multiplayer;
    public int StartingRange => abilityData.StartingRange;
    public int StartingDamage => abilityData.StartingDamage;
    public bool CanExecuteThisTurn => abilityData.CanExecuteThisTurn;
    public int StartingHealth => abilityData.StartingHealth;
    public int OpponentsStartingHealth => abilityData.OpponentsStartingHealth;
    public bool HasMyRequiredCardDied => abilityData.HasMyRequiredCardDied;
    public bool HasOpponentsRequiredCardDied => abilityData.HasOpponentsRequiredCardDied;
    public List<string> EffectedCards => abilityData.EffectedCards;

    public void Setup(string _owner)
    {
        abilityData = new AbilityData { Owner = _owner, IsVetoed = false, Cooldown = Effect.Cooldown, UniqueId = Guid.NewGuid().ToString()};
        Display.Setup(this);
    }

    public void SetIsMy(string _owner)
    {
        abilityData.Owner = _owner;
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

        if (abilityData.IsVetoed)
        {
            return false;
        }
        
        if (Details.Type!=AbilityCardType.CrowdControl)
        {
            return false;
        }
        if (GameplayManager.Instance.GameState != GameplayState.Playing)
        {
            return false;
        }

        if (!My)
        {
            return false;
        }

        if (GameplayManager.Instance.MyPlayer.Actions<=0)
        {
            DialogsManager.Instance.ShowOkDialog("You need 1 action to activate this ability");
            return false;
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
        if (GameplayManager.Instance.IsCardTaxed(UniqueId))
        {
            if (GameplayManager.Instance.MyPlayer.StrangeMatter<=0)
            {
                DialogsManager.Instance.ShowOkDialog("You don't have enough strange matter to pay Tax");
                return;
            }

            GameplayManager.Instance.MyPlayer.StrangeMatter--;
            GameplayManager.Instance.ActivatedTaxedCard();
            GameplayManager.Instance.TellOpponentToUpdateMyStrangeMatter();
        }
        
        GameplayManager.Instance.ActivateAbility(Details.Id);
    }

    public void SetParent(Transform _parent)
    {
        Parent = _parent;
        transform.SetParent(_parent);
        ResetPosition();
    }

    public void Activate()
    {
        if (abilityData.IsVetoed)
        {
            return;
        }
        
        effect.ActivateForOwner();
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
        abilityData.IsActive = _status;
    }

    public void AddEffectedCard(string _cardUniqueId)
    {
        abilityData.EffectedCards.Add(_cardUniqueId);
    }
    
    public void ResetEffectedCards()
    {
        abilityData.EffectedCards.Clear();
    }
    
    public void RemoveEffectedCard(string _cardUniqueId)
    {
        abilityData.EffectedCards.Remove(_cardUniqueId);
    }

    public void SetIsApplied(bool _status)
    {
        abilityData.IsApplied = _status;
    }

    public void SetStartingDamage(int _amount)
    {
        abilityData.StartingDamage = _amount;
    }

    public void SetStartingRange(int _amount)
    {
        abilityData.StartingRange = _amount;
    }

    public void SetRemainingCooldown(int _amount)
    {
        abilityData.RemainingCooldown = _amount;
    }
    
    public void SetCanExecuteThisTurn(bool _status)
    {
        abilityData.CanExecuteThisTurn = _status;
    }
    
    public void ChangeStartingHealth(int _amount)
    {
        abilityData.StartingHealth += _amount;
    }

    public void SetStartingHealth(int _amount)
    {
        abilityData.StartingHealth = _amount;
    }
    
    public void ChangeOpponentsStartingHealth(int _amount)
    {
        abilityData.OpponentsStartingHealth += _amount;
    }

    public void SetOpponentsStartingHealth(int _amount)
    {
        abilityData.OpponentsStartingHealth = _amount;
    }
    
    public void SetHasMyRequiredCardDied(bool _status)
    {
        abilityData.HasMyRequiredCardDied = _status;
    }

    public void SetHasOpponentsRequiredCardDied(bool _status)
    {
        abilityData.HasOpponentsRequiredCardDied = _status;
    }

    public void SetMultiplayer(int _amount)
    {
        abilityData.Multiplayer = _amount;
    }
    
    public void ChangeMultiplayer(int _amount)
    {
        abilityData.Multiplayer += _amount;
    }

    public void SetPlaceId(int _placeId)
    {
        abilityData.PlaceId = _placeId;
    }
}