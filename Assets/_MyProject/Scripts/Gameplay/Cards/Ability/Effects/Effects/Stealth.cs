using UnityEngine;

public class Stealth : AbilityEffect
{
    [SerializeField] private Sprite sprite;

    public override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        
        SniperStealth _stun = _keeper.EffectsHolder.AddComponent<SniperStealth>();
        _stun.IsBaseCardsEffect = false;
        _stun.Setup(true,sprite); 
        ManageActiveDisplay(false);
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        if (_keeper == null)
        {
            return;
        }

        SniperStealth _stealth = _keeper.EffectsHolder.GetComponent<SniperStealth>();

        if (_stealth==null)
        {
            return;
        }
        
        Destroy(_stealth);
        ManageActiveDisplay(false);
    }
}
