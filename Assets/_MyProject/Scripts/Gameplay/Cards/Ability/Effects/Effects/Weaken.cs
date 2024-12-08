public class Weaken : AbilityEffect
{
    
    public override void ActivateForOwner()
    {
        var guardian = GameplayManager.Instance.GetOpponentGuardian();
        AddEffectedCard(guardian.UniqueId);
        SetIsActive(true);
        guardian.SetDamage(2);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    public override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        var guardian = GetEffectedCards()[0];
        RemoveEffectedCard(guardian.UniqueId);
        guardian.SetDamage(3);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
