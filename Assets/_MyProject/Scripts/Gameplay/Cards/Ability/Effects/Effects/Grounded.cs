public class Grounded : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        ManageActiveDisplay(true);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public bool IsCardEffected(string _uniqueId)
    {
        return GetEffectedCards()[0].UniqueId == _uniqueId;
    }

    public bool CanApplyEffect()
    {
        var _effectedCards = GetEffectedCards();
        if (_effectedCards == null || _effectedCards.Count==0)
        {
            return true;
        }

        return false;
    }

    public void ApplyEffect(string _uniqueId)
    {
        AddEffectedCard(_uniqueId);
    }

    public void EndEffect()
    {
        CancelEffect();
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
        ClearEffectedCards();
        SetIsActive(false);
        RoomUpdater.Instance.ForceUpdate();
    }
}