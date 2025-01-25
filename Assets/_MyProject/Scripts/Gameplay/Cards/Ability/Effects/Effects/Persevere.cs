using System.Collections;
using UnityEngine;

public class Persevere : AbilityEffect
{
    private int attackChange=1;

    protected override void ActivateForOwner()
    {
        Card _keeper = GameplayManager.Instance.GetMyKeeper();
        GameplayManager.OnStartedResponseAction += CheckWithDelay;
        AddEffectedCard(_keeper.UniqueId);
        _keeper.UpdatedHealth += CheckKeeper;
        CheckKeeper(_keeper);
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void CheckWithDelay()
    {
        StartCoroutine(CheckRoutine());
        IEnumerator CheckRoutine()
        {
            yield return new WaitForSeconds(1);
            CheckKeeper();
        }
    }

    private void CheckKeeper()
    {
        Card _keeper = GetEffectedCards()[0];
        CheckKeeper(_keeper);
    }

    private void CheckKeeper(Card _keeper)
    {
        if (!GameplayManager.Instance.IsMyTurn())
        {
            Debug.Log("It is not my turn");
            if (!GameplayManager.Instance.IsMyResponseAction())
            {
                Debug.Log("It is not my response actions");
                return;
            }
        }
        
        Debug.Log($"Is active: {IsApplied}, health: {_keeper.Health}");
        if (IsApplied&&_keeper.Health>2)
        {
            SetIsApplied(false);
            _keeper.ChangeDamage(-attackChange);
            ManageActiveDisplay(false);
        }
        else if (!IsApplied&& _keeper.Health<=2)
        {
            SetIsApplied(true);
            _keeper.ChangeDamage(attackChange);
            ManageActiveDisplay(true);
        }
    }

    protected override void CancelEffect()
    {
        GameplayManager.OnStartedResponseAction -= CheckWithDelay;
        Card _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        _keeper.UpdatedHealth -= CheckKeeper;
        if (!IsApplied)
        {
            return;
        }
        
        SetIsApplied(false);
        _keeper.ChangeDamage(-attackChange);
        ManageActiveDisplay(false);
    }
}
