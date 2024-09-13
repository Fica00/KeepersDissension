using System.Collections;
using UnityEngine;

public class ForestWall : WallBase
{
    private void OnEnable()
    {
        CardBase.OnGotDestroyed += CheckCard;
    }

    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= CheckCard;
    }

    private void CheckCard(CardBase _card)
    {
        if (CardBase != _card)
        {
            return;
        }

        if (AttackerPlace == -1)
        {
            return;
        }

        if (_card.My)
        {
            GameplayManager.Instance.PlayAudioOnBoth("Leaves55",CardBase);
        }
        else
        {
            return;
        }

        TablePlaceHandler _cardPlace = GameplayManager.Instance.TableHandler.GetPlace(AttackerPlace);
        if (_cardPlace == null)
        {
            return;
        }
        Card _cardObject = _cardPlace.GetCardNoWall();
        if (_cardObject == null)
        {
            return;
        }
        
        CardBase _attacker = GameplayManager.Instance.TableHandler.GetPlace(AttackerPlace).GetCard();
        if (Collapse.IsActiveForMe)
        {
            if (!_attacker.My)
            {
                DamageAttacker(AttackerPlace);
            }
        }
        else if (Collapse.IsActiveForOpponent)
        {
            if (_attacker.My)
            {
                DamageAttacker(AttackerPlace);
            }
        }

        int _distance = GameplayManager.Instance.TableHandler.DistanceBetweenPlaces(_cardPlace, Card.GetTablePlace());
        if (_distance <= 1)
        {
            StartCoroutine(TryToHeal(_cardObject));
        }

        if (Immunity.IsActiveForMe && _attacker.My)
        {
            return;
        }
        if (Immunity.IsActiveForOpponent && !_attacker.My)
        {
            return;
        }
    }

    IEnumerator TryToHeal(CardBase _cardObject)
    {
        yield return new WaitForSeconds(2);
        if (_cardObject==null)
        {
            yield break;
        }
        Card _card = _cardObject as Card;
        _card.Heal(1);
        GameplayManager.Instance.UpdateHealth(_card.Details.Id,_card.My,(int)_card.Stats.Health);
    }
}