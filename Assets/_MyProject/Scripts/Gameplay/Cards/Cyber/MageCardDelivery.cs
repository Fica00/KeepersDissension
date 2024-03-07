public class MageCardDelivery : CardSpecialAbility
{
    private int cardEffectedByDeliver = -1;

    private void Start()
    {
        Player.OnStartedTurn += ResetAbilities;
    }

    private void OnDisable()
    {
        Player.OnStartedTurn -= ResetAbilities;
    }

    private void ResetAbilities()
    {
        if (cardEffectedByDeliver != -1)
        {
            GameplayManager.Instance.ChangeCanFlyToDodge(cardEffectedByDeliver,false);
            cardEffectedByDeliver = -1;
        }
    }


    public override void UseAbility()
    {
        if (Player.Actions<=0)
        {
            UIManager.Instance.ShowOkDialog("You don't have enough actions");
            return;
        }

        if (!GameplayManager.Instance.MyTurn)
        {
            return;
        }
        
        UIManager.Instance.ShowYesNoDialog("Are you sure that you want to use deliver ability?", YesUseDeliverAbility);
    }

    private void YesUseDeliverAbility()
    {
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        GameplayManager.Instance.SelectPlaceForSpecialAbility(
            TablePlaceHandler.Id,
            1,
            PlaceLookFor.Occupied,
            CardMovementType.EightDirections,
            true,
            LookForCardOwner.My,
            SelectedSpot);
        
        void SelectedSpot(int _id)
        {
            GameplayManager.Instance.GameState = GameplayState.Playing;
            if (_id==-1)
            {
                UIManager.Instance.ShowOkDialog("No cards found");
                return;
            }
            
            CardBase _cardAtSpot = GameplayManager.Instance.TableHandler.GetPlace(_id).GetCard();
            if (!(_cardAtSpot != null))
            {
                UIManager.Instance.ShowOkDialog("Select warrior");
                return;
            }

            Card _card = ((Card)_cardAtSpot);
            if (!_card.IsWarrior())
            {
                UIManager.Instance.ShowOkDialog("Select warrior");
                return;
            }

            if (!_card.My)
            {
                UIManager.Instance.ShowOkDialog("Select friendly card");
                return;
            }
            
            if (Card is not Keeper)
            {
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
            
            Player.Actions--;
            cardEffectedByDeliver = _card.Details.Id;
            GameplayManager.Instance.TellOpponentSomething("Opponent used his Delivery ability");
            GameplayManager.Instance.ChangeCanFlyToDodge(cardEffectedByDeliver,true);
        }
    }
}
