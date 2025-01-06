using System.Linq;
using UnityEngine;

public class SlowDown : AbilityEffect
{
    protected override void ActivateForOwner()
        {
            SetIsActive(true);
            ClearEffectedCards();
            SetRemainingCooldown(2);
            GameplayManager.Instance.MyPlayer.OnEndedTurn += LowerCounter;
            ManageActiveDisplay(true);
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
        }
    
        public void AddCard(string _uniqueId)
        {
            AddEffectedCard(_uniqueId);
            RoomUpdater.Instance.ForceUpdate();
        }

        public void ClearEffectedCardsForMe()
        {
            Debug.Log(11111);
            foreach (var _effectedCard in GetEffectedCards().ToList())
            {
                if (!_effectedCard.GetIsMy())
                {
                    continue;
                }
                
                RemoveEffectedCard(_effectedCard.UniqueId);
            }
            
            RoomUpdater.Instance.ForceUpdate();
        }
    
        public bool CanMoveCard(string _uniqueId)
        {
            if (!IsActive)
            {
                return true;
            }
    
            return !GetEffectedCards().Find(_cardSavedCard =>_cardSavedCard.UniqueId == _uniqueId);
        }
    
        protected override void CancelEffect()
        {
            SetRemainingCooldown(0);
            LowerCounter();
        }
        
        private void LowerCounter()
        {
            if (RemainingCooldown>0)
            {
                SetRemainingCooldown(RemainingCooldown-1);
                RoomUpdater.Instance.ForceUpdate();
                return;
            }
            
            ManageActiveDisplay(false);
            GameplayManager.Instance.MyPlayer.OnEndedTurn -= LowerCounter;
            SetIsActive(false);
            ClearEffectedCards();
            RoomUpdater.Instance.ForceUpdate();
        }
}