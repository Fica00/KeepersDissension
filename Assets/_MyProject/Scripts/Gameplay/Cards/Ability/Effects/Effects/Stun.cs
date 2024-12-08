using UnityEngine;

public class Stun : AbilityEffect
{
    [SerializeField] private Sprite sprite;

    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            return;
        }
        SetIsActive(true);
        var keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(keeper.UniqueId);
        MageCardStun _stun = keeper.EffectsHolder.AddComponent<MageCardStun>();
        _stun.IsBaseCardsEffect = false;
        _stun.Setup(true,sprite);
        RemoveAction();
        SetIsActive(true);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }
        ManageActiveDisplay(false);
        var keeper = GetEffectedCards()[0];
        RemoveEffectedCard(keeper.UniqueId);
        SetIsActive(false);
        
        MageCardStun _stun = keeper.EffectsHolder.GetComponent<MageCardStun>();
        if (_stun==null)
        {
            return;
        }
        
        Destroy(_stun);
    }
}
