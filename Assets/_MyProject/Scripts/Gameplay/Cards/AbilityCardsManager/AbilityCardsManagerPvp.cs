using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityCardsManagerPvp : AbilityCardsManagerBase
{
    public override void Setup()
    {
        if (!FirebaseManager.Instance.RoomHandler.IsOwner)
        {
            return;
        }
        
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
        AddStartingCards(FirebaseManager.Instance.RoomHandler.BoardData.MyPlayer.PlayerId);
        AddStartingCards(FirebaseManager.Instance.RoomHandler.BoardData.OpponentPlayer.PlayerId);
        
        void AddStartingCards(string _owner)
        {
            for (int _i = 0; _i < FirebaseManager.Instance.RoomHandler.BoardData.AmountOfStartingAbilities; _i++)
            {
                var _ability = DrawAbilityCard();
                if (_ability==null)
                {
                    return;
                }
                
                GameplayManager.Instance.AddAbilityToPlayer(_owner, _ability.UniqueId);
            }
        }
    }

    protected override void SetupShopAbilities()
    {
        for (int _i = 0; _i < FirebaseManager.Instance.RoomHandler.BoardData.AmountOfCardsInShop; _i++)
        {
            var _ability = DrawAbilityCard(false);
            if (_ability==null)
            {
                return;
            }
            
            GameplayManager.Instance.AddAbilityToShop(_ability.UniqueId);
        }
    }

    protected override void BuyAbility(AbilityData _abilityData)
    {
        int _price = FirebaseManager.Instance.RoomHandler.BoardData.AbilityCardPrice - GameplayManager.Instance.StrangeMatterCostChange(true);
        
        if (!GameplayManager.Instance.CanPlayerDoActions())
        {
            return;
        }
        
        if (_abilityData==null)
        {
            return;
        }
        
        if (GameplayManager.Instance.IsAbilityActive<Famine>())
        {
            DialogsManager.Instance.ShowOkDialog("Using strange matter is forbidden by Famine ability");
            return;
        }

        if (GameplayManager.Instance.IsResponseAction())
        {
            if (!GameplayManager.Instance.IsMyResponseAction())
            {
                return;
            }

            if (!GameplayManager.Instance.IsKeeperResponseAction)
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

        if (GameplayManager.Instance.MyStrangeMatter()<_price)
        {
            DialogsManager.Instance.ShowOkDialog($"You need {_price} strange matter to buy ability card");
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog(
            $"Are you sure that you want to buy ability for {_price} strange matter?", ()=>
        {
            GameplayManager.Instance.TellOpponentSomething("Opponent bought ability card");
            GameplayManager.Instance.ChangeMyStrangeMatter(-_price);
            GameplayManager.Instance.ChangeStrangeMaterInEconomy(_price);
            arrowPanel.Hide();
            ReplaceCardInShop();
            GameplayManager.Instance.BuyAbility(_abilityData.UniqueId);
        });
    }

    private void ReplaceCardInShop()
    {
        var _ability = DrawAbilityCard(false);
        if (_ability==null)
        {
            return;
        }
        GameplayManager.Instance.AddAbilityToShop(_ability.UniqueId);
    }
    
    private AbilityData DrawAbilityCard(bool _startingDraw=true)
    {
        var _allAbilities = FirebaseManager.Instance.RoomHandler.BoardData.Abilities;
        for (int _i = 0; _i < _allAbilities.Count; _i++)
        {
            if (_startingDraw && !CardsManager.Instance.CanAbilityBeGiven(_allAbilities[_i].CardId))
            {
                continue;
            }

            if (_allAbilities[_i].Owner != "noone")
            {
                continue;
            }
        
            return _allAbilities[_i];
        }

        return null;
    }

    protected override void TryBuyFromHand(CardBase _card)
    {
        if (!GameplayManager.Instance.CanPlayerDoActions())
        {
            return;
        }
        
        if (_card is not AbilityCard _abilityCard)
        {
            return;
        }
        
        if (GameplayManager.Instance.IsResponseAction())
        {
            if (!GameplayManager.Instance.IsMyResponseAction())
            {
                return;
            }
        
            if (!GameplayManager.Instance.IsKeeperResponseAction)
            {
                return;
            }
        }
        else if (!GameplayManager.Instance.IsMyTurn())
        {
            return;
        }
        
        int _price = FirebaseManager.Instance.RoomHandler.RoomData.BoardData.AbilityCardPrice - GameplayManager.Instance.StrangeMatterCostChange
            (true);
        
        if (GameplayManager.Instance.MyStrangeMatter()<_price)
        {
            DialogsManager.Instance.ShowOkDialog($"You dont have enough strange matter, this action requires {_price}");
            return;
        }
        
        if (_abilityCard==null)
        {
            return;
        }
        
        DialogsManager.Instance.ShowYesNoDialog($"Do you want to buy this ability for {_price} strange matter?", () =>
        {
            if (_abilityCard != null)
            {
                GameplayManager.Instance.ChangeMyStrangeMatter(-_price);
                GameplayManager.Instance.BuyAbility(_abilityCard.UniqueId);
            }
        });
    }

    public override AbilityData RemoveAbilityFromShop(string _abilityId)
    {
        var _abilitiesInShop = FirebaseManager.Instance.RoomHandler.BoardData.Abilities;
        AbilityData _ability = _abilitiesInShop.Find(_element => _element.UniqueId == _abilityId);
        if (_ability==null)
        {
            return null;
        }
        
        for (int _i = 0; _i < ShopAbilitiesDisplays.Count; _i++)
        {
            if (ShopAbilitiesDisplays[_i].Ability==_ability)
            {
                ShopAbilitiesDisplays[_i].Empty();
            }
        }
        
        ShowShop();
        return _ability;
    }
    
    protected override void ShowShop()
    {
        List<AbilityData> _abilitiesInShop = FirebaseManager.Instance.RoomHandler.BoardData.Abilities.FindAll(_ability => _ability.Owner == "shop");
        _abilitiesInShop = _abilitiesInShop.OrderBy(_ability => _ability.Name).ToList();

        foreach (var _shopAbility in ShopAbilitiesDisplays)
        {
            _shopAbility.Empty();
        }

        for (int _i = 0; _i < _abilitiesInShop.Count; _i++)
        {
            ShopAbilitiesDisplays[_i].Setup(_abilitiesInShop[_i]);
        }
    }
}
