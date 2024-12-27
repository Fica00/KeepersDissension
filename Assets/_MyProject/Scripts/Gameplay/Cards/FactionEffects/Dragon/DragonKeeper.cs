using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DragonKeeper : CardSpecialAbility
{
    private GameplayPlayer player;
    
    private void Start()
    {
        if (!GetPlayer().IsMy)
        {
            return;
        }
        
        player = GetPlayer();
        player.OnStartedTurn += EnableAbility;
    }

    private void EnableAbility()
    {
        player.OnStartedTurn -= EnableAbility;
        SetCanUseAbility(true);
    }
    
    public override void UseAbility()
    {
        DialogsManager.Instance.ShowYesNoDialog("Use keepers ultimate?", Activate);
    }
    
    private void Activate()
    {
        List<Card> _availableCards = GetAvailableCards();

        if (_availableCards.Count==0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have any minion that you cloud revive");
            RemoveAction();
            return;
        }

        List<CardBase> _cards = new List<CardBase>();
        foreach (var _availableCard in _availableCards)
        {
            _cards.Add(_availableCard);
        }
        ChooseCardPanel.Instance.ShowCards(_cards,PlaceMinion,true);
    }

    private List<Card> GetAvailableCards()
    {
        List<Card> _availableCards = GameplayManager.Instance.GetAllCardsOfType(CardType.Minion, IsMy);
        foreach (var _availableCard in _availableCards.ToList())
        {
            if (_availableCard.HasDied)
            {
                Debug.Log("1111111");
                continue;
            }

                Debug.Log("222222222");
            _availableCards.Remove(_availableCard);
        }

        return _availableCards;
    }
    
    private void PlaceMinion(CardBase _card)
    {
        if (_card==null)
        {
            RemoveAction();
            return;
        }

        Card _effectedCard = _card as Card;
        if (_effectedCard==null)
        {
            return;
        }
        
        GameplayManager.Instance.MyPlayer.OnStartedTurn += EnableMinion;
        GameplayManager.Instance.BuyMinion(_effectedCard, 0, _placeId: GetPlaceForMinion());
        SetCanUseAbility(false);
        _effectedCard.CardData.IsStunned = true;
        Card.CardData.WarriorAbilityData.EffectedCards.Add(_effectedCard.UniqueId);
        DialogsManager.Instance.ShowOkDialog("This minion will be available next turn");
        RemoveAction();
    }
    
    private void RemoveAction()
    {
        GameplayManager.Instance.MyPlayer.Actions--;
        if (GameplayManager.Instance.MyPlayer.Actions>0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    private int GetPlaceForMinion()
    {
        List<int> _placesNearLifeForce = new List<int>(){10,12,18,17,19,9,13,19,16,23,24,25,26,27};
        int _availablePlaceId=-1;
        foreach (var _placeId in _placesNearLifeForce)
        {
            if (!GameplayManager.Instance.TableHandler.GetPlace(_placeId).IsOccupied)
            {
                _availablePlaceId = _placeId;
                break;
            }
        }

        if (_availablePlaceId==-1)
        {
            for (int _i = 8; _i < 57; _i++)
            {
                if (!GameplayManager.Instance.TableHandler.GetPlace(_i).IsOccupied)
                {
                    _availablePlaceId = _i;
                    break;
                }
            }
        }

        return _availablePlaceId;
    }

    private void EnableMinion()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= EnableMinion;
        Card _card = GameplayManager.Instance.GetCard(Card.CardData.WarriorAbilityData.EffectedCards[0]);
        Card.CardData.WarriorAbilityData.EffectedCards.Clear();
        _card.CardData.IsStunned = false;
    }
}