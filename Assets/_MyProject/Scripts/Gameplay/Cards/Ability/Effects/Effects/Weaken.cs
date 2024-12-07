using System.Linq;

public class Weaken : AbilityEffect
{
    private Guardian guardian;
    
    public override void ActivateForOwner()
    {
        guardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => !_guardian.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        guardian = FindObjectsOfType<Guardian>().ToList().Find(_guardian => _guardian.My);
        Activate();
    }

    private void Activate()
    {
        guardian.SetDamage(2);
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (guardian==null)
        {
            return;
        }

        guardian.SetDamage(3);
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
