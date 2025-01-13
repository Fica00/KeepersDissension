using UnityEngine;

public class MageCardDelivery : CardSpecialAbility
{
    private GameplayPlayer player;
    
    private void Start()
    {
        if (!GetPlayer().IsMy)
        {
            return;
        }
        
        player = GetPlayer();
        player.OnStartedTurn += ResetAbilities;
    }

    private void OnDisable()
    {
        if (player==null)
        {
            return; 
        }
        
        player.OnStartedTurn -= ResetAbilities;
    }

    private void ResetAbilities()
    {
        foreach (var _effectedCard in Card.CardData.WarriorAbilityData.EffectedCards2)
        {
            Card _card = GameplayManager.Instance.GetCard(_effectedCard);
            if (_card == null)
            {
                continue;
            }

            _card.CardData.HasDelivery = false;
        }

        Card.CardData.WarriorAbilityData.EffectedCards2.Clear();
        SetCanUseAbility(true);
    }


    public override void UseAbility()
    {
        DialogsManager.Instance.ShowYesNoDialog("Are you sure that you want to use deliver ability?", YesUseDeliverAbility);
    }

    private void YesUseDeliverAbility()
    {
        GameplayManager.Instance.SelectPlaceForSpecialAbility(TablePlaceHandler.Id, 1, PlaceLookFor.Occupied, CardMovementType.EightDirections, true,
            LookForCardOwner.Both, SelectedSpot);
    }

    void SelectedSpot(int _placeId)
    {
        Card _card = CanApplyEffect(_placeId);
        if (_card==null)
        {
            return;
        }

        TryToPlayAudio();
        Debug.Log("adding effected card: "+ _card.UniqueId);
        Card.CardData.WarriorAbilityData.EffectedCards2.Add(_card.UniqueId);
        GameplayManager.Instance.TellOpponentSomething("Opponent used his Delivery ability");
        _card.CardData.HasDelivery = true;
        Debug.Log("Setting has delivery");
        
        player.Actions--;
        if (player.Actions>0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    private Card CanApplyEffect(int _placeId)
    {
        if (_placeId == -1)
        {
            DialogsManager.Instance.ShowOkDialog("No cards found");
            return null;
        }

        CardBase _cardAtSpot = GameplayManager.Instance.TableHandler.GetPlace(_placeId).GetCard();
        if (!(_cardAtSpot != null))
        {
            DialogsManager.Instance.ShowOkDialog("Select warrior");
            return null;
        }

        Card _card = ((Card)_cardAtSpot);
        if (!_card.IsWarrior())
        {
            DialogsManager.Instance.ShowOkDialog("Select warrior");
            return null;
        }

        return _card;
    }

    private void TryToPlayAudio()
    {
        if (Card is Keeper)
        {
            return;
        }
        
        if (Card.Details.Faction.IsCyber)
        {
            GameplayManager.Instance.PlayAudioOnBoth("IfYouAreHurt", Card);
        }
        else if (Card.Details.Faction.IsDragon)
        {
            GameplayManager.Instance.PlayAudioOnBoth("IfInDanger", Card);
        }
        else if (Card.Details.Faction.IsForest)
        {
            GameplayManager.Instance.PlayAudioOnBoth("IOfferYouMyProtection", Card);
        }
        else if (Card.Details.Faction.IsSnow)
        {
            GameplayManager.Instance.PlayAudioOnBoth("YouWillRemainProtected", Card);
        }
    }
}
