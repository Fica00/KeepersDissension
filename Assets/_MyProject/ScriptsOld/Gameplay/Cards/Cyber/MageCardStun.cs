public class MageCardStun : CardSpecialAbility
{
    private int placeEffectedByStun = -1;
    int range=2;

    private void Start()
    {
        Player.OnStartedTurn += ResetAbilities;
        CanUseAbility = true;
    }

    private void OnDisable()
    {
        Player.OnStartedTurn -= ResetAbilities;
    }

    private void ResetAbilities()
    {
        CanUseAbility = true;
        if (placeEffectedByStun != -1)
        {
            // GameplayManager.Instance.ChangeMovementForCard(placeEffectedByStun, true);
            placeEffectedByStun = -1;
        }
    }

    public override void UseAbility()
    {
        if (Player.Actions <= 0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have enough actions");
            return;
        }

        if (!GameplayManager.Instance.IsMyTurn())
        {
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog("Are you sure that you want to use stun ability?", YesUseStunAbility);
    }

    private void YesUseStunAbility()
    {
        GameplayManager.Instance.SetGameState(GameplayState.UsingSpecialAbility);
        GameplayManager.Instance.SelectPlaceForSpecialAbility(TablePlaceHandler.Id, range, PlaceLookFor.Occupied,
            Card.MovementType, false, LookForCardOwner.Both, SelectedSpot, _ignoreWalls:true);

        void SelectedSpot(int _id)
        {
            GameplayManager.Instance.SetGameState(GameplayState.Playing);
            if (_id == -1)
            {
                DialogsManager.Instance.ShowOkDialog("No enemy cards found");
                return;
            }

            CardBase _cardAtSpot = GameplayManager.Instance.TableHandler.GetPlace(_id).GetCardNoWall();
            if (_cardAtSpot == null)
            {
                DialogsManager.Instance.ShowOkDialog("Select card");
                return;
            }

            Card _card = ((Card)_cardAtSpot);
            if (!_card.IsWarrior())
            {
                DialogsManager.Instance.ShowOkDialog("Select warrior");
                return;
            }

            if (!_card.CanMove)
            {
                DialogsManager.Instance.ShowOkDialog("This card can't move");
                return;
            }
            
            if (Card is not Keeper)
            {
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

            placeEffectedByStun = _id;
            GameplayManager.Instance.TellOpponentSomething("Opponent used Mage stun ability");
            // GameplayManager.Instance.ChangeMovementForCard(_id, false);
            Player.Actions--;
        }
    }
}
