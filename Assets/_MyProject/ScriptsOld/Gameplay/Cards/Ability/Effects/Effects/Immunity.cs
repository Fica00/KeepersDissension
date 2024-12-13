public class Immunity : AbilityEffect
{
   protected override void ActivateForOwner()
   {
      MoveToActivationField();
      SetIsActive(true);
      RemoveAction();
      OnActivated?.Invoke();
      GameplayManager.Instance.MyPlayer.OnEndedTurn += DisableEffect;
      ManageActiveDisplay(true);
   }

   private void DisableEffect()
   {
      if (!IsActive)
      {
         return;
      }
      
      GameplayManager.Instance.MyPlayer.OnEndedTurn -= DisableEffect;
      SetIsActive(false);
      ManageActiveDisplay(false);
   }

   protected override void CancelEffect()
   {
      DisableEffect();
   }
}
