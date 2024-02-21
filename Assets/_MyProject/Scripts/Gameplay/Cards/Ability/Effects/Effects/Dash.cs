using System.Linq;

public class Dash : AbilityEffect
{
    private Keeper keeper;
    private GameplayPlayer player;
    
    public override void ActivateForOwner()
    {
        player = GameplayManager.Instance.MyPlayer;
        Activate(true);
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        player = GameplayManager.Instance.OpponentPlayer;
        Activate(false);
    }

    private void Activate(bool _isMy)
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My==_isMy);
        player.OnEndedTurn += AddSpeed;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void AddSpeed()
    {
        keeper.Speed=1;
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        player.OnEndedTurn -= AddSpeed;
        player = null;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (player!=null)
        {
            player.OnEndedTurn -= AddSpeed;
        }
    }
}
