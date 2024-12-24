using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    public static CardsManager Instance;

    private List<Card> allCards;
    private List<AbilityCard> allAbilities;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            allCards = Resources.LoadAll<Card>("Cards/").ToList();
            allAbilities = Resources.LoadAll<AbilityCard>("Abilities").ToList();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetAbilityName(int _id)
    {
        return allAbilities.Find(_ability => _ability.Details.Id == _id).name;
    }

    public Sprite GetAbilityImage(int _id)
    {
        return allAbilities.Find(_ability => _ability.Details.Id == _id).Details.Foreground;
    }
    
    public Sprite GetAbilityBackground(int _id)
    {
        return allAbilities.Find(_ability => _ability.Details.Id == _id).Details.Background;
    }
    
    public bool CanAbilityBeGiven(int _id)
    {
        return allAbilities.Find(_ability => _ability.Details.Id == _id).Details.CanBeGivenToPlayer;
    }

    public Card CreateCard(int _cardId)
    {
        Card _desiredCard = GetCardData(_cardId);
        Card _cardObject = Instantiate(_desiredCard);
        return _cardObject;
        
         Card GetCardData(int _cardId) 
         {
            Card _desiredCard = null;
            foreach (var _card in allCards)
            {
                if (_card.Details.Id == _cardId)
                {
                    _desiredCard = _card;
                    break;
                }
            }

            if (_desiredCard == null)
            {
                throw new System.Exception("Cant find prefab for card with id: " + _cardId);
            }

            return _desiredCard;
        }
    }

    public List<Card> Get(FactionSO _faction)
    {
        return allCards.FindAll(_card => _card.Details.Faction == _faction).ToList();
    }
}
