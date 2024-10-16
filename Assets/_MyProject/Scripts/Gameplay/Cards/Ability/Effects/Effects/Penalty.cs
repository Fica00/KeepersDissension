using UnityEngine;

public class Penalty : AbilityEffect
{
   public static bool IsActive;

   private void Awake()
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

      IsActive = true;
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
      GameplayManager.Instance.OpponentPlayer.OnStartedTurn += RemoveEffect;
   }

   private void Activate()
   {
      IsActive = true;
      GameplayManager.OnCardMoved += CheckMove;
      GameplayManager.OnSwitchedPlace += CheckCards;
      GameplayManager.Instance.MyPlayer.OnStartedTurn += RemoveEffect;
   }

   private void CheckCards(CardBase _card1, CardBase _card2)
   {
      CheckMove(_card1 as Card,_card2.GetTablePlace().Id,_card1.GetTablePlace().Id,false);
      CheckMove(_card2 as Card,_card1.GetTablePlace().Id,_card2.GetTablePlace().Id,false);
   }

   private void CheckMove(CardBase _cardBase, int _movedFrom, int _movedTo, bool _didTeleport)
   {
      if (_cardBase is not Keeper _keeper)
      {
         return;
      }

      if (_keeper.My)
      {
         return;
      }
      
      if (_didTeleport)
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
