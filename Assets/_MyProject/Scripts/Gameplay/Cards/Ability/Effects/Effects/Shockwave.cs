using System.Collections.Generic;
using System.Linq;

public class Shockwave : AbilityEffect
{
    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        
        int _damage = FindObjectsOfType<AbilityCard>().ToList().FindAll(_ability => _ability.My &&_ability.Details
        .Type == AbilityCardType.CrowdControl && _ability.Details.Color == AbilityColor.Red && _ability.GetTablePlace() != null).Count;
        
        if (_damage==0)
        {
            Finish();
            DialogsManager.Instance.ShowOkDialog("You don't have any red ability card");
            return;
        }

        
        List<Card> _killedCards = new List<Card>();
        int _amountOfCardsToDamage = 0;
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

            _amountOfCardsToDamage++;
            GameplayManager.Instance.DamageCardByAbility(_card.UniqueId, _damage, _didKill =>
            {
                _amountOfCardsToDamage--;
                if (_didKill)
                {
                    _killedCards.Add(_card);
                }

                if (_amountOfCardsToDamage==0)
                {
                    CalculateStrangeMatter();
                }
            });
        }

        void CalculateStrangeMatter()
        {
            int _strangeMatter = 0;
            foreach (var _killedCard in _killedCards)
            {
                _strangeMatter += GameplayManager.Instance.GetStrangeMatterForCard(_killedCard);
            }
            
            GameplayManager.Instance.ChangeStrangeMaterInEconomy(_strangeMatter);
            Finish();
        }
    }

    private void Finish()
    {
        OnActivated?.Invoke();
        RemoveAction();
    }
}
