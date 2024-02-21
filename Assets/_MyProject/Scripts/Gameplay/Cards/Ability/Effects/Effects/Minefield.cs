using System.Linq;
using UnityEngine;

public class Minefield : AbilityEffect
{
    [SerializeField] private Sprite sprite;
    private Keeper keeper;
    private GameplayPlayer player;

    public override void ActivateForOwner()
    {
        MoveToActivationField();
        player = GameplayManager.Instance.MyPlayer;
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        Activate();
        RemoveAction();
    }

    public override void ActivateForOther()
    {
        player = GameplayManager.Instance.OpponentPlayer;
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => !_keeper.My);
        Activate();
    }

    private void Activate()
    {
        BomberMinefield _bomberMinefield = keeper.EffectsHolder.AddComponent<BomberMinefield>();
        _bomberMinefield.IsBaseCardsEffect = false;
        _bomberMinefield.Setup(false,sprite);
        _bomberMinefield.OnActivated += Finish;
        _bomberMinefield.UseAbility();
        
        player.OnEndedTurn += RemoveEffect;
    }

    private void Finish()
    {
        BomberMinefield _bomberMinefield = keeper.EffectsHolder.AddComponent<BomberMinefield>();
        _bomberMinefield.OnActivated -= Finish;
        OnActivated?.Invoke();
    }

    private void RemoveEffect()
    {
        player.OnEndedTurn -= RemoveEffect;
        BomberMinefield _effect = keeper.EffectsHolder.GetComponent<BomberMinefield>();
        if (_effect!=null)
        {
            Destroy(_effect);
        }
    }
}
