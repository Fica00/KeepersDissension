using System;
using System.Collections.Generic;
using System.Linq;
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
    private List<AbilityCard> shownAbilities;
    
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
    
    private void CheckIfCanBringBack(AbilityCard _abilityCard)
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
        else if (!GameplayManager.Instance.IsMyTurn())
        {
            return;
        }
        
        if (GameplayManager.Instance.IsAbilityActive<Subdued>() && GameplayCheats.CheckForCd)
        {
            DialogsManager.Instance.ShowOkDialog("Activation of the ability is blocked by Subdued ability");
            return;
        }

        bool _canReturn = _abilityCard.CanReturnFromActivationField();        
        if (!GameplayCheats.CheckForCd)
        {
            _canReturn = true;
        }

        int _indexOfAbility = shownAbilities.IndexOf(_abilityCard);
        for (int _i = _indexOfAbility+1; _i < shownAbilities.Count; _i++)
        {
            shownAbilities[_i].Data.PlaceInActivationField -= 1;
        }
        
        if (_canReturn)
        {
            GameplayManager.Instance.ReturnAbilityFromActivationField(_abilityCard.UniqueId);
        }
        
        Close();
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

        shownAbilities = _cardBase.Cast<AbilityCard>().ToList();
        shownAbilities = shownAbilities.OrderBy(_abilityCard => _abilityCard.Data.PlaceInActivationField).ToList();
        
        foreach (var _card in shownAbilities)
        {
            var _display = Instantiate(activationFiledAbilityDisplay, cardsHolder);
            _display.Setup(_card);
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