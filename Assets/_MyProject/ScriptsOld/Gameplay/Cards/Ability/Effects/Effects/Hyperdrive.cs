using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hyperdrive : AbilityEffect
{
    private AbilityCard abilityCard;
    private GameplayState gameplayState;

    protected override void ActivateForOwner()
    {
        gameplayState = GameplayManager.Instance.GameState();
        GameplayManager.Instance.SetGameState(GameplayState.UsingSpecialAbility);
        List<CardBase> _availableEffects = GetAvailableEffects();
        
        if (_availableEffects.Count==0)
        {
            GameplayManager.Instance.SetGameState(gameplayState);
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
        GameplayManager.Instance.PlaceAbilityOnTable(abilityCard.UniqueId);
        StartCoroutine(AcitvateAbility());

        IEnumerator AcitvateAbility()
        {
            yield return new WaitForSeconds(1f);
            GameplayManager.Instance.ActivateAbility(abilityCard.UniqueId);
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
                GameplayManager.Instance.SetGameState(gameplayState);
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
        foreach (var _ownedAbility in GameplayManager.Instance.GetOwnedAbilities(true))
        {
            if (_ownedAbility.Cooldown==0)
            {
                continue;
            }
            if (_ownedAbility.Type != AbilityCardType.CrowdControl)
            {
                continue;
            }

            
            _availableEffects.Add(FindObjectsOfType<AbilityCard>().First(_card => _card.UniqueId == _ownedAbility.UniqueId));
        }
        return _availableEffects;
    }

    private void EndAbility()
    {
        MoveToActivationField();
        GameplayManager.Instance.SetGameState(gameplayState);
        RemoveAction();
        OnActivated?.Invoke();
        abilityCard.Effect.OnActivated -= EndAbility;
    }
}
