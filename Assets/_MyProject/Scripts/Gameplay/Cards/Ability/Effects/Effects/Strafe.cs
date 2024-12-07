using System.Linq;

public class Strafe : AbilityEffect
{
    private Keeper keeper;
    
    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void Activate()
    {
        keeper.ChangeMovementType(CardMovementType.EightDirections);
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        
        keeper.ChangeMovementType(CardMovementType.FourDirections);
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
