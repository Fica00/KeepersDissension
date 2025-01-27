using UnityEngine;

public class Stun : AbilityEffect
{
    [SerializeField] private Sprite sprite;

    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        MageCardStun _stun = _keeper.EffectsHolder.AddComponent<MageCardStun>();
        _stun.IsBaseCardsEffect = false;
        _stun.Setup(true,sprite);
        SetIsActive(true);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        SetIsActive(false);
        MageCardStun _stun = _keeper.EffectsHolder.GetComponent<MageCardStun>();
        if (_stun==null)
        {
            return;
        }
        
        Destroy(_stun);
    }
}
