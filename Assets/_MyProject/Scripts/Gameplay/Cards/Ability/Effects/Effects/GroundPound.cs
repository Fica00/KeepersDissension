using System.Collections.Generic;
using System.Linq;

public class GroundPound : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        Keeper _keeper = GameplayManager.Instance.GetMyKeeper();
        TablePlaceHandler _keeperPlace = _keeper.GetTablePlace();
        int _attackingPlaceId = _keeperPlace.Id;
        List<Card> _availablePlaces = GameplayManager.Instance.TableHandler.GetAttackableCards(_attackingPlaceId,
            CardMovementType.EightDirections);

        foreach (var _cardOnPlace in _availablePlaces.ToList())
        {
            GameplayManager.Instance.DamageCardByAbility(_cardOnPlace.UniqueId, _keeper.Damage, _ => { GameplayManager.Instance.HideCardActions();});
        }        

        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
    }
}