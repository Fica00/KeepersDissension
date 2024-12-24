using UnityEngine;

public class Strength : AbilityEffect
{
    [SerializeField] private int amount;
    
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        _keeper.ChangeDamage(amount);
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    protected override void CancelEffect()
    {
        var _keeper = GetEffectedCards()[0];
        SetIsActive(false);
        _keeper.ChangeDamage(-amount);
        RemoveEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(false);
    }
}
