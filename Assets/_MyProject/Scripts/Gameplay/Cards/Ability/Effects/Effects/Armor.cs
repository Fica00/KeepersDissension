using UnityEngine;

public class Armor : AbilityEffect
{
    [HideInInspector] public bool IsActive;
    private GameplayPlayer player;
    private GameplayPlayer secondPlayer;
    
    public override void ActivateForOwner()
    {
        player = GameplayManager.Instance.MyPlayer;
        secondPlayer = GameplayManager.Instance.OpponentPlayer;
        player.OnStartedTurn += Activate;
        secondPlayer.OnStartedTurn += Activate;
        Activate();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        player = GameplayManager.Instance.OpponentPlayer;
        secondPlayer = GameplayManager.Instance.MyPlayer;
        player.OnStartedTurn += Activate;
        secondPlayer.OnStartedTurn += Activate;
        Activate();
    }

    private void OnDisable()
    {
        if (player==null)
        {
            return;
        }

        player.OnStartedTurn -= Activate;
        secondPlayer.OnStartedTurn -= Activate;
        player = null;
        secondPlayer = null;
    }
    
    private void Activate()
    {
        IsActive = true;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void CancelEffect()
    {
        if (player == null)
        {
            return;
        }

        OnDisable();
        IsActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
