using UnityEngine;

public class Minefield : AbilityEffect
{
    [SerializeField] private Sprite sprite;
    
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        BomberMinefield _bomberMinefield = _keeper.EffectsHolder.AddComponent<BomberMinefield>();
        _bomberMinefield.IsBaseCardsEffect = false;
        _bomberMinefield.Setup(true,null);
        _bomberMinefield.UseForFree(Finish);

        void Finish()
        {
            OnActivated?.Invoke();
            ManageActiveDisplay(true);
            RemoveAction();
        }
    }

    protected override void CancelEffect()
    {
        Card _keeper = GetEffectedCards()[0];
        BomberMinefield _bomberMinefield = _keeper.EffectsHolder.GetComponent<BomberMinefield>();
        ManageActiveDisplay(false);
        SetIsActive(false);
        
        if (_bomberMinefield==null)
        {
            return;
        }
        
        Destroy(_bomberMinefield);
    }
}
