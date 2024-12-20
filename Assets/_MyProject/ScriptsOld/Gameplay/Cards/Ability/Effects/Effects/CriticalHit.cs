public class CriticalHit : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        var _keeper = GameplayManager.Instance.GetMyKeeper();
        GameplayManager.Instance.MyPlayer.OnEndedTurn += Deactivate;
        AddEffectedCard(_keeper.UniqueId);
        Apply(_keeper);
        ManageActiveDisplay(true);
    }

    private void Apply(Card _keeper)
    {
        SetIsActive(true);
        SetStartingRange(_keeper.Range);
        SetStartingDamage(_keeper.Damage);
        _keeper.SetRange(1);
        _keeper.SetDamage(3);
        ForceKeeperAttack(_keeper);
    }

    private void ForceKeeperAttack(Card _keeper)
    {
        GameplayState _state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;

        TablePlaceHandler _keeperTablePlace = _keeper.GetTablePlace();
        GameplayManager.Instance.SelectPlaceForSpecialAbility(_keeperTablePlace.Id,1,PlaceLookFor.Both, _keeper.MovementType,false,LookForCardOwner.Both,Attack,false);

        void Attack(int _placeId)
        {
            if (_placeId==-1)
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

            CardAction _attackAction = new CardAction
            {
                StartingPlaceId = _keeperTablePlace.Id,
                FirstCardId = _keeper.Details.Id,
                FinishingPlaceId = _placeId,
                SecondCardId = _place.GetCard().Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
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
            GameplayManager.Instance.GameState = _state;
            RemoveAction();
            OnActivated?.Invoke();
        }
    }

    protected override void CancelEffect()
    {
        ManageActiveDisplay(false);
    }

    private void Deactivate()
    {
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= Deactivate;
        SetIsActive(false);
        var _keeper = GetEffectedCards()[0];
        RemoveEffectedCard(_keeper.UniqueId);
        _keeper.SetRange(StartingRange);
        _keeper.SetDamage(StartingDamage);
        ManageActiveDisplay(false);
    }

}
