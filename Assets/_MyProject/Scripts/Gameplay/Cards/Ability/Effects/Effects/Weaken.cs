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
        guardian.Stats.Damage=2;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (guardian==null)
        {
            return;
        }

        guardian.Stats.Damage = 3;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
