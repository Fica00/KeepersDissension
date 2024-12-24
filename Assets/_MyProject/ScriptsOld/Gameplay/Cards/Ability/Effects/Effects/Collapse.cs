public class Collapse : AbilityEffect
{
   protected override void ActivateForOwner()
   {
      SetIsActive(true);
      ManageActiveDisplay(true);
      OnActivated?.Invoke();
      RemoveAction();
   }

   protected override void CancelEffect()
   {
      SetIsActive(false);
      ManageActiveDisplay(false);
   }
}
