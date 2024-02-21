using System.Linq;

public class Repel : AbilityEffect
{
    private Keeper keeper;
    
    public override void ActivateForOwner()
    {
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        RemoveAction();
        OnActivated?.Invoke();
        GameplayManager.OnCardAttacked += CheckAttackingCard;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    public override void ActivateForOther()
    {
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (keeper!=null)
        {
            GameplayManager.OnCardAttacked -= CheckAttackingCard;
        }
    }

    private void CheckAttackingCard(CardBase _attackingCard, CardBase _defendingCard, int _damage)
    {
        if (_attackingCard is not Keeper)
        {
            return;
        }
        
        GameplayManager.Instance.PushCardBack(_defendingCard.GetTablePlace().Id,_attackingCard.GetTablePlace().Id, 100);
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        
        GameplayManager.OnCardAttacked -= CheckAttackingCard;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
