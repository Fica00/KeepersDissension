using System.Linq;
using UnityEngine;

public class Stun : AbilityEffect
{
    [SerializeField] private Sprite sprite;
    private Keeper keeper;

    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void Activate()
    {
        MageCardStun _stun = keeper.EffectsHolder.AddComponent<MageCardStun>();
        _stun.IsBaseCardsEffect = false;
        _stun.Setup(true,sprite);
    }

    public override void CancelEffect()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        if (keeper==null)
        {
            return;
        }

        MageCardStun _stun = keeper.EffectsHolder.GetComponent<MageCardStun>();
        if (_stun==null)
        {
            return;
        }
        
        Destroy(_stun);
    }
}
