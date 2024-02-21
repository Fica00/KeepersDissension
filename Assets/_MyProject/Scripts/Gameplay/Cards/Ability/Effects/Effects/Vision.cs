using System.Linq;

public class Vision : AbilityEffect
{
    private Keeper keeper;

    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        ScoutVision _vision = keeper.EffectsHolder.AddComponent<ScoutVision>();
        _vision.IsBaseCardsEffect = false;
        _vision.Setup(false,null);
        RemoveAction();
        OnActivated?.Invoke();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        if (keeper==default)
        {
            return;
        }
        
        ScoutVision _vision = keeper.EffectsHolder.GetComponent<ScoutVision>();
        if (_vision==null)
        {
            return;
        }
        
        Destroy(_vision);
        keeper = null;
    }
}