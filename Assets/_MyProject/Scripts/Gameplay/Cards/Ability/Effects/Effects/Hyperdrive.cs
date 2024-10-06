using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hyperdrive : AbilityEffect
{
    private AbilityCard abilityCard;
    private GameplayState gameplayState;
    
    public override void ActivateForOwner()
    {
        Debug.Log("----- Activating Hyperdrive");
        gameplayState = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        List<CardBase> _availableEffects = GetAvailableEffects();
        
        if (_availableEffects.Count==0)
        {
            GameplayManager.Instance.GameState = gameplayState;
            RemoveAction();
            OnActivated?.Invoke();
            UIManager.Instance.ShowOkDialog("You don't have any available abilities");
            MoveToActivationField();
            return;
        }
        
        Debug.Log("----- Showing cards to activate for hyperdrive");
        ChooseCardImagePanel.Instance.Show(_availableEffects, _card =>
        {
            ActivateCard(_card);
        });
    }

    private void ActivateNextCard()
    {
        StartCoroutine(ActivateRoutine());
        
        IEnumerator ActivateRoutine()
        {
            yield return new WaitForSeconds(1);
            abilityCard.Effect.OnActivated -= ActivateNextCard;
            List<CardBase> _availableEffects = GetAvailableEffects();
            
            if (_availableEffects.Count==0)
            {
                GameplayManager.Instance.GameState = gameplayState;
                RemoveAction();
                OnActivated?.Invoke();
                UIManager.Instance.ShowOkDialog("You don't have anymore available abilities");
                MoveToActivationField();
                yield break;
            }
        
            ChooseCardImagePanel.Instance.Show(_availableEffects, _card =>
            {
                ActivateCard(_card,true);
            });
        }
    }
    
    private void ActivateCard(CardBase _cardBase, bool _end=false)
    {
        Debug.Log("----- Activating ability for hyperdrive");
        GameplayManager.Instance.MyPlayer.Actions++;
        abilityCard = _cardBase as AbilityCard;
        if (_end)
        {
            Debug.Log("----- Ending hyperdrive");
            abilityCard.Effect.OnActivated += EndAbility;
        }
        else
        {
            Debug.Log("----- Choosing next card for hyperdrive");
            abilityCard.Effect.OnActivated += ActivateNextCard;
        }
        GameplayManager.Instance.PlaceAbilityOnTable(abilityCard.Details.Id);
        StartCoroutine(AcitvateAbility());

        IEnumerator AcitvateAbility()
        {
            yield return new WaitForSeconds(1f);
            GameplayManager.Instance.ActivateAbility(abilityCard.Details.Id);
        }
    }

    private List<CardBase> GetAvailableEffects()
    {
        List<CardBase> _availableEffects = new();
        foreach (var _ownedAbility in GameplayManager.Instance.MyPlayer.OwnedAbilities)
        {
            if (_ownedAbility.Effect.Cooldown==0)
            {
                Debug.Log("----- I don't have availble effects");
                continue;
            }
            if (_ownedAbility.Details.Type != AbilityCardType.CrowdControl)
            {
                Debug.Log("----- This effect is not cc");
                continue;
            }

            Debug.Log("----- Adding as possible effect");
            _availableEffects.Add(_ownedAbility);
        }
        return _availableEffects;
    }

    private void EndAbility()
    {
        MoveToActivationField();
        GameplayManager.Instance.GameState = gameplayState;
        RemoveAction();
        OnActivated?.Invoke();
        abilityCard.Effect.OnActivated -= EndAbility;
    }
}
