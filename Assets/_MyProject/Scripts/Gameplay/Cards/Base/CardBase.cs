using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR;

public class CardBase : MonoBehaviour
{
    public static Action<CardBase> OnPositionedOnTable;
    public static Action<CardBase> OnPositionedInHand;
    public static Action<CardBase> OnGotDestroyed;
    public Action UpdatedCanMove;
    
    [field: SerializeField] public GameObject EffectsHolder { get; private set; }
    public CardDisplayBase Display;
    public CardPlace CardPlace { get; private set; }
    public CardMovementType MovementType;
    [HideInInspector] public bool HasDied;
    [HideInInspector] public bool CanBeUsed = true;
    private bool canMove=true;

    protected Transform Parent;
    protected bool IsMy;

    private Vector3 scaleOnTable
    {
        get
        {
            if (this is AbilityCard)
            {
                return new (0.7f, 0.75f, 1);
            }
            else
            {
                return new(0.8f, 0.8f, 1);
            }
        }
    }

    public List<CardSpecialAbility> SpecialAbilities => EffectsHolder.GetComponents<CardSpecialAbility>().ToList();
    [HideInInspector] public List<EffectBase> Effects = new();
    public bool My=>IsMy;

    public bool CanMove
    {
        get => canMove;
        set
        {
            canMove = value;
            UpdatedCanMove?.Invoke();
        }
    }

    public virtual void Setup(bool _isMy)
    {
        throw new Exception("Setup must be implemented");
    }
    
    public void PositionInHand(bool _rotateHorizontaly=false)
    {
        CardPlace = CardPlace.Hand;
        if (_rotateHorizontaly)
        {
            // transform.DORotate(new Vector3(0, 0, 90), 0.5f);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
        else
        {
            // transform.DORotate(new Vector3(0, 0, 0), 0.5f);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }

        OnPositionedInHand?.Invoke(this);
    }
    
    public void PositionOnTable(TablePlaceHandler _tablePosition)
    {
        CardPlace = CardPlace.Table;
        MoveToPosition(_tablePosition);
        OnPositionedOnTable?.Invoke(this);
    }

    public void ChangeOwner()
    {
        IsMy = !IsMy;
        SetRotation();
    }

    public void ReturnFromHand()
    {
        CardPlace = CardPlace.Deck;
        transform.SetParent(Parent);
        ResetPosition();
        Vector3 _desiredRotation = IsMy ? Vector3.one : Vector3.back;
        transform.rotation = Quaternion.Euler(_desiredRotation);
    }

    public void MoveToPosition(TablePlaceHandler _tablePosition)
    {
        RectTransform _rect = GetComponent<RectTransform>();
        transform.SetParent(_tablePosition.transform);
        _rect.pivot = new Vector2(0.5f, 0.5f);
        transform.DOLocalMove(Vector3.zero, 1f);
        transform.DOScale(scaleOnTable, 1f);
        SetRotation();
    }

    public void RotateToBeVertical()
    {
        Vector3 _targetRotation = IsMy ? new Vector3(0, 0, -90) : new Vector3(0, 0, 90);
        transform.DORotate(_targetRotation, 1f);
    }

    private void SetRotation()
    {
        // Vector3 _targetRotation = IsMy ? new Vector3(0, 0, 90) : new Vector3(0, 0, 270);
        Vector3 _targetRotation = new Vector3(0, 0, 90);
        if (this is AbilityCard)
        {
            if (((AbilityCard)this).IsVetoed)
            {
                // _targetRotation = IsMy ? Vector3.zero : new Vector3(0, 0, 90);
                _targetRotation = Vector3.zero;
            }
            else
            {
                // _targetRotation = IsMy ? Vector3.zero : new Vector3(0, 0, 180);
                _targetRotation = Vector3.zero;
            }
        }
        transform.DORotate(_targetRotation, 1f);
    }

    public virtual void SetParent(Transform _parent)
    {
        throw new Exception("Set parent must be implemented");
    }
    
    protected void ResetPosition()
    {
        var _transform = transform;
        _transform.localScale=Vector3.one;
        _transform.localPosition=Vector3.zero;
    }

    public virtual bool IsWarrior()
    {
        //warriors are minions,keepers and guardians
        throw new Exception("Is warrior must be implemented");
    }

    public virtual bool IsAttackable()
    {
        throw new Exception("Is attack attackable be implemented");
    }

    public TablePlaceHandler GetTablePlace()
    {
        return GetComponentInParent<TablePlaceHandler>();
    }

    public virtual void Heal(int _amount)
    {
        
    }

    public virtual void Heal()
    {
        
    }

    public void Destroy()
    {
        OnGotDestroyed?.Invoke(this);
        Card _card = GetComponent<Card>();
        if (_card == null)
        {
            return;
        }

        if (name.ToLower().Contains("wall"))
        {
            if (_card.Details.Faction.IsSnow)
            {
                GameplayManager.Instance.PlayAudioOnBoth("IceWallBraking",_card);
            }
        }
    }
}
