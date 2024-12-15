using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityCardsManagerBase : MonoBehaviour
{
    public static Action<AbilityCardType> UsedAbility;
    public static AbilityCardsManagerBase Instance;

    [SerializeField] protected int abilityCardPrice;
    [SerializeField] protected int amountOfStartingAbilities;
    [SerializeField] protected ArrowPanel arrowPanel;
    [SerializeField] private AbilityShopDisplay abilityShopDisplayPrefab;
    [SerializeField] private Transform abilityShopDisplayHolder;
    [SerializeField] private int amountOfCardsInShop;

    private List<AbilityCard> abilities;
    private List<AbilityCard> abilitiesInShop = new ();
    protected List<AbilityShopDisplay> shopAbilityDisplay = new ();

    protected void Awake()
    {
        Instance = this;
        abilities = CardsManager.Instance.GetAbilityCards();
        if (FirebaseManager.Instance.RoomHandler.IsTestingRoom)
        {
            amountOfStartingAbilities = 0;
            amountOfCardsInShop = abilities.Count;
        }

        for (int i = 0; i < amountOfCardsInShop; i++)
        {
            AbilityShopDisplay _abilityDisplay = Instantiate(abilityShopDisplayPrefab, abilityShopDisplayHolder);
            shopAbilityDisplay.Add(_abilityDisplay);
        }

        abilities = FirebaseManager.Instance.RoomHandler.IsTestingRoom 
            ? abilities.OrderBy(_ability => _ability.Details.Foreground.name).ToList() 
            : abilities.OrderBy(_ => Guid.NewGuid()).ToList();
        
        foreach (var _ability in abilities)
        {
            _ability.Setup("Shop");
            _ability.SetParent(transform);
        }
    }
    
    private void OnEnable()
    {
        CardHandInteractions.OnCardClicked += BuyCardFromHand;
        AbilityShopDisplay.OnAbilityClicked += TryBuyAbilityCard;
    }

    private void OnDisable()
    {
        CardHandInteractions.OnCardClicked -= BuyCardFromHand;
        AbilityShopDisplay.OnAbilityClicked -= TryBuyAbilityCard;
    }
    
    protected virtual void TryBuyAbilityCard(AbilityCard _abilityCard)
    {
        throw new Exception("Try buy ability card must be implemented");
    }

    private void BuyCardFromHand(CardBase _card)
    {
        if (_card is not AbilityCard _abilityCard)
        {
            return;
        }

        if (!(abilities.Contains(_abilityCard) || GameplayManager.Instance.MyPlayer.GetAbilities().Contains
        (_abilityCard)))
        {
            return;
        }

        if (!GameplayManager.Instance.MyTurn && !GameplayManager.Instance.IsKeeperResponseAction)
        {
            return;
        }

        GameplayPlayer _player = GameplayManager.Instance.MyPlayer;

        int _price = abilityCardPrice - GameplayManager.Instance.StrangeMatterCostChange();

        if (_player.StrangeMatter<_price && !GameplayCheats.HasUnlimitedGold)
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
                YesBuyAbilityFromHand(_abilityCard.Details.Id);
                UsedAbility?.Invoke(_abilityCard.Details.Type);
                _player.RemoveStrangeMatter(_price);
            }
        });
    }

    private void YesBuyAbilityFromHand(int _abilityId)
    {
        GameplayManager.Instance.BuyAbilityFromHand(_abilityId);
    }

    public void Setup()
    {
        if (!FirebaseManager.Instance.RoomHandler.IsOwner)
        {
            return;
        }
        
        DealAbilities();
        SetupShopAbilities();
    }
    
    protected virtual void DealAbilities()
    {
        throw new Exception("Deal abilities must be implemented");
    }

    protected virtual void SetupShopAbilities()
    {
        throw new Exception("Setup shop abilities must be implemented");
    }

    public AbilityCard DrawAbilityCard(bool _startingDraw=true)
    {
        if (abilities.Count==0)
        {
            return null;
        }

        for (int _i = 0; _i < abilities.Count; _i++)
        {
            if (_startingDraw && !abilities[_i].Details.CanBeGivenToPlayer)
            {
                continue;
            }

            return abilities[_i];
        }

        return null;
    }

    public AbilityCard DrawAbilityCard(int _id)
    {
        AbilityCard _chosenAbility = null;
        foreach (var _ability in abilities)
        {
            if (_ability.Details.Id==_id)
            {
                _chosenAbility = _ability;
                break;
            }
        }

        return _chosenAbility;
    }

    public void RemoveAbility(AbilityCard _ability)
    {
        abilities.Remove(_ability);
    }

    public void AddAbilityToShop(AbilityCard _ability)
    {
        abilitiesInShop.Add(_ability);
        _ability.SetIsMy("Shop");
        ShowShop();
    }

    private void ShowShop()
    {
        for (int i = 0; i < shopAbilityDisplay.Count; i++)
        {
            if (i>=abilitiesInShop.Count)
            {
                return;
            }
            shopAbilityDisplay[i].Setup(abilitiesInShop[i]);
        }
    }

    public AbilityCard RemoveAbilityFromShop(int _abilityId)
    {
        AbilityCard _ability = abilitiesInShop.Find(_element => _element.Details.Id == _abilityId);
        abilitiesInShop.Remove(_ability);
        for (int i = 0; i < shopAbilityDisplay.Count; i++)
        {
            if (shopAbilityDisplay[i].Ability==_ability)
            {
                shopAbilityDisplay[i].Empty();
            }
        }

        ShowShop();
        return _ability;
    }
}
