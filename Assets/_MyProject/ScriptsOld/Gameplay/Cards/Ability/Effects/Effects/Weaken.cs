public class Weaken : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _guardian = GameplayManager.Instance.GetOpponentGuardian();
        AddEffectedCard(_guardian.UniqueId);
        SetIsActive(true);
        _guardian.SetDamage(2);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    protected override void CancelEffect()
    {
        if (!IsActive)
        {
            return;
        }

        var _guardian = GetEffectedCards()[0];
        RemoveEffectedCard(_guardian.UniqueId);
        _guardian.SetDamage(3);
        ManageActiveDisplay(false);
        SetIsActive(false);
    }
}
