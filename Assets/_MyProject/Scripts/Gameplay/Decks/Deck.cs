using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    public List<Card> Cards = new List<Card>();
    public List<AbilityCard> Abilities = new List<AbilityCard>();

    public void AddNewCard(Card _card)
    {
        Cards.Add(_card);
    }
    public void AddNewCard(AbilityCard _card)
    {
        Abilities.Add(_card);
    }

    public Card DrawCard(CardType _type)
    {
        return Cards.Find(_card => _card.Details.Type == _type);
    }

    public Card DrawCard(int _id)
    {
        return Cards.Find(_card => _card.Details.Id == _id);
    }

    public AbilityCard DrawAbility(int _id)
    {
        return Abilities.Find(_ability => _ability.Details.Id == _id);
    }
}