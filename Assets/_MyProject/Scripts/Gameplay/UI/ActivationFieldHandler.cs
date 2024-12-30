using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivationFieldHandler : MonoBehaviour
{
    public static Action OnShowed;
    public static Action OnHided;
    
    [SerializeField] private GameObject holder;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform cardsHolder;
    [SerializeField] private ActivationFiledAbilityDisplay activationFiledAbilityDisplay;
    private List<ActivationFiledAbilityDisplay> shownCards = new();
    
    private void OnEnable()
    {
        ActivationFiledAbilityDisplay.OnClicked += CheckIfCanBringBack;
        TablePlaceHandler.OnPlaceClicked += CheckPlace;
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        ActivationFiledAbilityDisplay.OnClicked -= CheckIfCanBringBack;
        TablePlaceHandler.OnPlaceClicked -= CheckPlace;
        closeButton.onClick.RemoveListener(Close);
    }
    
    private void CheckIfCanBringBack(AbilityCard _abilityCards)
    {
        if (GameplayManager.Instance.IsResponseAction())
        {
            if (!GameplayManager.Instance.IsMyResponseAction())
            {
                return;
            }

            if (!GameplayManager.Instance.IsKeeperResponseAction)
            {
                return;
            }
        }
        
        if (!GameplayManager.Instance.IsMyTurn())
        {
            return;
        }
        
        if (GameplayManager.Instance.IsAbilityActive<Subdued>() && GameplayCheats.CheckForCd)
        {
            DialogsManager.Instance.ShowOkDialog("Activation of the ability is blocked by Subdued ability");
            return;
        }

        int _indexOfCard = 1;
        foreach (var _shownCard in shownCards)
        {
            if (_shownCard.AbilityCard==_abilityCards)
            {
                break;
            }

            _indexOfCard++;
        }

        AbilityEffect _effect = _abilityCards.Effect;
        int _amountOfCardsOnTop = shownCards.Count - _indexOfCard;
        bool _canReturn = _effect.Cooldown <= _amountOfCardsOnTop;
        
        if (!GameplayCheats.CheckForCd)
        {
            _canReturn = true;
        }
        
        if (_canReturn)
        {
            GameplayManager.Instance.ReturnAbilityFromActivationField(_abilityCards.UniqueId);
            Close();
        }
        else
        {
            DialogsManager.Instance.ShowOkDialog($"This card needs {_effect.Cooldown} on top of it but there is only " +
                                            $"{_amountOfCardsOnTop}");
        }
    }

    private void CheckPlace(TablePlaceHandler _clickedPlace)
    {
        if (_clickedPlace.Id != 65 && _clickedPlace.Id != -1)
        {
            return;
        }

        ShowCards(_clickedPlace.GetCards());
    }

    private void ShowCards(List<CardBase> _cardBase)
    {
        if (_cardBase==null || _cardBase.Count == 0)
        {
            return;
        }

        if (shownCards.Count>0)
        {
            return;
        }

        foreach (var _card in _cardBase)
        {
            AbilityCard _abilityCard = _card as AbilityCard;
            var _display = Instantiate(activationFiledAbilityDisplay, cardsHolder);
            _display.Setup(_abilityCard);
            shownCards.Add(_display);
        }
        
        holder.SetActive(true);
        OnShowed?.Invoke();
    }

    private void Close()
    {
        foreach (var _shownCard in shownCards)
        {
            Destroy(_shownCard.gameObject);
        }
        shownCards.Clear();
        
        holder.SetActive(false);
        OnHided?.Invoke();
    }

}