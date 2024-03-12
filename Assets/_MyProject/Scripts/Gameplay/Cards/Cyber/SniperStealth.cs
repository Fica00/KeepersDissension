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
            UIManager.Instance.ShowOkDialog("You don't have enough actions");
            return;
        }

        if (!GameplayManager.Instance.MyTurn)
        {
            return;
        }

        UIManager.Instance.ShowYesNoDialog("Are you sure that you want to use stealth ability?", YesUseStealth);
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
                UIManager.Instance.ShowOkDialog("There is no empty space");
                return;
            }

            TablePlaceHandler _stealthPlace = GameplayManager.Instance.TableHandler.GetPlace(0);

            if (_stealthPlace.IsOccupied)
            {
                UIManager.Instance.ShowOkDialog("Stealth tile is already occupied");
                return;
            }

            if (GameplayManager.Instance.MyPlayer.Actions <= 0)
            {
                UIManager.Instance.ShowOkDialog("You don't have anymore actions");
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
                FirstCardId = Card.Details.Id,
                StartingPlaceId = _originalPlace,
                FinishingPlaceId = 0,
                Type = CardActionType.Move,
                Cost = 0,
                IsMy = true,
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
                if (_place.ContainsMarker)
                {
                    Debug.Log("Added");
                    _changeSprites.Add(_place);
                    continue;
                }
                if (_place.IsOccupied)
                {
                    continue;
                }

                Card _marker = Player.GetCard(CardType.Marker);
                markers.Add(_marker);
                if (_marker == null)
                {
                    break;
                }

                if (_place.Id == stealthFromPlace)
                {
                    originalCardMarker = _marker;
                    ((Card)originalCardMarker).Stats = Card.Stats;
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
            UIManager.Instance.ShowOkDialog("Stealth tile is already occupied");
            return;
        }

        if (GameplayManager.Instance.MyPlayer.Actions <= 0)
        {
            UIManager.Instance.ShowOkDialog("You don't have anymore actions");
            return;
        }

        GameplayManager.Instance.TellOpponentSomething("Opponent used Stealth");
        int _originalPlace = TablePlaceHandler.Id;
        stealthFromPlace = _placeId;
        CanUseAbility = false;

        CardAction _moveAction = new CardAction
        {
            FirstCardId = Card.Details.Id,
            StartingPlaceId = _originalPlace,
            FinishingPlaceId = 0,
            Type = CardActionType.Move,
            Cost = 0,
            IsMy = true,
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

            Card _marker = Player.GetCard(CardType.Marker);
            markers.Add(_marker);
            if (_marker == null)
            {
                break;
            }

            if (_place.Id == stealthFromPlace)
            {
                originalCardMarker = _marker;
                ((Card)originalCardMarker).Stats = Card.Stats;
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
            if (!GameplayManager.Instance.MyTurn)
            {
                if (CardBase.My)
                {
                    GameplayManager.Instance.RequestResponseAction((CardBase as Card).Details.Id);
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
                    FirstCardId = _place.GetCardNoWall().Details.Id,
                    StartingPlaceId = _place.Id,
                    FinishingPlaceId = ReturnDiscoveryCardTo,
                    Type = CardActionType.Move,
                    Cost = 0,
                    IsMy = false,
                    CanTransferLoot = false,
                    Damage = -1,
                    CanCounter = false
                };
                GameplayManager.Instance.ExecuteCardAction(_moveBackAction);
            }

            yield return new WaitForSeconds(0.3f);
            CardAction _moveAction = new CardAction
            {
                FirstCardId = Card.Details.Id,
                StartingPlaceId = 0,
                FinishingPlaceId = stealthFromPlace,
                Type = CardActionType.Move,
                Cost = 0,
                IsMy = true,
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
                SecondCardId = Card.Details.Id,
                Type = CardActionType.Attack,
                Cost = 0,
                IsMy = true,
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
                FirstCardId = Card.Details.Id,
                StartingPlaceId = 0,
                FinishingPlaceId = stealthFromPlace,
                Type = CardActionType.Move,
                Cost = 0,
                IsMy = true,
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
        GameplayManager.Instance.TryDestroyMarkers(_places);
        CanUseAbility = true;
    }

    private void Unsubscribe()
    {
        CardBase.OnGotDestroyed -= CheckDestroyedCard;
    }
}
