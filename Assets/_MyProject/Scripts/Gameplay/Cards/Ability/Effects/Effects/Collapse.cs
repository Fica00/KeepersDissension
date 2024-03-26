public class Collapse : AbilityEffect
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
      RemoveAction();
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
      OnActivated?.Invoke();
   }

   public override void ActivateForOther()
   {
      IsActiveForOpponent = true;
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
   }

   public override void CancelEffect()
   {
      IsActiveForMe = false;
      AbilityCard.ActiveDisplay.gameObject.SetActive(false);
   }
}
