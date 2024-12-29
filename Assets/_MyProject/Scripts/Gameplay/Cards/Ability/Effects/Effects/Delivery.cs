using UnityEngine;

public class Delivery : AbilityEffect
{
    [SerializeField] private Sprite sprite;
    
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        MageCardDelivery _delivery = _keeper.EffectsHolder.AddComponent<MageCardDelivery>();
        _delivery.IsBaseCardsEffect = false;
        _delivery.Setup(true,sprite);
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
        RemoveAction();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        MageCardDelivery _delivery = _keeper.EffectsHolder.GetComponent<MageCardDelivery>();
        if (_delivery==null)
        {
            return;
        }
        
        Destroy(_delivery);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}