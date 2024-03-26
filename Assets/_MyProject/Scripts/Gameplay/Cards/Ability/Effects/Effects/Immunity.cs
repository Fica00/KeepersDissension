public class Immunity : AbilityEffect
{
   public static bool IsActiveForMe;
   public static bool IsActiveForOpponent;
   private void OnEnable()
   {
      IsActiveForMe = false;
      IsActiveForOpponent = false;
   }

   public override void ActivateForOwner()
   {
      MoveToActivationField();
      IsActiveForMe = true;
      RemoveAction();
      OnActivated?.Invoke();
      GameplayManager.Instance.MyPlayer.OnEndedTurn += DisableEffect;
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
   }

   public override void ActivateForOther()
   {
      IsActiveForOpponent = true;
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
      GameplayManager.Instance.OpponentPlayer.OnEndedTurn += DisableEffect;
   }

   private void DisableEffect()
   {
      if (IsActiveForMe)
      {
         GameplayManager.Instance.MyPlayer.OnEndedTurn -= DisableEffect;
      }
      else
      {
         GameplayManager.Instance.OpponentPlayer.OnEndedTurn -= DisableEffect;
      }
      IsActiveForMe = false;
      IsActiveForOpponent = false;
      AbilityCard.ActiveDisplay.gameObject.SetActive(false);
   }

   public override void CancelEffect()
   {
      DisableEffect();
   }
}
