public class Penalty : AbilityEffect
{
   public static bool IsActive;

   private void OnEnable()
   {
      IsActive = false;
   }

   public override void ActivateForOwner()
   {
      if (IsActive)
      {
         RemoveAction();
         MoveToActivationField();
         OnActivated?.Invoke();
         return;
      }
      Activate();
      RemoveAction();
      OnActivated?.Invoke();
      MoveToActivationField();
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
   }

   public override void ActivateForOther()
   {
      if (IsActive)
      {
         return;
      }
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
      GameplayManager.Instance.OpponentPlayer.OnStartedTurn += RemoveEffect;
   }

   private void Activate()
   {
      GameplayManager.OnCardMoved += CheckMove;
      GameplayManager.Instance.MyPlayer.OnStartedTurn += RemoveEffect;
   }

   private void CheckMove(CardBase _cardBase, int _movedFrom, int _movedTo)
   {
      if (_cardBase is not Keeper _keeper)
      {
         return;
      }

      if (_keeper.My)
      {
         return;
      }

      TablePlaceHandler _startingPlace = GameplayManager.Instance.TableHandler.GetPlace(_movedFrom);
      TablePlaceHandler _endingPlace = GameplayManager.Instance.TableHandler.GetPlace(_movedTo);
      int _distance = GameplayManager.Instance.TableHandler.DistanceBetweenPlaces(_startingPlace, _endingPlace);
      if (_distance<=0)
      {
         return;
      }

      CardAction _attackAction = new CardAction
      {
         StartingPlaceId = _movedTo,
         FirstCardId = _keeper.Details.Id,
         FinishingPlaceId = _movedTo,
         SecondCardId = _keeper.Details.Id,
         Type = CardActionType.Attack,
         Cost = 0,
         IsMy = true,
         CanTransferLoot = false,
         Damage = _distance,
         CanCounter = false
      };
      GameplayManager.Instance.ExecuteCardAction(_attackAction);
      RemoveAction();
   }

   private void RemoveEffect()
   {
      AbilityCard.ActiveDisplay.gameObject.SetActive(false);
      IsActive = false;
      try
      {
         GameplayManager.OnCardMoved -= CheckMove;
         GameplayManager.Instance.MyPlayer.OnStartedTurn -= RemoveEffect;
      }
      catch
      {
      }
      
   }

   public override void CancelEffect()
   {
      RemoveEffect();
   }
}
