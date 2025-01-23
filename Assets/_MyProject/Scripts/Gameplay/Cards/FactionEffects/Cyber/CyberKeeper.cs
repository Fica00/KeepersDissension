public class CyberKeeper : CardSpecialAbility
{
    private GameplayPlayer player;

    private bool wasResponseAction;

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
        GameplayManager.Instance.SelectPlaceForSpecialAbility(TablePlaceHandler.Id, Card.Range, PlaceLookFor.Both,
            CardMovementType.EightDirections, false, LookForCardOwner.Enemy, SelectedSpot);
    }

    private void SelectedSpot(int _placeId)
    {
        if (_placeId == -1)
        {
            Waste();
            DialogsManager.Instance.ShowOkDialog("Ultimate was used but no enemy Warrior nearby to be selected");
            return;
        }

        CardBase _cardAtSpot = GameplayManager.Instance.TableHandler.GetPlace(_placeId).GetCard();
        if (!(_cardAtSpot != null))
        {
            Waste();
            DialogsManager.Instance.ShowOkDialog("Select warrior");
            return;
        }

        Card _card = ((Card)_cardAtSpot);
        
        if (!_card.IsWarrior())
        {
            Waste();
            DialogsManager.Instance.ShowOkDialog("Select warrior, wasted ultimate");
            return;
        }

        if (_card.My)
        {
            Waste();
            DialogsManager.Instance.ShowOkDialog("Select opponents card, wasted ultimate");
            return;
        }

        GameplayManager.Instance.TellOpponentSomething("Opponent used his Ultimate!");
        SetCanUseAbility(false);
        GameplayManager.Instance.ChangeOwnerOfCard(_card.UniqueId);
        Card.CardData.WarriorAbilityData.EffectedCards.Add(_card.UniqueId);
        wasResponseAction = GameplayManager.Instance.IsMyResponseAction();
        if (wasResponseAction)
        {
            GameplayManager.Instance.MyPlayer.OnStartedTurn += ReturnCard;
        }
        else
        {
            GameplayManager.Instance.MyPlayer.OnEndedTurn += ReturnCard;
        }
        RemoveAction();
    }

    private void Waste()
    {
        GameplayManager.Instance.TellOpponentSomething("Opponent wasted his ultimate");
        RemoveAction();
        SetCanUseAbility(false);
    }

    private void RemoveAction()
    {
        GameplayManager.Instance.MyPlayer.Actions--;
        if (GameplayManager.Instance.MyPlayer.Actions > 0)
        {
            RoomUpdater.Instance.ForceUpdate();
        }
    }

    private void ReturnCard()
    {
        if (wasResponseAction)
        {
            GameplayManager.Instance.MyPlayer.OnStartedTurn -= ReturnCard;
        }
        else
        {
            GameplayManager.Instance.MyPlayer.OnEndedTurn -= ReturnCard;
        }
        GameplayManager.Instance.ChangeOwnerOfCard(Card.CardData.WarriorAbilityData.EffectedCards[0]);
        Card.CardData.WarriorAbilityData.EffectedCards.Clear();
        RoomUpdater.Instance.ForceUpdate(); 
    }
}