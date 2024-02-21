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

        int _distance = GameplayManager.Instance.TableHandler.DistanceBetweenPlaces(_cardPlace, Card.GetTablePlace());
        if (_distance <= 1)
        {
            StartCoroutine(TryToHeal(_cardObject));
        }

        if (!CardBase.My)
        {
            if (Collapse.IsActive)
            {
                DamageAttacker(AttackerPlace);
            }
        }

        if (Immunity.IsActiveForMe || Immunity.IsActiveForOpponent)
        {
            return;
        }

        if (Collapse.IsActive)
        {
            DamageAttacker(AttackerPlace);
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
    }
}