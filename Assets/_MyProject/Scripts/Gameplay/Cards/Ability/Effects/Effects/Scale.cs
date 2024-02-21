using System.Linq;

public class Scale : AbilityEffect
{
    private Keeper keeper;
    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
    }

    private void Activate()
    {
        keeper.EffectsHolder.AddComponent<ScalerScale>();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (keeper == null)
        {
            return;
        }

        ScalerScale _scale = keeper.EffectsHolder.GetComponent<ScalerScale>();
        if (_scale==null)
        {
            return;
        }
        
        Destroy(_scale);
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
