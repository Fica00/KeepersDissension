using System;
using System.Collections;
using UnityEngine;

public class AbilityEffect : MonoBehaviour
{
    public Action OnActivated;
    [SerializeField] protected int cooldown;
    public AbilityCard AbilityCard => GetComponentInParent<AbilityCard>();
    public int Cooldown => cooldown;

    public virtual void ActivateForOther()
    {
        
    }

    public virtual void ActivateForOwner()
    {
        
    }

    public virtual void CancelEffect()
    {
        Debug.Log("Should remove effect",gameObject);
    }

    protected void ReturnToHand()
    {
        StartCoroutine(ReturnToHandRoutine());

        IEnumerator ReturnToHandRoutine()
        {
            yield return new WaitForSeconds(0.1f);
            GameplayManager.Instance.PlaceAbilityOnTable(AbilityCard.Details.Id);
        }
    }

    protected void MoveToActivationField()
    {
        GameplayPlayer _player =
            AbilityCard.My ? GameplayManager.Instance.MyPlayer : GameplayManager.Instance.OpponentPlayer;

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

    public void SetCooldown(int _amount)
    {
        cooldown = _amount;
    }

    protected void RemoveAction()
    {
        // OnActivated?.Invoke();
        if (!AbilityCard.My)
        {
            return;
        }

        if (AbilityCard.Details.Type!=AbilityCardType.Passive)
        {
            GameplayManager.Instance.MyPlayer.Actions--;
        }
    }
}
