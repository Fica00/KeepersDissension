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

        bool _didGetResponseAction = false;

        foreach (var _cardOnPlace in _availablePlaces.ToList())
        {
            bool _gaveResponse = GameplayManager.Instance.DamageCardByAbility(_cardOnPlace.UniqueId, _keeper.Damage, _ => { GameplayManager.Instance
                    .HideCardActions();}, true, _keeper.UniqueId, true);
            if (_gaveResponse)
            {
                _didGetResponseAction = true;
            }
        }

        MoveToActivationField();
        OnActivated?.Invoke();
        RemoveAction();
        
        if (_didGetResponseAction)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }
}