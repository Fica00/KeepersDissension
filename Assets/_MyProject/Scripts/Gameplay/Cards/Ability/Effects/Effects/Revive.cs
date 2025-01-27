using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Revive : AbilityEffect
{
    private bool forceEnd;

    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        List<CardBase> _validCards = GameplayManager.Instance.GetDeadMinions(true).Cast<CardBase>().ToList();
        
        if (_validCards.Count==0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have any dead minion");
            OnActivated?.Invoke();
            RemoveAction();
            return;
        }

        StartCoroutine(ActivateRoutine(_validCards));
    }
    
    IEnumerator ActivateRoutine(List<CardBase> _validCards)
    {
        bool _continue;
        for (int _i = 0; _i < 3; _i++)
        {
            if (forceEnd)
            {
                forceEnd = false;
                break;
            }
            _continue = false;
            if (_validCards.Count==0)
            {
                DialogsManager.Instance.ShowOkDialog("You revived all dead minions");
                break;
            }
                
            ChooseCardImagePanel.Instance.Show(_validCards,PlaceMinion,true,true);
            yield return new WaitUntil(() => _continue);
        }
        
        RemoveAction();
        OnActivated?.Invoke();

        void Continue()
        {
            _continue = true;
        }

        void PlaceMinion(CardBase _card)
        {
            if (_card==null)
            {
                forceEnd = true;
                Continue();
                return;
            }
            
            GameplayManager.Instance.BuyMinion(_card, 0, Continue);
            _validCards.Remove(_card);
        }
    }
}