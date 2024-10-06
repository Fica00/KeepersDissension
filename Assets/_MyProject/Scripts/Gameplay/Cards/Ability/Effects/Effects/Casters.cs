using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casters : AbilityEffect
{
    private AbilityCard abilityCard;
    private GameplayState gameplayState;
    private List<CardBase> availableEffects = new ();
    public static bool IsActive;

    private void OnEnable()
    {
        IsActive = false;
    }

    public override void ActivateForOwner()
    {
        Debug.Log("----- Activating casters");
        IsActive = true;
        gameplayState = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        availableEffects = GetAvailableEffects();
        
        if (availableEffects.Count==0)
        {
            Debug.Log("----- I don't have available effects for casters");
            UIManager.Instance.ShowOkDialog("You don't have any available abilities, waiting for player to play a card");
            TellOpponentToPlayACard();
            return;
        }

        if (GameplayManager.Instance.MyPlayer.Actions==0)
        {
            Debug.Log("----- I don't have available actions for casters");
            UIManager.Instance.ShowOkDialog("You don't have enough actions, waiting for player to play a card");
            TellOpponentToPlayACard();
            return;
        }
        
        Debug.Log("----- Showing available effects: "+availableEffects.Count);
        ChooseCardImagePanel.Instance.Show(availableEffects, _card =>
        {
            ActivateCard(_card, TellOpponentToPlayACard);
        });
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
    }
    
    public override void ActivateForOther()
    {
        gameplayState = GameplayManager.Instance.GameState;
        GameplayManager.Instance.GameState = GameplayState.UsingSpecialAbility;
        AbilityCard.ActiveDisplay.gameObject.SetActive(true);
        IsActive = true;
    }

    private void TellOpponentToPlayACard()
    {
        Debug.Log("----- Casters tell opponent to activate a card");
        GameplayUI.Instance.ActionAndTurnDisplay.ShowAction(1,false);
        GameplayManager.Instance.MyPlayer.OnEndedTurn?.Invoke();
        GameplayManager.Instance.TellOpponentToPlaceFirstCardCasters();
    }

    public void ActivateForOpponentFirst()
    {
        GameplayUI.Instance.ActionAndTurnDisplay.ShowAction(1,true);
        GameplayManager.Instance.OpponentPlayer.OnEndedTurn?.Invoke();
        availableEffects = GetAvailableEffects();
        if (availableEffects.Count==0)
        {
            UIManager.Instance.ShowOkDialog("You don't have any available abilities, waiting for opponent to place card");
            TellOpponentThatIPlacedCard();
            return;
        }
        
        GameplayManager.Instance.MyPlayer.Actions++;
        ChooseCardImagePanel.Instance.Show(availableEffects, _card =>
        {
            ActivateCard(_card, TellOpponentThatIPlacedCard);
        });
    }

    private void TellOpponentThatIPlacedCard()
    {
        GameplayUI.Instance.ActionAndTurnDisplay.ShowAction(1,false);
        GameplayManager.Instance.MyPlayer.OnEndedTurn?.Invoke();
        GameplayManager.Instance.OpponentPlacedFirstAbilityForCasters();
    }

    private void TellOpponentThatIPlacedSecondCard()
    {
        Debug.Log("----- I placed seconds card");
        GameplayManager.Instance.MyPlayer.OnEndedTurn?.Invoke();
        GameplayManager.Instance.FinishCasters();
        Finish();
        StartCoroutine(FinishRoutine());
        IEnumerator FinishRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            GameplayManager.Instance.EndTurn();
        }
    }

    public void FinishCasters()
    {
        GameplayManager.Instance.OpponentPlayer.OnEndedTurn?.Invoke();
        Finish();
        OnActivated?.Invoke();
    }

    private void Finish()
    {
        GameplayManager.Instance.GameState = gameplayState;
        AbilityCard.ActiveDisplay.gameObject.SetActive(false);
        IsActive = false;
    }
    
    public void ActivateForMe()
    {
        GameplayUI.Instance.ActionAndTurnDisplay.ShowAction(1,true);
        GameplayManager.Instance.OpponentPlayer.OnEndedTurn?.Invoke();
        availableEffects = GetAvailableEffects();
        
        if (availableEffects.Count==0)
        {
            UIManager.Instance.ShowOkDialog("You don't have any available abilities, waiting for player to play a card");
            TellOpponentThatIPlacedSecondCard();
            return;
        }

        ChooseCardImagePanel.Instance.Show(availableEffects, _card =>
        {
            ActivateCard(_card, TellOpponentThatIPlacedSecondCard);
        });
    }
    
    private void ActivateCard(CardBase _cardBase, Action _callBack)
    {
        abilityCard = _cardBase as AbilityCard;
        abilityCard.Effect.OnActivated += Callback;
        StartCoroutine(AcitvateAbility());

        IEnumerator AcitvateAbility()
        {
            yield return new WaitForSeconds(1f);
            GameplayManager.Instance.ActivateAbility(abilityCard.Details.Id);
        }

        void Callback()
        {
            abilityCard.Effect.OnActivated -= Callback;
            StartCoroutine(DelayCallBack());
            IEnumerator DelayCallBack()
            {
                yield return new WaitForSeconds(1);
                _callBack?.Invoke();
            }
        }
    }
    
    private List<CardBase> GetAvailableEffects()
    {
        List<CardBase> _availableEffects = new();
        foreach (var _ownedAbility in GameplayManager.Instance.MyPlayer.OwnedAbilities)
        {
            if (_ownedAbility.Details.Type != AbilityCardType.CrowdControl)
            {
                continue;
            }

            if (_ownedAbility.GetTablePlace() == null)
            {
                continue;
            }

            if (_ownedAbility.GetTablePlace().Id == -1)
            {
                continue;
            }
            
            _availableEffects.Add(_ownedAbility);
        }
        return _availableEffects;
    }

}
