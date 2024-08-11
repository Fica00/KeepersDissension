using System.Linq;

public class Retaliate : AbilityEffect
{
    private int counter = 0;
    private Keeper keeper;
    private bool IsActive;
    
    public override void ActivateForOwner()
    {
        if (IsActive)
        {
            MoveToActivationField();
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }
        counter = 2;
        keeper = FindObjectsOfType<Keeper>().ToList().Find(_keeper => _keeper.My);
        MoveToActivationField();
        RemoveAction();
        OnActivated?.Invoke();
        GameplayManager.OnCardAttacked += CheckAttackingCard;
        GameplayManager.Instance.MyPlayer.OnEndedTurn += LowerCounter;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        IsActive = true;
    }

    public override void ActivateForOther()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        counter = 2;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        GameplayManager.Instance.OpponentPlayer.OnEndedTurn += DisableActiveDisplay;
    }

    private void DisableActiveDisplay()
    {
        if (counter>0)
        {
            counter--;
            return;
        }

        IsActive = false;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }

    private void LowerCounter()
    {
        if (counter>0)
        {
            counter--;
            return;
        }

        IsActive = false;
        GameplayManager.OnCardAttacked -= CheckAttackingCard;
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= LowerCounter;
    }

    private void CheckAttackingCard(CardBase _attackingCard, CardBase _defendingCard, int _damage)
    {
        if (_defendingCard != keeper)
        {
            return;
        }
        if (_attackingCard == keeper)
        {
            return;
        }

        CardAction _returnDamage = new CardAction
        {
            StartingPlaceId = _attackingCard.GetTablePlace().Id,
            FirstCardId = (_attackingCard as Card).Details.Id,
            FinishingPlaceId = _attackingCard.GetTablePlace().Id,
            SecondCardId = (_attackingCard as Card).Details.Id,
            Type = CardActionType.Attack,
            Cost = 0,
            IsMy = true,
            CanTransferLoot = false,
            Damage = _damage,
            CanCounter = false,
            GiveLoot = false
        };
        
        GameplayManager.Instance.ExecuteCardAction(_returnDamage);
    }

    private void OnDisable()
    {
        CancelEffect();
    }

    public override void CancelEffect()
    {
        if (keeper==null)
        {
            return;
        }
        
        counter = 0;
        LowerCounter();
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
    }
}
