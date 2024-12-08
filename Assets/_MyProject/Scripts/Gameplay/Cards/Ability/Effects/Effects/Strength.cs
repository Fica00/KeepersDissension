using System.Linq;

public class Strength : AbilityEffect
{
    public override void ActivateForOwner()
    {
        var _keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        _keeper.ChangeDamage(1);
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        RemoveAction();
        OnActivated?.Invoke();
        ManageActiveDisplay(true);
    }

    public override void CancelEffect()
    {
        if (IsActive)
        {
            return;
        }

        var _keeper = GetEffectedCards()[0];
        _keeper.ChangeDamage(-1);
        RemoveEffectedCard(_keeper.UniqueId);
        ManageActiveDisplay(false);
    }
}
