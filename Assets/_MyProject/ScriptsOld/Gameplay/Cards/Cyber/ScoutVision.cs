using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutVision : CardSpecialAbility
{
    private void Start()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn += UseVision;
        UseVision();
    }

    private void OnEnable()
    {
        GameplayManager.OnCardAttacked += UseVision;
        GameplayManager.OnCardMoved += UseVision;
        GameplayManager.OnPlacedCard += UseVision;
    }
    
    private void OnDisable()
    {
        GameplayManager.Instance.MyPlayer.OnStartedTurn -= UseVision;
        GameplayManager.OnCardAttacked -= UseVision;
        GameplayManager.OnCardMoved -= UseVision;
        GameplayManager.OnPlacedCard -= UseVision;
    }
    
    private void UseVision(CardBase _arg1, CardBase _arg2, int _arg3)
    {
        UseVision();
    }

    private void UseVision(CardBase _arg1, int _arg2, int _arg3, bool _)
    {
        UseVision();
    }

    private void UseVision(CardBase _obj)
    {
        UseVision();
    }

    private void UseVision()
    {
        if (Card==null)
        {
            return;
        }

        CardPlace _cardPlace = GameplayManager.Instance.CardPlace(Card);
        if (_cardPlace != CardPlace.Table)
        {
            return;
        }

        StartCoroutine(UseVisionRoutine());

        IEnumerator UseVisionRoutine()
        {
            yield return new WaitForSeconds(1);
            if (Card.GetTablePlace()==null)
            {
                yield break;
            }
            List<TablePlaceHandler> _places = GameplayManager.Instance.TableHandler.GetPlacesAround(
                Card.GetTablePlace().Id, CardMovementType.EightDirections, 1, false);
            List<int> _markerPositions = new List<int>();
            foreach (var _place in _places)
            {
                if (!_place.ContainsMarker)
                {
                    continue;
                }

                if (_place.GetMarker().My == Card.My)
                {
                    continue;
                }

                if (_place.GetMarker().IsVoid)
                {
                    continue;
                }

                _markerPositions.Add(_place.Id);
            }

            if (_markerPositions.Count==0)
            {
                yield break;
            }
            
            // GameplayManager.Instance.CheckForBombInMarkers(_markerPositions,DestroyMarker);
        }
    }
}