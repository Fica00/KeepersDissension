public class MageCardStun : CardSpecialAbility
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
        foreach (var _effectedCard in Card.CardData.WarriorAbilityData.EffectedCards)
        {
            Card _card = GameplayManager.Instance.GetCard(_effectedCard);
            if (_card == null)
            {
                continue;
            }

            _card.CardData.IsStunned = false;
        }

        Card.CardData.WarriorAbilityData.EffectedCards.Clear();
        SetCanUseAbility(true);
    }

    public override void UseAbility()
    {
        DialogsManager.Instance.ShowYesNoDialog("Are you sure that you want to use stun ability?", YesUseStunAbility);
    }

    private void YesUseStunAbility()
    {
        if (Card is Keeper)
        {
            SetCanUseAbility(false);
        }
        
        GameplayManager.Instance.SelectPlaceForSpecialAbility(TablePlaceHandler.Id, 2, PlaceLookFor.Occupied, Card.MovementType, false,
            LookForCardOwner.Both, SelectedSpot, _ignoreWalls: true);
    }

    private void SelectedSpot(int _id)
    {
        Card _card = CanPlaceCardOnSpot(_id);
        if (_card==null)
        {
            return;
        }
        
        TryToPlayAudio();
        _card.CardData.IsStunned = true;
        Card.CardData.WarriorAbilityData.EffectedCards.Add(_card.UniqueId);
        GameplayManager.Instance.TellOpponentSomething("Opponent used Mage stun ability");
        player.Actions--;
        if (player.Actions>0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    private Card CanPlaceCardOnSpot(int _id)
    {
        if (_id == -1)
        {
            DialogsManager.Instance.ShowOkDialog("No enemy cards found");
            return null;
        }

        CardBase _cardAtSpot = GameplayManager.Instance.TableHandler.GetPlace(_id).GetCardNoWall();
        if (_cardAtSpot == null)
        {
            DialogsManager.Instance.ShowOkDialog("Select card");
            return null;
        }

        Card _card = (Card)_cardAtSpot;
        if (!_card.IsWarrior())
        {
            DialogsManager.Instance.ShowOkDialog("Select warrior");
            return null;
        }

        if (!_card.CheckCanMove())
        {
            DialogsManager.Instance.ShowOkDialog("This card can't move");
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
            GameplayManager.Instance.PlayAudioOnBoth("DoNotMove", Card);
        }
        else if (Card.Details.Faction.IsDragon)
        {
            GameplayManager.Instance.PlayAudioOnBoth("MoveNoMore", Card);
        }
        else if (Card.Details.Faction.IsForest)
        {
            GameplayManager.Instance.PlayAudioOnBoth("IWillHoldThemHere", Card);
        }
        else if (Card.Details.Faction.IsSnow)
        {
            GameplayManager.Instance.PlayAudioOnBoth("SteadyYourStride", Card);
        }
    }
}