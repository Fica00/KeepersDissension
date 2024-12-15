using UnityEngine;

public class ScalerLeapfrog : CardSpecialAbility
{
    //nothing to do here

    private void OnEnable()
    {
        CardBase.OnGotDestroyed += CheckForResponseAction;
    }

    private void OnDisable()
    {
        CardBase.OnGotDestroyed -= CheckForResponseAction;
    }

    private void CheckForResponseAction(CardBase _card)
    {
        if (!CardBase.GetIsMy())
        {
            return;
        }

        if (_card == null)
        {
            return;
        }

        if (TablePlaceHandler==null)
        {
            return;
        }

        var _tablePlace = _card.GetTablePlace();

        if (_tablePlace == null)
        {
            return;
        }

        if (_tablePlace.Id != TablePlaceHandler.Id)
        {
            return;
        }

        if (GameplayManager.Instance.GameState != GameplayState.Waiting)
        {
            return;
        }

        if (_card==Card)
        {
            Debug.Log("Destroyed this card");
            return;
        }

        // GameplayManager.Instance.RequestResponseAction(((Card)CardBase).UniqueId);
    }
}