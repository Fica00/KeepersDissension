using UnityEngine;

public class Grounded : AbilityEffect
{
   public override void ActivateForOwner()
   {
      // if (IsActive)
      // {
      //    RemoveAction();
      //    MoveToActivationField();
      //    OnActivated?.Invoke();
      //    return;
      // }
      // RemoveAction();
      // OnActivated?.Invoke();
      // MoveToActivationField();
      // isActiveForMe = true;
      // AbilityCard.ActiveDisplay.gameObject.SetActive(true);
   }

   public bool IsActive(CardBase _attackingCard, CardBase _defendingCard)
   {
      // if (_attackingCard is not Keeper)
      // {
      //    return false;
      // }
      //
      // switch ((_attackingCard as Card).My)
      // {
      //    case true when isActiveForMe:
      //       Setup();
      //       defendingCard = _defendingCard;
      //       return true;
      //    case false when isActiveForOpponent:
      //       Setup();
      //       defendingCard = _defendingCard;
      //       return true;
      //    default:
      //       return false;
      // }

      return true;
   }

   private void Setup()
   {
      // skipFirst = true;
      // isActiveForMe = false;
      // isActiveForOpponent = false;
      // GameplayManager.OnCardAttacked += CheckAttackedCard;
   }

   private void CheckAttackedCard(CardBase _attackingCard, CardBase _defendingCard, int _damage)
   {
      // if (skipFirst)
      // {
      //    skipFirst = false;
      //    return;
      // }
      // if (_defendingCard != defendingCard)
      // {
      //    return;
      // }
      //
      // if (_attackingCard == defendingCard)
      // {
      //    return;
      // }
      //
      // if (_damage<1)
      // {
      //    return;
      // }
      //
      // Debug.Log(defendingCard.name,defendingCard.gameObject);
      // Debug.Log(defendingCard.GetTablePlace().Id,defendingCard.gameObject);
      //
      //
      // GameplayManager.Instance.ChangeMovementForCard(defendingCard.GetTablePlace().Id,true);
      // GameplayManager.OnCardAttacked -= CheckAttackedCard;
      // AbilityCard.ActiveDisplay.gameObject.SetActive(false);
   }

   public override void CancelEffect()
   {
      // AbilityCard.ActiveDisplay.gameObject.SetActive(false);
      // isActiveForMe = false;
      // isActiveForOpponent = false;
   }
}
