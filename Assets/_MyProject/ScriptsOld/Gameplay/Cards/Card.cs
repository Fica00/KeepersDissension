using System;
using UnityEngine;

public class Card : CardBase
{
    public Action UpdatedHealth;

    public CardDetails Details;
    private CardData cardData;
    public CardData Data;

    public int Health => cardData.Stats.Health;
    public int Range => cardData.Stats.Range;
    public int Damage => cardData.Stats.Damage;
    public bool IsVoid => cardData.IsVoid;
    public bool CanMoveOnWall => cardData.CanMoveOnWall;
    public int Speed => cardData.Stats.Speed;
    public bool CanFlyToDodgeAttack => cardData.CanFlyToDodgeAttack;
    public bool CanMove => cardData.CanMove;
    public bool CanBeUsed => cardData.CanBeUsed;
    public bool My => cardData.Owner == FirebaseManager.Instance.PlayerId;
    public CardMovementType MovementType => cardData.MovementType;
    public bool HasDied => cardData.HasDied;
    public string UniqueId => cardData.UniqueId;
    public int PercentageOfHealthToRecover => cardData.PercentageOfHealthToRecover;
    

    public void Setup(string _owner)
    {
        cardData = new CardData
        {
            Owner = _owner,
            UniqueId = Guid.NewGuid().ToString(),
            CardId = Details.Id,
            IsVoid = false,
            CanFlyToDodgeAttack = false,
            CanMoveOnWall = false,
            Stats = new CardStats { 
                Damage = Details.Stats.Damage,
                Health = Details.Stats.Health,
                Range = Details.Stats.Range,
                Speed =  Details.Stats.Speed,
                MaxHealth = Details.Stats.MaxHealth}
        };

        Display.Setup(this);
        Setup();
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
        float _amount = Details.Stats.Health - cardData.Stats.Health;
        ChangeHealth((int)_amount);
        UpdatedHealth?.Invoke();
    }
    
    public void SetHealth(int _amount)
    {
        int _alteredAmount = Math.Clamp(_amount, 0, Details.Stats.Health);
        cardData.Stats.Health = _alteredAmount;
        UpdatedHealth?.Invoke();
    }

    public void ChangeHealth(int _amount)
    {
        int _newHealth = Math.Clamp(cardData.Stats.Health + _amount, 0, Details.Stats.Health);
        cardData.Stats.Health = _newHealth;
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
        cardData.Stats.Damage += _amount;
    }

    public void SetDamage(int _amount)
    {
        cardData.Stats.Damage = _amount;
    }

    public void ChangeRange(int _amount)
    {
        cardData.Stats.Range += _amount;
    }
    
    public void SetRange(int _amount)
    {
        cardData.Stats.Range = _amount;
    }

    public void ChangeSpeed(int _amount)
    {
        cardData.Stats.Speed += _amount;
    }

    public void SetSpeed(int _amount)
    {
        cardData.Stats.Speed = _amount;
    }

    public void SetMaxHealth(int _amount)
    {
        cardData.Stats.MaxHealth = _amount;
    }

    public void SetCanMoveOnWall(bool _status)
    {
        cardData.CanMoveOnWall = _status;
    }

    public void SetCanFlyToDodgeAttack(bool _status)
    {
        cardData.CanFlyToDodgeAttack = _status;
    }

    public void SetIsVoid(bool _status)
    {
        cardData.IsVoid = _status;
    }

    public void SetCanMove(bool _status)
    {
        cardData.CanMove = _status;
    }

    public void SetCanBeUsed(bool _status)
    {
        cardData.CanBeUsed = _status;
    }
    
    public void ChangeOwner()
    {
        cardData.Owner = GetIsMy() ? FirebaseManager.Instance.OpponentId : FirebaseManager.Instance.PlayerId;
        SetRotation();
    }

    public void SetPercentageOfHealthToRecover(int _amount)
    {
        cardData.PercentageOfHealthToRecover = _amount;
    }

    public void ChangePercentageOfHealthToRecover(int _amount)
    {
        cardData.PercentageOfHealthToRecover += _amount;
    }

    public void ChangeMovementType(CardMovementType _movementType)
    {
        cardData.MovementType = _movementType;
    }

    public void SetHasDied(bool _status)
    {
        cardData.HasDied = _status;
    }
    
    public void CopyStats(Card _card)
    {
        
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
    
}