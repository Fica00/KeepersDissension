using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Revive : AbilityEffect
{
    private GameplayState state;
    private bool forceEnd;

    protected override void ActivateForOwner()
    {
        MoveToActivationField();
        List<CardBase> _validCards = new List<CardBase>();
        foreach (var _validCard in GameplayManager.Instance.MyPlayer.GetAllCardsOfType(CardType.Minion))
        {
            _validCards.Add(_validCard);
        }
        
        foreach (var _validCard in _validCards.ToList())
        {
            if (((Card)_validCard).HasDied)
            {
                continue;
            }

            _validCards.Remove(_validCard);
        }
        
        if (_validCards.Count==0)
        {
            DialogsManager.Instance.ShowOkDialog("You don't have any dead minion");
            RemoveAction();
            OnActivated?.Invoke();
            return;
        }

        StartCoroutine(ActivateRoutine());

        IEnumerator ActivateRoutine()
        {
            state = GameplayManager.Instance.GameState;
            GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
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
            GameplayManager.Instance.GameState = state;
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
}
