using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    public static CardsManager Instance;

    private List<Card> allCards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            allCards = Resources.LoadAll<Card>("Cards/").ToList();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<AbilityCard> GetAbilityCards()
    {
        List<AbilityCard> _abilities = Resources.LoadAll<AbilityCard>("Abilities").ToList();
        List<AbilityCard> _abilityObjects = new List<AbilityCard>();
        foreach (var _ability in _abilities)
        {
            _abilityObjects.Add(Instantiate(_ability));
        }

        return _abilityObjects;
    }

    public Card CreateCard(int _cardId, bool _isMy)
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
