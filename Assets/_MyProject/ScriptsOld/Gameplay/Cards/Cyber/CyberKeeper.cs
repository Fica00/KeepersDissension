using System.Collections;
using UnityEngine;

public class CyberKeeper : CardSpecialAbility
{
    private CardBase effectedCard;

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

        if (!GameplayManager.Instance.IsMyTurn())
        {
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog("Use keepers ultimate?", Use);

        void Use()
        {
            // GameplayManager.Instance.TellOpponentThatIUsedUltimate();
            GameplayManager.Instance.SetGameState(GameplayState.UsingSpecialAbility);
            GameplayManager.Instance.SelectPlaceForSpecialAbility(
                TablePlaceHandler.Id,
                Card.Range,
                PlaceLookFor.Occupied,
                CardMovementType.EightDirections,
                false,
                LookForCardOwner.Enemy,
                SelectedSpot);

            void SelectedSpot(int _id)
            {
                GameplayManager.Instance.SetGameState(GameplayState.Playing);
                if (_id == -1)
                {
                    Player.Actions--;
                    GameplayManager.Instance.TellOpponentSomething("Opponent wasted his ultimate!");
                    CanUseAbility = false;
                    return;
                }

                CardBase _cardAtSpot = GameplayManager.Instance.TableHandler.GetPlace(_id).GetCard();
                if (!(_cardAtSpot != null))
                {
                    DialogsManager.Instance.ShowOkDialog("Select warrior");
                    return;
                }

                Card _card = ((Card)_cardAtSpot);
                if (!_card.IsWarrior())
                {
                    DialogsManager.Instance.ShowOkDialog("Select warrior");
                    return;
                }

                if (_card.My)
                {
                    DialogsManager.Instance.ShowOkDialog("Select opponents card");
                    return;
                }

                if (GameplayManager.Instance.MyPlayer.Actions <= 0)
                {
                    DialogsManager.Instance.ShowOkDialog("You don't have anymore actions");
                    return;
                }

                Player.Actions--;
                GameplayManager.Instance.TellOpponentSomething("Opponent used his Ultimate!");
                CanUseAbility = false;
                // GameplayManager.Instance.ChangeOwnerOfCard(_id);
                effectedCard = _cardAtSpot;
                CardBase.OnGotDestroyed += CheckIfGotDestroyed;
                Player.OnEndedTurn += ReturnCard;
            }
        }
    }

    private void CheckIfGotDestroyed(CardBase _cardBase)
    {
        if (_cardBase != effectedCard)
        {
            return;
        }

        Unsubscribe();
    }

    private void ReturnCard()
    {
        StartCoroutine(ReturnRoutine());
        IEnumerator ReturnRoutine()
        {
            yield return new WaitForSeconds(2);
            SniperStealth _stealthAbility=null;
            foreach (var _effect in effectedCard.SpecialAbilities)
            {
                if (_effect is SniperStealth _stealth)
                {
                    _stealthAbility = _stealth;
                }
            }
            Unsubscribe();
            int _id = effectedCard.GetTablePlace().Id;
            // GameplayManager.Instance.ChangeOwnerOfCard(_id);

            if (_stealthAbility!=null)
            {
                if (effectedCard.GetTablePlace().Id==0)
                {
                    _stealthAbility.Card.Hide();
                    _stealthAbility.LeaveStealth();
                    StartCoroutine(TellOpponentToActivateStealth(_stealthAbility.StealthedFrom,_stealthAbility
                    .PlaceMinionsFrom));
                    StartCoroutine(ShowCardAgain(_stealthAbility.Card));
                }
            }
        }

        IEnumerator ShowCardAgain(Card _card)
        {
            yield return new WaitForSeconds(2);
            _card.UnHide();
        }
        
        IEnumerator TellOpponentToActivateStealth(int _placeId,int _placeMinionsFrom)
        {
            yield return new WaitForSeconds(1);
            // GameplayManager.Instance.TellOpponentToUseStealth(((Card)effectedCard).Details.Id,_placeId,_placeMinionsFrom);
        }
    }

    private void Unsubscribe()
    {
        CardBase.OnGotDestroyed -= CheckIfGotDestroyed;
        GameplayManager.Instance.MyPlayer.OnEndedTurn -= ReturnCard;
    }
}