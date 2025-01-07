using System;
using System.Collections.Generic;
using UnityEngine;

public class BomberMinefield : CardSpecialAbility
{
    BomberData bomberData = new();

    private GameplayPlayer player;

    private void OnEnable()
    {
        GameplayManager.OnCardMoved += CheckDestroyedCard;
    }

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
        GameplayManager.OnCardMoved -= CheckDestroyedCard;

        if (player==null)
        {
            return; 
        }
        
        player.OnStartedTurn -= ResetAbilities;
    }

    private void ResetAbilities()
    {
        SetCanUseAbility(true);
    }

    public override void UseAbility()
    {
        if (!CanUse())
        {
            return;
        }

        DialogsManager.Instance.ShowYesNoDialog("Are you sure that you want to use minefield ability?", () =>
        {
            YesUseMinefield(null,true);
        });
    }

    public bool CanUse()
    {
        if (Card.CardData.WarriorAbilityData.BomberData.Count >= 3)
        {
            DialogsManager.Instance.ShowOkDialog("You can have maximum of 3 bombs");
            return false;
        }

        return true;
    }

    public void UseForFree(Action _callBack)
    {
        YesUseMinefield(_callBack,false);
    }

    private void YesUseMinefield(Action _callBack, bool _useActions)
    {
        GameplayManager.Instance.SelectPlaceForSpecialAbility(TablePlaceHandler.Id, 1, PlaceLookFor.Empty, CardMovementType.EightDirections, false,
            LookForCardOwner.My, _place =>
            {
                PlaceBomb(_place, _callBack, _useActions);
            });
    }

    private void PlaceBomb(int _placeId, Action _callBack,bool _useActions)
    {
        bomberData = new BomberData();
        GameplayManager.Instance.CloseAllPanels();

        if (!CanPlaceBomb(_placeId))
        {
            _callBack?.Invoke();
            return;
        }

        TryPlayAudio();
        DoPlaceBomb(_placeId);

        if (_useActions)
        {
            var _player = GetPlayer();
            _player.Actions--;
            if (_player.Actions > 0)
            {
                RoomUpdater.Instance.ForceUpdate();
            }
        }
        
        _callBack?.Invoke();
    }

    private bool CanPlaceBomb(int _placeId)
    {
        if (_placeId == -1)
        {
            DialogsManager.Instance.ShowOkDialog("There is no empty space for bomb");
            return false;
        }

        TablePlaceHandler _tablePlace = GameplayManager.Instance.TableHandler.GetPlace(_placeId);
        if (_tablePlace.ContainsVoid)
        {
            DialogsManager.Instance.ShowOkDialog("Please select unoccupied space");
            return false;
        }

        if (_tablePlace.IsOccupied && !_tablePlace.ContainsMarker)
        {
            DialogsManager.Instance.ShowOkDialog("Please select unoccupied space");
            return false;
        }

        foreach (var _cardData in FirebaseManager.Instance.RoomHandler.BoardData.Cards)
        {
            if (_cardData.WarriorAbilityData == null)
            {
                continue;
            }

            if (_cardData.WarriorAbilityData.BomberData == null)
            {
                continue;
            }

            foreach (var _bomberData in _cardData.WarriorAbilityData.BomberData)
            {
                if (bomberData.BombPlace == -1)
                {
                    continue;
                }

                if (_bomberData.BombPlace != _placeId)
                {
                    continue;
                }
                
                DialogsManager.Instance.ShowOkDialog("This place already contains bomb");
                return false;
            }
        }

        return true;
    }

    private void TryPlayAudio()
    {
        if (Card is Keeper) return;

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

    private void DoPlaceBomb(int _placeId)
    {
        List<string> _markers = new();
        foreach (var _place in GameplayManager.Instance.TableHandler.GetPlacesAround(TablePlaceHandler.Id, CardMovementType.EightDirections))
        {
            if (_place.ContainsVoid)
            {
                continue;
            }

            if (_place.ContainsMarker)
            {
                if (_place.Id == _placeId)
                {
                    bomberData.BombPlace = _place.Id;
                }
                _markers.Add(_place.GetMarker().UniqueId);
                continue;
            }

            if (_place.IsOccupied)
            {
                continue;
            }

            var _marker = GameplayManager.Instance.GetCardOfTypeNotPlaced(CardType.Marker, GetPlayer().IsMy);
            if (_marker == null)
            {
                break;
            }

            _markers.Add(_marker.UniqueId);
            if (_place.Id == _placeId)
            {
                bomberData.BombPlace = _placeId;
            }
            GameplayManager.Instance.PlaceCard(_marker, _place.Id);
            GameplayManager.Instance.TableHandler.ActionsHandler.ClearPossibleActions();
        }


        bomberData.PlacedPlace = _placeId;
        foreach (var _markerPlace in _markers)
        {
            GameplayManager.Instance.ChangeSprite(_markerPlace, Card.Details.Faction.Id + 1, true);
        }

        Card.CardData.WarriorAbilityData.BomberData.Add(bomberData);
    }

    private void CheckDestroyedCard(CardBase _cardBase,int _startingPlaceId,int  _finishingPlaceId)
    {
        if (_cardBase is not Card _)
        {
            return;
        }
        if (Card.CardData.WarriorAbilityData == null)
        {
            return;
        }

        if (Card.CardData.WarriorAbilityData.BomberData == null)
        {
            return;
        }

        foreach (var _bomberData in Card.CardData.WarriorAbilityData.BomberData)
        {
            if (_bomberData.BombPlace == -1)
            {
                continue;
            }

            if (_bomberData.BombPlace != _finishingPlaceId)
            {
                continue;
            }

            List<TablePlaceHandler> _placesAround =
                GameplayManager.Instance.TableHandler.GetPlacesAround(_bomberData.PlacedPlace, CardMovementType.EightDirections);
            foreach (var _placeAround in _placesAround)
            {
                if (!_placeAround.ContainsMarker)
                {
                    continue;
                }
                
                Card _markerCard = _placeAround.GetMarker();
                if (_markerCard.GetIsMy() != IsMy)
                {
                    continue;
                }
                if (_markerCard == null)
                {
                    continue;
                }

                GameplayManager.Instance.DamageCardByAbility(_markerCard.UniqueId,1,null);
            }

            Debug.Log("Bomb exploded");
            GameplayManager.Instance.BombExploded(_cardBase.GetTablePlace().Id, true);
            Card.CardData.WarriorAbilityData.BomberData.Remove(_bomberData);
            break;
        }   
    }
}