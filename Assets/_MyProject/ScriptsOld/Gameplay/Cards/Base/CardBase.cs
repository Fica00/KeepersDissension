using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CardBase : MonoBehaviour
{
    public static Action<CardBase> OnGotDestroyed;
    protected static Action<CardBase> OnPositionedOnTable;
    protected static Action<CardBase> OnPositionedInHand;
    
    [field: SerializeField] public GameObject EffectsHolder { get; private set; }
    public List<CardSpecialAbility> SpecialAbilities => EffectsHolder.GetComponents<CardSpecialAbility>().ToList();
    public CardDisplayBase Display;
    public bool AllowCardEffectOnDeath;

    protected Transform Parent;

    public void PositionInHand(bool _rotateHorizontally=false)
    {
        SetPlace(CardPlace.Hand);
        RotateCard(_rotateHorizontally);
        OnPositionedInHand?.Invoke(this);
    }

    public void RotateCard(bool _rotateHorizontally=false)
    {
        transform.rotation = Quaternion.Euler(_rotateHorizontally ? new Vector3(0, 0, 90) : new Vector3(0, 0, 0));
    }
    
    public void PositionOnTable(TablePlaceHandler _tablePosition)
    {
        SetPlace(CardPlace.Table);
        MoveToPosition(_tablePosition);
        OnPositionedOnTable?.Invoke(this);
    }

    public void ReturnFromHand()
    {
        SetPlace(CardPlace.Deck);
        transform.SetParent(Parent);
        ResetPosition();
        Vector3 _desiredRotation = GetIsMy() ? Vector3.one : Vector3.back;
        transform.rotation = Quaternion.Euler(_desiredRotation);
    }

    public void MoveToPosition(TablePlaceHandler _tablePosition)
    {
        RectTransform _rect = GetComponent<RectTransform>();
        transform.SetParent(_tablePosition.transform);
        _rect.pivot = new Vector2(0.5f, 0.5f);
        transform.DOLocalMove(Vector3.zero, 1f);
        transform.DOScale(ScaleOnTable, 1f);
        SetRotation();
    }
    
    private Vector3 ScaleOnTable => this is AbilityCard ? new Vector3(0.7f, 0.75f, 1) : new Vector3(0.8f, 0.8f, 1);

    public void RotateToBeVertical()
    {
        Vector3 _targetRotation = GetIsMy() ? new Vector3(0, 0, -90) : new Vector3(0, 0, 90);
        transform.DORotate(_targetRotation, 1f);
    }

    protected void SetRotation()
    {
        Vector3 _targetRotation = new Vector3(0, 0, 90);
        if (this is AbilityCard)
        {
            _targetRotation = Vector3.zero;
        }
        transform.DORotate(_targetRotation, 1f);
    }
    
    protected void ResetPosition()
    {
        var _transform = transform;
        _transform.localScale=Vector3.one;
        _transform.localPosition=Vector3.zero;
    }

    public virtual bool IsWarrior()
    {
        throw new Exception("Is warrior must be implemented");
    }
    
    public virtual bool IsLifeForce()
    {
        throw new Exception("IsLifeForce must be implemented");
    }

    public virtual bool IsAttackable()
    {
        throw new Exception("Is attack attackable be implemented");
    }

    public TablePlaceHandler GetTablePlace()
    {
        return GetComponentInParent<TablePlaceHandler>();
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

    public virtual bool GetIsMy()
    {
        throw new Exception();
    }

    private void SetPlace(CardPlace _place)
    {
        if (this is Card _card)
        {
            GameplayManager.Instance.SetCardPlace(_card.UniqueId, _place);
        }
        else if (this is AbilityCard _ability)
        {
            GameplayManager.Instance.SetCardPlace(_ability.UniqueId, _place);
        }
    }
}
