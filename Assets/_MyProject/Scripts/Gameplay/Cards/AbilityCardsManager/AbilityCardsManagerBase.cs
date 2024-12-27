using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCardsManagerBase : MonoBehaviour
{
    public static AbilityCardsManagerBase Instance;

    [SerializeField] protected ArrowPanel arrowPanel;
    [SerializeField] private AbilityShopDisplay abilityShopDisplayPrefab;
    [SerializeField] private Transform abilityShopDisplayHolder;

    protected List<AbilityShopDisplay> ShopAbilitiesDisplays = new();
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int _i = 0; _i < FirebaseManager.Instance.RoomHandler.BoardData.AmountOfCardsInShop; _i++)
        {
            AbilityShopDisplay _abilityDisplay = Instantiate(abilityShopDisplayPrefab, abilityShopDisplayHolder);
            ShopAbilitiesDisplays.Add(_abilityDisplay);
        }
    }

    public virtual void Setup()
    {
        CreateCards();
        DealAbilities();
        SetupShopAbilities();
    }

    protected virtual void CreateCards()
    {
        throw new Exception("Create cards");
    }
    
    protected virtual void DealAbilities()
    {
        throw new Exception("Deal abilities must be implemented");
    }

    protected virtual void SetupShopAbilities()
    {
        throw new Exception();
    }
    
    private void OnEnable()
    {
        CardHandInteractions.OnCardClicked += TryBuyFromHand;
        AbilityShopDisplay.OnAbilityClicked += TryBuyFromShop;
    }

    private void OnDisable()
    {
        CardHandInteractions.OnCardClicked -= TryBuyFromHand;
        AbilityShopDisplay.OnAbilityClicked -= TryBuyFromShop;
    }

    protected virtual void TryBuyFromHand(CardBase _card)
    {
        throw new Exception();
    }
    
    protected virtual void TryBuyFromShop(AbilityData _abilityData)
    {
        throw new Exception("Try buy ability card must be implemented");
    }

    public virtual AbilityData RemoveAbilityFromShop(string _abilityId)
    {
        throw new Exception();
    }
}