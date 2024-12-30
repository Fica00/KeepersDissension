using System.Linq;

public class Shockwave : AbilityEffect
{
    private int strangeMatter;
    private int cardsToDamage;

    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        strangeMatter = 0;
        cardsToDamage = 0;
        int _damage = FindObjectsOfType<AbilityCard>().ToList().FindAll(_ability => _ability.My &&_ability.Details
        .Type == AbilityCardType.CrowdControl && _ability.Details.Color == AbilityColor.Red).Count;
        
        if (_damage==0)
        {
            Finish();
            DialogsManager.Instance.ShowOkDialog("You don't have any red ability card");
            return;
        }

        
        foreach (var _card in FindObjectsOfType<Card>().ToList())
        {
            if (!_card.IsAttackable())
            {
                continue;
            }

            if (_card.GetTablePlace()==null)
            {
                continue;
            }

            cardsToDamage++;
            GameplayManager.Instance.DamageCardByAbility(_card.UniqueId, _damage, _didKill =>
            {
                AddStrangeMatter(_card);
            });
        }
        
    }

    private void AddStrangeMatter(Card _card)
    {
        cardsToDamage--;

        strangeMatter += GameplayManager.Instance.GetStrangeMatterForCard(_card);
        
        if (cardsToDamage==0)
        {
            GameplayManager.Instance.ChangeStrangeMaterInEconomy(strangeMatter);
            Finish();
        }
    }

    private void Finish()
    {
        OnActivated?.Invoke();
        RemoveAction();
    }
}
