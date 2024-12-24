using UnityEngine;

public class Stealth : AbilityEffect
{
    [SerializeField] private Sprite sprite;

    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SniperStealth _stealth = _keeper.EffectsHolder.AddComponent<SniperStealth>();
        _stealth.IsBaseCardsEffect = false;
        _stealth.Setup(true,sprite); 
        ManageActiveDisplay(false);
        RemoveAction();
        OnActivated?.Invoke();
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        SniperStealth _stealth = _keeper.EffectsHolder.GetComponent<SniperStealth>();
        ManageActiveDisplay(false);

        if (_stealth==null)
        {
            return;
        }
        
        Destroy(_stealth);
    }
}
