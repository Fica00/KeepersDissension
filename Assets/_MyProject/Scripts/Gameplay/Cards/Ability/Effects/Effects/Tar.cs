using UnityEngine;

public class Tar : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        SetIsActive(true);
        GameplayManager.Instance.MyPlayer.OnEndedTurn += TryEnd;
        MoveToActivationField();
        ManageActiveDisplay(true);
        SetRemainingCooldown(1);
        OnActivated?.Invoke();
        RemoveAction();
    }

    private void TryEnd()
    {
        Debug.Log(RemainingCooldown);
        if (RemainingCooldown>0)
        {
            SetRemainingCooldown(RemainingCooldown-1);
            RoomUpdater.Instance.ForceUpdate();
            return;
        }
        
        Debug.Log("got here");
        SetIsActive(false);
        ManageActiveDisplay(false);
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= TryEnd;
        RoomUpdater.Instance.ForceUpdate();
    }

    protected override void CancelEffect()
    {
        SetRemainingCooldown(0);
        TryEnd();
    }
}