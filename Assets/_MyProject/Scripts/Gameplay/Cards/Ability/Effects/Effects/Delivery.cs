using System.Linq;
using UnityEngine;

public class Delivery : AbilityEffect
{
    [SerializeField] private Sprite sprite;
    private Keeper keeper;

    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
    }

    private void Activate()
    {
        MageCardDelivery _delivery = keeper.EffectsHolder.AddComponent<MageCardDelivery>();
        _delivery.IsBaseCardsEffect = false;
        _delivery.Setup(true,sprite);
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }

        MageCardDelivery _delivery = keeper.EffectsHolder.GetComponent<MageCardDelivery>();
        if (_delivery==null)
        {
            return;
        }
        
        Destroy(_delivery);
    }
}
