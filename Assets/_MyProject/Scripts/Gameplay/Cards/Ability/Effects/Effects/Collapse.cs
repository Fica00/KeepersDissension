public class Collapse : AbilityEffect
{
   public static bool IsActive;

   private void OnEnable()
   {
      IsActive = false;
   }

   public override void ActivateForOwner()
   {
      RemoveAction();
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
      OnActivated?.Invoke();
   }

   public override void ActivateForOther()
   {
      IsActive = true;
      AbilityCard.ActiveDisplay.gameObject.SetActive(true);
   }

   public override void CancelEffect()
   {
      IsActive = false;
      AbilityCard.ActiveDisplay.gameObject.SetActive(false);
   }
}
