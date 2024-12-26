using System;
using UnityEngine;

public class Card : CardBase
{
    public Action UpdatedHealth;

    public CardDetails Details;

    private string uniqueId;
    public CardData CardData => FirebaseManager.Instance.RoomHandler.BoardData.Cards.Find(_card => _card.UniqueId == uniqueId);
    public int Health=> CardData.Stats.Health;
    public int Range => CardData.Stats.Range;
    public int Damage => CardData.Stats.Damage;
    public bool IsVoid => CardData.IsVoid;
    public int Speed => CardData.Stats.Speed;
    public bool HasDelivery => CardData.HasDelivery;
    public bool CanBeUsed => CardData.CanBeUsed;
    public bool My => CardData.Owner == FirebaseManager.Instance.PlayerId;
    public CardMovementType MovementType => CardData.MovementType;
    public bool HasDied => CardData.HasDied;
    public string UniqueId => CardData.UniqueId;
    public int PercentageOfHealthToRecover => CardData.PercentageOfHealthToRecover;
    

    public void Setup(string _uniqueId)
    {
        uniqueId = _uniqueId;
        Display.Setup(this);
        Setup();
    }

    public CardData GenerateCardData(string _owner, string _uniqueId)
    {
        return new CardData
        {
            Owner = _owner,
            UniqueId = _uniqueId,
            CardId = Details.Id,
            IsVoid = false,
            HasDelivery = false,
            MovementType = CardMovementType.FourDirections,
            Stats = new CardStats { 
                Damage = Details.Stats.Damage,
                Health = Details.Stats.Health,
                Range = Details.Stats.Range,
                Speed =  Details.Stats.Speed,
                MaxHealth = Details.Stats.MaxHealth},
        };
    }

    public void SetUniqueId(string _uniqueCardId)
    {
        uniqueId = _uniqueCardId;
    }

    public void SetParent(Transform _parent)
    {
        Parent = _parent;
        transform.SetParent(_parent);
        ResetPosition();
    }

    protected virtual void Setup()
    {
        //if a card like Keeper needs to initialize its own values
    }

    public void HealFull()
    {
        float _amount = Details.Stats.Health - CardData.Stats.Health;
        ChangeHealth((int)_amount);
        UpdatedHealth?.Invoke();
    }
    
    public void SetHealth(int _amount)
    {
        int _alteredAmount = Math.Clamp(_amount, 0, Details.Stats.Health);
        CardData.Stats.Health = _alteredAmount;
        UpdatedHealth?.Invoke();
    }

    public void ChangeHealth(int _amount)
    {
        int _newHealth = Math.Clamp(CardData.Stats.Health + _amount, 0, Details.Stats.Health);
        CardData.Stats.Health = _newHealth;
        UpdatedHealth?.Invoke();
    }

    public void Hide()
    {
        Display.gameObject.SetActive(false);
    }

    public void UnHide()
    {
        Display.gameObject.SetActive(true);
    }

    public void ChangeDamage(int _amount)
    {
        CardData.Stats.Damage += _amount;
    }

    public void SetDamage(int _amount)
    {
        CardData.Stats.Damage = _amount;
    }

    public void ChangeRange(int _amount)
    {
        CardData.Stats.Range += _amount;
    }
    
    public void SetRange(int _amount)
    {
        CardData.Stats.Range = _amount;
    }

    public void ChangeSpeed(int _amount)
    {
        CardData.Stats.Speed += _amount;
    }

    public void SetSpeed(int _amount)
    {
        CardData.Stats.Speed = _amount;
    }

    public void SetMaxHealth(int _amount)
    {
        CardData.Stats.MaxHealth = _amount;
    }

    public void SetCanFlyToDodgeAttack(bool _status)
    {
        CardData.HasDelivery = _status;
    }

    public void SetIsVoid(bool _status)
    {
        CardData.IsVoid = _status;
    }

    public void SetCanBeUsed(bool _status)
    {
        CardData.CanBeUsed = _status;
    }
    
    public void ChangeOwner()
    {
        CardData.Owner = GetIsMy() ? FirebaseManager.Instance.OpponentId : FirebaseManager.Instance.PlayerId;
        SetRotation();
    }

    public void SetPercentageOfHealthToRecover(int _amount)
    {
        CardData.PercentageOfHealthToRecover = _amount;
    }

    public void ChangePercentageOfHealthToRecover(int _amount)
    {
        CardData.PercentageOfHealthToRecover += _amount;
    }

    public void ChangeMovementType(CardMovementType _movementType)
    {
        CardData.MovementType = _movementType;
    }

    public void SetHasDied(bool _status)
    {
        CardData.HasDied = _status;
        if (CardData.HasDied)
        {
            CardData.PlaceId = -100;
        }
    }
    
    
    public override bool GetIsMy()
    {
        return My;
    }
    
    public override bool IsWarrior()
    {
        CardType _type = Details.Type;
        return _type is CardType.Minion or CardType.Guardian or CardType.Keeper;
    }

    public override bool IsAttackable()
    {
        CardType _type = Details.Type;
        return  IsWarrior() || _type is CardType.Wall or CardType.LifeForce or CardType.Marker;
    }
    
    public override bool IsLifeForce()
    {
        CardType _type = Details.Type;
        return _type is CardType.LifeForce;
    }

    public bool HasBlockaderAbility()
    {
        BlockaderCard _blockader = EffectsHolder.GetComponent<BlockaderCard>();
        if (_blockader== null)
        {
            return false;
        }

        return true;
    }
    
    public bool HasBlockaderRamAbility()
    {
        BlockaderRam _blockader = EffectsHolder.GetComponent<BlockaderRam>();
        if (_blockader== null)
        {
            return false;
        }

        return true;
    }    
    
    public bool HasScalerLeapfrog()
    {
        ScalerLeapfrog _blockader = EffectsHolder.GetComponent<ScalerLeapfrog>();
        if (_blockader== null)
        {
            return false;
        }

        return true;
    }    
    
    public bool HasScaler()
    {
        ScalerScale _scale = EffectsHolder.GetComponent<ScalerScale>();
        if (_scale== null)
        {
            return false;
        }

        return true;
    }    
    
    public bool HasVision()
    {
        ScoutVision _vision = EffectsHolder.GetComponent<ScoutVision>();
        if (_vision== null)
        {
            return false;
        }

        return true;
    }

    public bool TryToUseBlockaderAbility()
    {
        BlockaderCard _blockader = EffectsHolder.GetComponent<BlockaderCard>();
        if (_blockader.CanBlock)
        {
            _blockader.MarkAsUsed();
            return true;
        }

        return false;
    }

    public bool CheckCanMove()
    {
        return !CardData.IsStunned && CanBeUsed;
    }

    public void ChangeDelivery(bool _status)
    {
        CardData.HasDelivery = _status;
    }
    
    public bool IsCyber()
    {
        return Details.Faction.IsCyber;
    }
}