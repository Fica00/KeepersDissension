using System.Linq;

public class Snipe : AbilityEffect
{
    private bool isActive;
    private int startingRange;
    private float startingDamage;
    private Keeper keeper;

    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        MoveToActivationField();
        Activate();
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void Activate()
    {
        if (isActive)
        {
            return;
        }

        GameplayManager.Instance.MyPlayer.OnEndedTurn += Deactivate;
        isActive = true;
        startingRange = keeper.Stats.Range;
        startingDamage = keeper.Stats.Damage;
        keeper.Stats.Range = 3;
        keeper.Stats.Damage = 1;
        ForceKeeperAttack();
    }

    private void ForceKeeperAttack()
    {
        GameplayState _state = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;

        TablePlaceHandler _keeperTablePlace = keeper.GetTablePlace();
        GameplayManager.Instance.SelectPlaceForSpecialAbility(_keeperTablePlace.Id, 3, PlaceLookFor.Both, keeper.MovementType, false, LookForCardOwner
        .Both, Attack, false);

        void Attack(int _placeId)
        {
            if (_placeId == -1)
            {
                UIManager.Instance.ShowOkDialog("There are no available spaces");
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
                FirstCardId = keeper.Details.Id,
                FinishingPlaceId = _placeId,
                SecondCardId = _place.GetCard().Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
                CanTransferLoot = true,
                Damage = (int)keeper.Stats.Damage,
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

    private void Deactivate()
    {
        if (!isActive)
        {
            return;
        }

        GameplayManager.Instance.MyPlayer.OnEndedTurn -= Deactivate;
        isActive = false;
        keeper.Stats.Range = startingRange;
        keeper.Stats.Damage = startingDamage;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    public override void CancelEffect()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
