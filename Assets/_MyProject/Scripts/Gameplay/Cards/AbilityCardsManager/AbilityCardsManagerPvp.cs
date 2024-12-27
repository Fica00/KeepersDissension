using System.Collections;
using UnityEngine;

public class AbilityCardsManagerPvp : AbilityCardsManagerBase
{
    public override void Setup()
    {
        if (!FirebaseManager.Instance.RoomHandler.IsOwner)
        {
            return;
        }
        
        Debug.Log("Setting up");
        base.Setup();
    }

    protected override void CreateCards()
    {
        foreach (AbilityCard _ability in Resources.LoadAll<AbilityCard>("Abilities"))
        {
            var _abilityInstance = Instantiate(_ability, null);
            AbilityData _abilityData = _abilityInstance.CreateData("noone");
            _abilityInstance.Setup(_abilityData.UniqueId);
            FirebaseManager.Instance.RoomHandler.BoardData.Abilities.Add(_abilityData);
        }
    }

    protected override void DealAbilities()
    {
        AddStartingCards(true);
        AddStartingCards(false);
        
        void AddStartingCards(bool _isMyPlayer)
        {
            for (int _i = 0; _i < FirebaseManager.Instance.RoomHandler.BoardData.AmountOfStartingAbilities; _i++)
            {
                var _ability = DrawAbilityCard();
                if (_ability==null)
                {
                    return;
                }
                
                GameplayManager.Instance.AddAbilityToPlayer(_isMyPlayer, _ability.UniqueId);
            }
        }
    }

    protected override void SetupShopAbilities()
    {
        for (int _i = 0; _i < FirebaseManager.Instance.RoomHandler.BoardData.AmountOfCardsInShop; _i++)
        {
            var _ability = DrawAbilityCard(false);
            Debug.Log(11111);
            if (_ability==null)
            {
            Debug.Log(22222);
                return;
            }
            
            Debug.Log(_ability.UniqueId);
            GameplayManager.Instance.AddAbilityToShop(_ability.UniqueId);
        }
    }

    protected override void TryBuyFromShop(AbilityData _abilityData)
    {
        int _price = FirebaseManager.Instance.RoomHandler.BoardData.AbilityCardPrice - GameplayManager.Instance.StrangeMatterCostChange();
        
        if (_abilityData==null)
        {
            return;
        }
        
        if (GameplayManager.Instance.IsAbilityActive<Famine>())
        {
            DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
            return;
        }

        if (GameplayManager.Instance.IsResponseAction2())
        {
            if (!GameplayManager.Instance.IsMyResponseAction2())
            {
                return;
            }

            if (!GameplayManager.Instance.IsKeeperResponseAction2)
            {
                return;
            }
        }
        else if (!GameplayManager.Instance.IsMyTurn())
        {
            return;
        }

        if (GameplayManager.Instance.AmountOfAbilitiesPlayerCanBuy()<=0)
        {
            DialogsManager.Instance.ShowOkDialog("You can't buy any more abilities");
            return;
        }

        if (GameplayManager.Instance.MyStrangeMatter()<_price&& !GameplayCheats.HasUnlimitedGold)
        {
            DialogsManager.Instance.ShowOkDialog($"You need {_price} strange matter to buy ability card");
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog(
            $"Are you sure that you want to buy ability for {_price} strange matter?", ()=>
        {
            GameplayManager.Instance.ChangeMyStrangeMatter(-_price);
            GameplayManager.Instance.BuyAbilityFromShop(_abilityData.UniqueId);
            arrowPanel.Hide();
            StartCoroutine(ReplaceCardInShop());
        });
    }

    IEnumerator ReplaceCardInShop()
    {
        yield return new WaitForSeconds(1);
        var _ability = DrawAbilityCard(false);
        if (_ability==null)
        {
            yield break;
        }
        GameplayManager.Instance.AddAbilityToShop(_ability.UniqueId);
    }
    
    private AbilityData DrawAbilityCard(bool _startingDraw=true)
    {
        // var _allAbilities = FirebaseManager.Instance.RoomHandler.BoardData.AvailableAbilities;
        // for (int _i = 0; _i < _allAbilities.Count; _i++)
        // {
        //     if (_startingDraw && !CardsManager.Instance.CanAbilityBeGiven(_allAbilities[_i].CardId))
        //     {
        //         continue;
        //     }
        //
        //     return _allAbilities[_i];
        // }

        return null;
    }

    protected override void TryBuyFromHand(CardBase _card)
    {
        // if (_card is not AbilityCard _abilityCard)
        // {
        //     return;
        // }
        //
        // if (GameplayManager.Instance.IsResponseAction2())
        // {
        //     if (!GameplayManager.Instance.IsMyResponseAction2())
        //     {
        //         return;
        //     }
        //
        //     if (!GameplayManager.Instance.IsKeeperResponseAction2)
        //     {
        //         return;
        //     }
        // }
        // else if (!GameplayManager.Instance.IsMyTurn())
        // {
        //     return;
        // }
        //
        // int _price = FirebaseManager.Instance.RoomHandler.RoomData.BoardData.AbilityCardPrice - GameplayManager.Instance.StrangeMatterCostChange();
        //
        // if (GameplayManager.Instance.MyStrangeMatter()<_price && !GameplayCheats.HasUnlimitedGold)
        // {
        //     DialogsManager.Instance.ShowOkDialog($"You dont have enough strange matter, this action requires {_price}");
        //     return;
        // }
        //
        // if (_abilityCard==null)
        // {
        //     return;
        // }
        //
        // DialogsManager.Instance.ShowYesNoDialog($"Do you want to buy this ability for {_price} strange matter?", () =>
        // {
        //     if (_abilityCard != null)
        //     {
        //         GameplayManager.Instance.BuyAbilityFromHand(_abilityCard.UniqueId);
        //         GameplayManager.Instance.ChangeMyStrangeMatter(-_price);
        //     }
        // });
    }

    public override AbilityData RemoveAbilityFromShop(string _abilityId)
    {
        // var _abilitiesInShop = FirebaseManager.Instance.RoomHandler.BoardData.AbilitiesInShop;
        // AbilityData _ability = _abilitiesInShop.Find(_element => _element.UniqueId == _abilityId);
        // if (_ability==null)
        // {
        //     return null;
        // }
        // _abilitiesInShop.Remove(_ability);
        // for (int _i = 0; _i < ShopAbilitiesDisplays.Count; _i++)
        // {
        //     if (ShopAbilitiesDisplays[_i].Ability==_ability)
        //     {
        //         ShopAbilitiesDisplays[_i].Empty();
        //     }
        // }
        //
        // ShowShop();
        // return _ability;
        return null;
    }
    
    private void ShowShop()
    {
        // for (int _i = 0; _i < ShopAbilitiesDisplays.Count; _i++)
        // {
        //     var _abilitiesInShop = FirebaseManager.Instance.RoomHandler.BoardData.AbilitiesInShop;
        //     if (_i>=_abilitiesInShop.Count)
        //     {
        //         return;
        //     }
        //     ShopAbilitiesDisplays[_i].Setup(_abilitiesInShop[_i]);
        // }
    }
}
