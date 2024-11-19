using System;
using UnityEngine;

public class Card : CardBase
{
    public Action UpdatedCanFlyToDodge;
    
    public CardDetails Details;
    [HideInInspector] public CardStats Stats;
    public bool CanMoveOnWall;
    private bool canFlyToDodgeAttack;
    [HideInInspector]public int Speed;
    public bool IsVoid;

    public bool CanFlyToDodgeAttack
    {
        get => canFlyToDodgeAttack;
        set
        {
            canFlyToDodgeAttack = value;
            UpdatedCanFlyToDodge?.Invoke();
        }
    }
    
    public override void Setup(bool _isMy)
    {
        IsMy = _isMy;

        Stats = new CardStats()
        {
            Damage = Details.Stats.Damage,
            Health = Details.Stats.Health,
            Range = Details.Stats.Range,
        };
        
        Display.Setup(this);
        Setup();
    }

    public override void SetParent(Transform _parent)
    {
        Parent = _parent;
        transform.SetParent(_parent);
        ResetPosition();
    }

    protected virtual void Setup()
    {
        //if a card like Keeper needs to initialize its own values
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

    public override void Heal(int _amount)
    {
        Stats.Health += _amount;
        if (Stats.Health>Details.Stats.Health)
        {
            Stats.Health = Details.Stats.Health;
        }
    }

    public override void Heal()
    {
        float _amount = Details.Stats.Health - Stats.Health;
        Heal((int)_amount);
    }

    public void Hide()
    {
        Display.gameObject.SetActive(false);
    }

    public void UnHide()
    {
        Display.gameObject.SetActive(true);
    }
}
