using System.Collections;
using UnityEngine;

public class AbilityCardsManagerPVP : AbilityCardsManagerBase
{
    protected override void DealAbilities()
    {
        AddStartingCards(true);
        AddStartingCards(false);
        
        void AddStartingCards(bool _isMyPlayer)
        {
            for (int _i = 0; _i < amountOfStartingAbilities; _i++)
            {
                AbilityCard _ability = DrawAbilityCard();
                if (_ability==null)
                {
                    return;
                }
                GameplayManager.Instance.AddAbilityToPlayer(_isMyPlayer, _ability.Details.Id);
            }
        }
    }

    protected override void SetupShopAbilities()
    {
        for (int _i = 0; _i < shopAbilityDisplay.Count; _i++)
        {
            AbilityCard _ability = DrawAbilityCard(false);
            if (_ability==null)
            {
                return;
            }
            GameplayManager.Instance.AddAbilityToShop(_ability.Details.Id);
        }
    }

    protected override void TryBuyAbilityCard(AbilityCard _abilityCard)
    {
        int _price = abilityCardPrice - GameplayManager.Instance.StrangeMatterCostChange;
        if (_abilityCard==null)
        {
            return;
        }
        
        if (Famine.IsActive)
        {
            UIManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
            return;
        }

        if (!GameplayManager.Instance.MyTurn && !GameplayManager.Instance.IsKeeperResponseAction)
        {
            return;
        }

        GameplayPlayer _player = GameplayManager.Instance.MyPlayer;

        if (_player.AmountOfAbilitiesPlayerCanBuy<=0)
        {
            UIManager.Instance.ShowOkDialog("You can't buy any more abilities");
            return;
        }

        if (_player.StrangeMatter<_price&& !GameplayCheats.HasUnlimitedGold)
        {
            UIManager.Instance.ShowOkDialog($"You need {_price} strange matter to buy ability card");
            return;
        }

        UIManager.Instance.ShowYesNoDialog(
            $"Are you sure that you want to buy ability for {_price} strange matter?", ()=>
        {
            _player.RemoveStrangeMatter(_price);
            GameplayManager.Instance.BuyAbilityFromShop(_abilityCard.Details.Id);
            arrowPanel.Hide();
            StartCoroutine(ReplaceCardInShop());
        });
    }

    IEnumerator ReplaceCardInShop()
    {
        yield return new WaitForSeconds(1);
        AbilityCard _ability = DrawAbilityCard(false);
        if (_ability==null)
        {
            yield break;
        }
        GameplayManager.Instance.AddAbilityToShop(_ability.Details.Id);
    }
}
