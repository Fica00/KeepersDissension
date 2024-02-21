using System.Collections.Generic;

public class SlowDown : AbilityEffect
{
    public static bool IsActive;
    private List<Card> cardsThatMovedThisTurn = new ();
    private GameplayPlayer player;
    private GameplayPlayer activatingPlayer;
    private int counter;

    private void OnEnable()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;   
        }
        activatingPlayer = GameplayManager.Instance.MyPlayer;
        Activate();
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
    }

    public override void ActivateForOther()
    {
        if (IsActive)
        {
            return;
        }
        activatingPlayer = GameplayManager.Instance.OpponentPlayer;
        Activate();
    }

    private void Activate()
    {
        player = GameplayManager.Instance.MyPlayer;
        IsActive = true;
        ClearList();
        counter = 2;
        player.OnEndedTurn += ClearList;
        GameplayManager.OnCardMoved += AddCard;
        activatingPlayer.OnEndedTurn += LowerCounter;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void LowerCounter()
    {
        if (counter>0)
        {
            counter--;
            return;
        }
        
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        player.OnEndedTurn -= ClearList;
        GameplayManager.OnCardMoved -= AddCard;
        activatingPlayer.OnEndedTurn -= LowerCounter;
        IsActive = false;
    }

    private void AddCard(CardBase _cardThatMoved, int _arg2, int _arg3)
    {
        cardsThatMovedThisTurn.Add(_cardThatMoved as Card);
    }

    private void ClearList()
    {
        cardsThatMovedThisTurn = new List<Card>();
    }

    public bool CanMoveCard(Card _card)
    {
        if (!IsActive)
        {
            return true;
        }

        return !cardsThatMovedThisTurn.Contains(_card);
    }

    public override void CancelEffect()
    {
        if (counter==0)
        {
            return;
        }

        counter = 0;
        LowerCounter();
    }
}
