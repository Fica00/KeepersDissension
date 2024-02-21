using UnityEngine;

public class Grounded : AbilityEffect
{
   private bool isActiveForMe;
   private bool isActiveForOpponent;
   private CardBase defendingCard;
   private bool skipFirst;


   private void OnEnable()
   {
      isActiveForMe = false;
      isActiveForOpponent = false;
   }

   public override void ActivateForOwner()
   {
      if (isActiveForMe)
      {
         RemoveAction();
         MoveToActivationField();
         OnActivated?.Invoke();
         return;
      }
      RemoveAction();
      OnActivated?.Invoke();
      MoveToActivationField();
      isActiveForMe = true;
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
   }

   public override void ActivateForOther()
   {
      if (isActiveForOpponent)
      {
         return;
      }
      isActiveForOpponent = true;
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
   }

   public bool IsActive(CardBase _attackingCard, CardBase _defendingCard)
   {
      if (_attackingCard is not Keeper)
      {
         return false;
      }

      switch (_attackingCard.My)
      {
         case true when isActiveForMe:
            Setup();
            defendingCard = _defendingCard;
            return true;
         case false when isActiveForOpponent:
            Setup();
            defendingCard = _defendingCard;
            return true;
         default:
            return false;
      }
   }

   private void Setup()
   {
      skipFirst = true;
      isActiveForMe = false;
      isActiveForOpponent = false;
      GameplayManager.OnCardAttacked += CheckAttackedCard;
   }

   private void CheckAttackedCard(CardBase _attackingCard, CardBase _defendingCard, int _damage)
   {
      if (skipFirst)
      {
         skipFirst = false;
         return;
      }
      if (_defendingCard != defendingCard)
      {
         return;
      }

      if (_attackingCard == defendingCard)
      {
         return;
      }

      if (_damage<1)
      {
         return;
      }

      if (defendingCard is Card { CanFlyToDodgeAttack: true })
      {
         return;
      }
      
      GameplayManager.Instance.ChangeMovementForCard(defendingCard.GetTablePlace().Id,true);
      GameplayManager.OnCardAttacked -= CheckAttackedCard;
      AbilityCard.ActiveDisplay.gameObject.SetActive(false);
   }

   public override void CancelEffect()
   {
      AbilityCard.ActiveDisplay.gameObject.SetActive(false);
      isActiveForMe = false;
      isActiveForOpponent = false;
   }
}
