using UnityEngine;

public class Minefield : AbilityEffect
{
    [SerializeField] private Sprite sprite;

    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        var _player = GameplayManager.Instance.MyPlayer;
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        SetIsActive(true);
        BomberMinefield _bomberMinefield = _keeper.EffectsHolder.AddComponent<BomberMinefield>();
        _bomberMinefield.IsBaseCardsEffect = false;
        _bomberMinefield.Setup(false,sprite);
        _bomberMinefield.OnActivated += Finish;
        // _bomberMinefield.UseAbilityFree();
        _player.OnEndedTurn += RemoveEffect;
    }

    private void Finish()
    {
        Card _keeper = GetEffectedCards()[0];
        BomberMinefield _bomberMinefield = _keeper.EffectsHolder.GetComponent<BomberMinefield>();
        _bomberMinefield.OnActivated -= Finish;
        RemoveAction();
        OnActivated?.Invoke();
    }

    private void RemoveEffect()
    {
        if (!IsActive)
        {
            return;
        }
        
        SetIsActive(false);
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= RemoveEffect;
        Card _keeper = GetEffectedCards()[0];
        BomberMinefield _effect = _keeper.EffectsHolder.GetComponent<BomberMinefield>();
        if (_effect!=null)
        {
            Destroy(_effect);
        }
    }
}
