using UnityEngine;

public class BountyHounter : AbilityEffect
{
    [SerializeField] private int amount;
    private bool isActive;
    
    public override void ActivateForOwner()
    {
        RemoveAction();

        if (PhotonManager.IsMasterClient)
        {
            GameplayManager.Instance.LootChanges[0] += amount;
        }
        else
        {
            GameplayManager.Instance.LootChanges[1] += amount;
        }
        isActive = true;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        OnActivated?.Invoke();
    }
    
    public override void ActivateForOther()
    {
        if (PhotonManager.IsMasterClient)
        {
            GameplayManager.Instance.LootChanges[1] += amount;
        }
        else
        {
            GameplayManager.Instance.LootChanges[0] += amount;
        }
        isActive = true;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (!isActive)
        {
            return;
        }

        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        isActive = false;
        if (PhotonManager.IsMasterClient)
        {
            GameplayManager.Instance.LootChanges[0] -= amount;
        }
        else
        {
            GameplayManager.Instance.LootChanges[1] -= amount;
        }
    }
}