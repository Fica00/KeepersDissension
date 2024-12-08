public class Immunity : AbilityEffect
{
   public override void ActivateForOwner()
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

   public override void CancelEffect()
   {
      DisableEffect();
   }
}
