using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SniperStealth : CardSpecialAbility
{
    public static int ReturnDiscoveryCardTo;
    private CardBase originalCardMarker;
    private int stealthFromPlace;
    private List<Card> markers = new ();
    private int placeMinionsFrom;

    public int StealthedFrom => stealthFromPlace;
    public int PlaceMinionsFrom => placeMinionsFrom; // used for cyborg ultimate

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

        DialogsManager.Instance.ShowYesNoDialog("Are you sure that you want to use stealth ability?", YesUseStealth);
    }

    private void YesUseStealth()
    {
        GameplayManager.Instance.SelectPlaceForSpecialAbility(TablePlaceHandler.Id, 1, PlaceLookFor.Empty,
            CardMovementType.FourDirections, true, LookForCardOwner.My, GoStealth);
        foreach (var _marker in markers.ToList())
        {
            if (_marker.GetTablePlace()==null)
            {
                markers.Remove(_marker);
            }
        }

        void GoStealth(int _placeId)
        {
            if (_placeId == -1)
            {
                DialogsManager.Instance.ShowOkDialog("There is no empty space");
                return;
            }

            TablePlaceHandler _stealthPlace = GameplayManager.Instance.TableHandler.GetPlace(0);

            if (_stealthPlace.ContainsVoid)
            {
                DialogsManager.Instance.ShowOkDialog("Cant place on void");
                return;
            }

            if (_stealthPlace.IsOccupied)
            {
                DialogsManager.Instance.ShowOkDialog("Stealth tile is already occupied");
                return;
            }

            if (GameplayManager.Instance.MyPlayer.Actions <= 0)
            {
                DialogsManager.Instance.ShowOkDialog("You don't have anymore actions");
                return;
            }

            PlaySoundEffect();
            placeMinionsFrom = Card.GetTablePlace().Id;
            Player.Actions--;
            GameplayManager.Instance.TellOpponentSomething("Opponent used Stealth");
            int _originalPlace = TablePlaceHandler.Id;
            stealthFromPlace = _placeId;
            CanUseAbility = false;

            CardAction _moveAction = new CardAction
            {
                FirstCardId = Card.UniqueId,
                StartingPlaceId = _originalPlace,
                FinishingPlaceId = 0,
                Type = CardActionType.Move,
                Cost = 0,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false
            };

            GameplayManager.Instance.ExecuteCardAction(_moveAction);
            CardBase.OnGotDestroyed += CheckDestroyedCard;
            List<TablePlaceHandler> _changeSprites = new List<TablePlaceHandler>();
            foreach (var _place in GameplayManager.Instance.TableHandler.GetPlacesAround(_originalPlace,
                         CardMovementType.FourDirections, 1, true))
            {
                if (_place.ContainsVoid)
                {
                    continue;
                }
                if (_place.ContainsMarker)
                {
                    _changeSprites.Add(_place);
                    continue;
                }
                if (_place.IsOccupied)
                {
                    continue;
                }

                Card _marker = GameplayManager.Instance.GetCardOfType(CardType.Marker,Player.IsMy);
                markers.Add(_marker);
                if (_marker == null)
                {
                    break;
                }

                if (_place.Id == stealthFromPlace)
                {
                    originalCardMarker = _marker;
                    ((Card)originalCardMarker).CopyStats(Card);
                }

                GameplayManager.Instance.PlaceCard(_marker, _place.Id);
                GameplayManager.Instance.TableHandler.ActionsHandler.ClearPossibleActions();
            }
            
            foreach (var _markerPlace in _changeSprites)
            {
                GameplayManager.Instance.ChangeSprite(_markerPlace.Id,_markerPlace.GetMarker().Details.Id,Card.Details.Faction.Id+1,true);
            }
        }
    }

    public void UseStealth(int _placeId, int _placeMinionsFrom)
    {
        PlaySoundEffect();
        TablePlaceHandler _stealthPlace = GameplayManager.Instance.TableHandler.GetPlace(0);
        foreach (var _marker in markers.ToList())
        {
            if (_marker.GetTablePlace()==null)
            {
                markers.Remove(_marker);
            }
        }
        if (_stealthPlace.IsOccupied)
        {
            DialogsManager.Instance.ShowOkDialog("Stealth tile is already occupied");
            return;
        }

        if (GameplayManager.Instance.MyPlayer.Actions <= 0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have anymore actions");
            return;
        }

        GameplayManager.Instance.TellOpponentSomething("Opponent used Stealth");
        int _originalPlace = TablePlaceHandler.Id;
        stealthFromPlace = _placeId;
        CanUseAbility = false;

        CardAction _moveAction = new CardAction
        {
            FirstCardId = Card.UniqueId,
            StartingPlaceId = _originalPlace,
            FinishingPlaceId = 0,
            Type = CardActionType.Move,
            Cost = 0,
            CanTransferLoot = false,
            Damage = -1,
            CanCounter = false
        };

        GameplayManager.Instance.ExecuteCardAction(_moveAction);
        CardBase.OnGotDestroyed += CheckDestroyedCard;
        foreach (var _place in GameplayManager.Instance.TableHandler.GetPlacesAround(_placeMinionsFrom,
                     CardMovementType.FourDirections, 1, true))
        {
            if (_place.IsOccupied)
            {
                continue;
            }

            Card _marker = GameplayManager.Instance.GetCardOfType(CardType.Marker,Player.IsMy);
            markers.Add(_marker);
            if (_marker == null)
            {
                break;
            }

            if (_place.Id == stealthFromPlace)
            {
                originalCardMarker = _marker;
                ((Card)originalCardMarker).CopyStats(Card);
            }

            GameplayManager.Instance.PlaceCard(_marker, _place.Id);
            GameplayManager.Instance.TableHandler.ActionsHandler.ClearPossibleActions();
        }
    }

    private void PlaySoundEffect()
    {
        if (Card is Keeper)
        {
            return;
        }
        if (Card.Details.Faction.IsCyber)
        {
            GameplayManager.Instance.PlayAudioOnBoth("TheyWillNotSeeMeComing", Card);
        }
        else if (Card.Details.Faction.IsDragon)
        {
            GameplayManager.Instance.PlayAudioOnBoth("IWillFlyIntoTheClouds", Card);
        }
        else if (Card.Details.Faction.IsForest)
        {
            GameplayManager.Instance.PlayAudioOnBoth("StillAndCalm", Card);
        }
        else if (Card.Details.Faction.IsSnow)
        {
            GameplayManager.Instance.PlayAudioOnBoth("IShallMakeMyselfSmall", Card);
        }
    }

    private void CheckDestroyedCard(CardBase _card)
    {
        if (_card is Card _destCard)
        {
            if (markers.Contains(_destCard))
            {
                markers.Remove(_destCard);
            }
        }

        if (_card != originalCardMarker)
        {
            ReturnDiscoveryCardTo = -1;
            return;
        }

        TablePlaceHandler _place = GameplayManager.Instance.TableHandler.GetPlace(stealthFromPlace);
        Card _cardAtLocation = _place.GetCard();

        if (GameplayManager.Instance.LastAction.Type== CardActionType.Move)
        {
            if (!GameplayManager.Instance.IsMyTurn())
            {
                if (CardBase.GetIsMy())
                {
                    // GameplayManager.Instance.RequestResponseAction((CardBase as Card).UniqueId);
                }
            }
            StartCoroutine(ReturnToPlaceRoutine());
        }
        else
        {
            StartCoroutine(DamageStealthedCard());
        }

        Unsubscribe();

        IEnumerator ReturnToPlaceRoutine()
        {
            yield return new WaitForSeconds(0.3f);
            if (_cardAtLocation != null && ReturnDiscoveryCardTo != -1)
            {
                CardAction _moveBackAction = new CardAction
                {
                    FirstCardId = _place.GetCardNoWall().UniqueId,
                    StartingPlaceId = _place.Id,
                    FinishingPlaceId = ReturnDiscoveryCardTo,
                    Type = CardActionType.Move,
                    Cost = 0,
                    CanTransferLoot = false,
                    Damage = -1,
                    CanCounter = false
                };
                GameplayManager.Instance.ExecuteCardAction(_moveBackAction);
            }

            yield return new WaitForSeconds(0.3f);
            CardAction _moveAction = new CardAction
            {
                FirstCardId = Card.UniqueId,
                StartingPlaceId = 0,
                FinishingPlaceId = stealthFromPlace,
                Type = CardActionType.Move,
                Cost = 0,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false
            };

            GameplayManager.Instance.ExecuteCardAction(_moveAction);
            DestroyMarkers(false);
        }

        IEnumerator DamageStealthedCard()
        {
            yield return new WaitForSeconds(1);
            CardAction _damageAction = new CardAction
            {
                StartingPlaceId = GameplayManager.Instance.LastAction.StartingPlaceId,
                FirstCardId = GameplayManager.Instance.LastAction.FirstCardId,
                FinishingPlaceId = Card.GetTablePlace().Id,
                SecondCardId = Card.UniqueId,
                Type = CardActionType.Attack,
                Cost = 0,
                CanTransferLoot = true,
                Damage = GameplayManager.Instance.LastAction.Damage,
                CanCounter = false,
                GiveLoot = false
            };
            GameplayManager.Instance.ExecuteCardAction(_damageAction);
            DestroyMarkers(false);
        }
    }

    public void LeaveStealth()
    {
        DestroyMarkers(true);
        StartCoroutine(LeaveStealthRoutine());

        IEnumerator LeaveStealthRoutine()
        {
            yield return new WaitForSeconds(0.3f);
            CardAction _moveAction = new CardAction
            {
                FirstCardId = Card.UniqueId,
                StartingPlaceId = 0,
                FinishingPlaceId = stealthFromPlace,
                Type = CardActionType.Move,
                Cost = 0,
                CanTransferLoot = false,
                Damage = -1,
                CanCounter = false
            };

            GameplayManager.Instance.TellOpponentSomething("Opponent left Stealth position");
            GameplayManager.Instance.ExecuteCardAction(_moveAction);
        }
    }

    private void DestroyMarkers(bool _unsubscribe)
    {
        if (_unsubscribe)
        {
            Unsubscribe();
        }

        List<int> _places = new List<int>();
        foreach (var _marker in markers)
        {
            TablePlaceHandler _place = _marker.GetTablePlace();
            if (_place == null)
            {
                continue;
            }

            _places.Add(_place.Id);
        }
        // GameplayManager.Instance.TryDestroyMarkers(_places);
        CanUseAbility = true;
    }

    private void Unsubscribe()
    {
        CardBase.OnGotDestroyed -= CheckDestroyedCard;
    }
}
