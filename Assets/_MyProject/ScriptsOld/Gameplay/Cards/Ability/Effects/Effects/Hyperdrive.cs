using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hyperdrive : AbilityEffect
{
    private AbilityCard abilityCard;
    private GameplayState gameplayState;

    protected override void ActivateForOwner()
    {
        gameplayState = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        List<CardBase> _availableEffects = GetAvailableEffects();
        
        if (_availableEffects.Count==0)
        {
            GameplayManager.Instance.GameState = gameplayState;
            RemoveAction();
            OnActivated?.Invoke();
            DialogsManager.Instance.ShowOkDialog("You don't have any available abilities");
            MoveToActivationField();
            return;
        }
        
        ChooseCardImagePanel.Instance.Show(_availableEffects, _card =>
        {
            ActivateCard(_card);
        });
    }
    
    private void ActivateCard(CardBase _cardBase, bool _end=false)
    {
        GameplayManager.Instance.MyPlayer.Actions++;
        abilityCard = _cardBase as AbilityCard;
        if (_end)
        {
            abilityCard.Effect.OnActivated += EndAbility;
        }
        else
        {
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
                DialogsManager.Instance.ShowOkDialog("You don't have anymore available abilities");
                MoveToActivationField();
                yield break;
            }
        
            ChooseCardImagePanel.Instance.Show(_availableEffects, _card =>
            {
                ActivateCard(_card,true);
            });
        }
    }

    private List<CardBase> GetAvailableEffects()
    {
        List<CardBase> _availableEffects = new();
        foreach (var _ownedAbility in GameplayManager.Instance.MyPlayer.OwnedAbilities)
        {
            if (_ownedAbility.Effect.Cooldown==0)
            {
                continue;
            }
            if (_ownedAbility.Details.Type != AbilityCardType.CrowdControl)
            {
                continue;
            }

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
