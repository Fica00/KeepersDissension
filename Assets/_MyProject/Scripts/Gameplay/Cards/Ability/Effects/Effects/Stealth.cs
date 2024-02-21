using System.Linq;
using UnityEngine;

public class Stealth : AbilityEffect
{
    [SerializeField] private Sprite sprite;
    private Keeper keeper;

    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
    }

    private void Activate()
    {
        SniperStealth _stun = keeper.EffectsHolder.AddComponent<SniperStealth>();
        _stun.IsBaseCardsEffect = false;
        _stun.Setup(true,sprite);
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (keeper == null)
        {
            return;
        }

        SniperStealth _stealth = keeper.EffectsHolder.GetComponent<SniperStealth>();

        if (_stealth==null)
        {
            return;
        }
        
        Destroy(_stealth);
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
