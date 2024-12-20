using System.Collections.Generic;
using System.Linq;

public class DragonKeeper : CardSpecialAbility
{
    private Card effectedCard;

    public override void UseAbility()
    {
        if (!CanUseAbility)
        {
            return;
        }

        if (Player.Actions <= 0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have enough actions");
            return;
        }

        if (!GameplayManager.Instance.MyTurn)
        {
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog("Use keepers ultimate?", Use);

        void Use()
        {
            // GameplayManager.Instance.TellOpponentThatIUsedUltimate();
            CanUseAbility = false;

            List<Card> _availableCards = GameplayManager.Instance.GetAllCardsOfType(CardType.Minion,true);
            foreach (var _availableCard in _availableCards.ToList())
            {
                if (_availableCard.HasDied)
                {
                    continue;
                }

                _availableCards.Remove(_availableCard);
            }

            if (_availableCards.Count==0)
            {
                DialogsManager.Instance.ShowOkDialog("You don't have any minion that you cloud revive");
                GameplayManager.Instance.MyPlayer.Actions--;
                return;
            }

            List<CardBase> _cards = new List<CardBase>();
            foreach (var _availableCard in _availableCards)
            {
                _cards.Add(_availableCard);
            }
            ChooseCardPanel.Instance.ShowCards(_cards,SaveMinion,true);
            
            void SaveMinion(CardBase _card)
            {
                if (_card==null)
                {
                    GameplayManager.Instance.MyPlayer.Actions--;
                    return;
                }

                effectedCard = _card as Card;
                GameplayManager.Instance.MyPlayer.OnStartedTurn += EnableMinion;
                GameplayManager.Instance.MyPlayer.Actions--;
                GameplayManager.Instance.BuyMinion(effectedCard, 0);
                effectedCard.SetCanBeUsed(false);
                DialogsManager.Instance.ShowOkDialog("This minion will be available next turn");
            }
        }
    }

    private void PlaceMinion()
    {
        List<int> _placesNearLifeForce = new List<int> {10,12,18,17,19,9,13,19,16,23,24,25,26,27};
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

        GameplayManager.Instance.PlaceCard(effectedCard,_availablePlaceId);
    }

    private void EnableMinion()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= EnableMinion;
        effectedCard.SetCanBeUsed(true);
        effectedCard = null;
    }

    private void OnDisable()
    {
        if (effectedCard!=null)
        {
            GameplayManager.Instance.MyPlayer.OnStartedTurn -= PlaceMinion;
        }
    }
}