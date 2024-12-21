public class Snipe : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        AddEffectedCard(_keeper.UniqueId);
        MoveToActivationField();
        GameplayManager.Instance.MyPlayer.OnEndedTurn += Deactivate;
        SetIsActive(true);
        SetStartingDamage(_keeper.Damage);
        SetStartingRange(_keeper.Range);
        _keeper.SetRange(3);
        _keeper.SetDamage(1);
        ManageActiveDisplay(true);
        ForceKeeperAttack(_keeper);
    }
    
    private void ForceKeeperAttack(Card _keeper)
    {
        GameplayState _state = GameplayManager.Instance.GameState();
        GameplayManager.Instance.SetGameState(GameplayState.UsingSpecialAbility);

        TablePlaceHandler _keeperTablePlace = _keeper.GetTablePlace();
        GameplayManager.Instance.SelectPlaceForSpecialAbility(_keeperTablePlace.Id, 3, PlaceLookFor.Both, _keeper.MovementType, false, LookForCardOwner
        .Both, Attack, false);

        void Attack(int _placeId)
        {
            if (_placeId == -1)
            {
                DialogsManager.Instance.ShowOkDialog("There are no available spaces");
                Finish();
                return;
            }

            TablePlaceHandler _place = GameplayManager.Instance.TableHandler.GetPlace(_placeId);
            if (!_place.IsOccupied)
            {
                Finish();
                return;
            }

            Card _card = _place.GetCard();
            if (_card is Wall && GameplayManager.Instance.TableHandler.DistanceBetweenPlaces(_keeper.GetTablePlace(),_place)>1)
            {
                Finish();
                return;
            }

            CardAction _attackAction = new CardAction
            {
                StartingPlaceId = _keeperTablePlace.Id,
                FirstCardId = _keeper.UniqueId,
                FinishingPlaceId = _placeId,
                SecondCardId = _place.GetCard().UniqueId,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = true,
                Damage = _keeper.Damage,
                CanCounter = true,
                GiveLoot = false
            };

            GameplayManager.Instance.ExecuteCardAction(_attackAction);
            Finish();
        }

        void Finish()
        {
            Deactivate();
            GameplayManager.Instance.SetGameState( _state);
            RemoveAction();
            OnActivated?.Invoke();
        }
    }

    private void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        GameplayManager.Instance.MyPlayer.OnEndedTurn -= Deactivate;
        SetIsActive(false);
        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        _keeper.SetRange(StartingRange);
        _keeper.SetDamage(StartingDamage);
        ManageActiveDisplay(false);
    }

    protected override void CancelEffect()
    {
        Deactivate();
    }
}
