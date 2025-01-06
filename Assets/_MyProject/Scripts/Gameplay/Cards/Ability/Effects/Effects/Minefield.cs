using UnityEngine;

public class Minefield : AbilityEffect
{
    [SerializeField] private Sprite sprite;
    private BomberMinefield bomberMinefield;
    
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        
        if (bomberMinefield==default)
        {
            bomberMinefield = _keeper.EffectsHolder.AddComponent<BomberMinefield>();
            bomberMinefield.IsBaseCardsEffect = false;
            bomberMinefield.Setup(false,null);
        }

        if (bomberMinefield.CanUse())
        {
            bomberMinefield.UseForFree(Finish);
        }
        else
        {
            Finish();
        }

        void Finish()
        {
            MoveToActivationField();
            OnActivated?.Invoke();
            RemoveAction();
        }
    }

    protected override void CancelEffect()
    {
        if (bomberMinefield==null)
        {
            return;
        }
        
        Destroy(bomberMinefield);
        ClearEffectedCards();
    }
}
