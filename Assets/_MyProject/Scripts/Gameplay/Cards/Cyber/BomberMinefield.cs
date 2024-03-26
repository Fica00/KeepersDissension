using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BomberMinefield : CardSpecialAbility
{
    private Dictionary<CardBase,List<Card>> markers = new ();
    public static List<CardBase> BombMarkers;


    private void OnEnable()
    {
        CardBase.OnGotDestroyed += CheckDestroyedCard;
        GameplayManager.OnFoundBombMarker += DestroyRestOfCards;
    }

    private void DestroyRestOfCards(CardBase _card)
    {
        if (!markers.ContainsKey(_card))
        {
            return;
        }

        List<int> _markerPlaces = new List<int>();
        foreach (var _marker in markers[_card].ToList())
        {
            if (_marker==_card)
            {
                continue;
            }

            TablePlaceHandler _tablePlace = _marker.GetTablePlace();
            if (_tablePlace==null)
            {
                continue;
            }
            _markerPlaces.Add(_tablePlace.Id);
        }
        
        GameplayManager.Instance.TryDestroyMarkers(_markerPlaces);
    }

    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= CheckDestroyedCard;
        GameplayManager.OnFoundBombMarker -= DestroyRestOfCards;
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

        if (BombMarkers.Count >= 3)
        {
            UIManager.Instance.ShowOkDialog("You can have maximum of 3 bombs");
            return;
        }

        UIManager.Instance.ShowYesNoDialog("Are you sure that you want to use minefield ability?", () =>
        {
            YesUseMinefield();
        }, () =>
        {
            OnActivated?.Invoke();
        });
    }

    public void UseAbilityFree()
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

        if (BombMarkers.Count >= 3)
        {
            UIManager.Instance.ShowOkDialog("You can have maximum of 3 bombs");
            return;
        }

        YesUseMinefield(false);
    }
    
    private void YesUseMinefield(bool _checkActions=true,Action _callBack=null)
    {
        GameplayManager.Instance.SelectPlaceForSpecialAbility(TablePlaceHandler.Id, 1, PlaceLookFor.Empty,
            CardMovementType.EightDirections, false, LookForCardOwner.My, PlaceBomb);
        
        void PlaceBomb(int _placeId)
        {
            GameplayManager.Instance.CloseAllPanels();
            if (_placeId == -1)
            {
                UIManager.Instance.ShowOkDialog("There is no empty space for bomb");
                OnActivated?.Invoke();
                return;
            }
            
            TablePlaceHandler _tablePlace = GameplayManager.Instance.TableHandler.GetPlace(_placeId);
            if (_tablePlace.IsOccupied && !_tablePlace.ContainsMarker)
            {
                UIManager.Instance.ShowOkDialog("Please select unoccupied space");
                OnActivated?.Invoke();
                return;
            }

            foreach (var _marker in BombMarkers)
            {
                if (_marker.GetTablePlace().Id==_placeId)
                {
                    UIManager.Instance.ShowOkDialog("This place already contains bomb");
                    OnActivated?.Invoke();
                    return;
                }
            }
            
            if (Player.Actions<=0 && !Casters.IsActive && _checkActions)
            {
                UIManager.Instance.ShowOkDialog("You don't have anymore actions");
                OnActivated?.Invoke();
                return;
            }

            if (Card is not Keeper)
            {
                if (Card.Details.Faction.IsCyber)
                {
                    GameplayManager.Instance.PlayAudioOnBoth("CyborgBomberMinefield", Card);
                }
                else if (Card.Details.Faction.IsDragon)
                {
                    GameplayManager.Instance.PlayAudioOnBoth("MindYourStep", Card);
                }
                else if (Card.Details.Faction.IsForest)
                {
                    GameplayManager.Instance.PlayAudioOnBoth("IHaveSetTheTrap", Card);
                }
                else if (Card.Details.Faction.IsSnow)
                {
                    GameplayManager.Instance.PlayAudioOnBoth("ThisIsWhatILiveFor", Card);
                }
            }
            
            if (_checkActions)
            {
                Player.Actions--;
            }
            
            List<Card> _markers = new List<Card>();
            CardBase _bombMarker = null;
            List<TablePlaceHandler> _placesWithMarkers = new List<TablePlaceHandler>();

            foreach (var _place in GameplayManager.Instance.TableHandler.GetPlacesAround(TablePlaceHandler.Id,
                         CardMovementType.EightDirections, 1, false))
            {
                Card _marker;
                if (_place.ContainsMarker)
                {
                    _marker = _place.GetMarker();
                    _placesWithMarkers.Add(_place);
                    if (_place.Id == _placeId)
                    {
                        _bombMarker = _marker;
                        BombMarkers.Add(_marker);
                    }
                    continue;
                }
                
                if (_place.IsOccupied)
                {
                    continue;
                }
                
                _marker = Player.GetCard(CardType.Marker);
                _markers.Add(_marker);
                if (_marker == null)
                {
                    break;
                }

                if (_place.Id == _placeId)
                {
                    Debug.Log("Added",gameObject);
                    _bombMarker = _marker;
                    BombMarkers.Add(_marker);
                }

                
                GameplayManager.Instance.PlaceCard(_marker, _place.Id);
                GameplayManager.Instance.TableHandler.ActionsHandler.ClearPossibleActions();
            }

            
            foreach (var _markerPlace in _placesWithMarkers)
            {
                GameplayManager.Instance.ChangeSprite(_markerPlace.Id,_markerPlace.GetMarker().Details.Id,Card.Details.Faction.Id+1,true);
            }
            
            _callBack?.Invoke();
            OnActivated?.Invoke();
            markers.Add(_bombMarker,_markers);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (var (_bomMarker,_restMarkers) in markers)
            {
                Debug.Log(_bomMarker.name,_bomMarker.gameObject);
                foreach (var _marker in _restMarkers)
                {
                    Debug.Log(_marker.name,_marker.gameObject);
                }
            }
        }
    }

    private void CheckDestroyedCard(CardBase _card)
    {
        if (_card is Card _destCard)
        {
            foreach (var _markers in markers.Values.ToList())
            {
                if (_markers.Contains(_destCard))
                {
                    markers.Remove(_destCard);
                }
            }
        }

        if (!BombMarkers.Contains(_card))
        {
            return;
        }

        BombMarkers.Remove(_card);
        
        GameplayManager.Instance.BombExploded(_card.GetTablePlace().Id);
    }
}