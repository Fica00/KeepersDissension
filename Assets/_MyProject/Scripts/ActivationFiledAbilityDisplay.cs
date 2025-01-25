using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActivationFiledAbilityDisplay : MonoBehaviour
{
    public static Action<AbilityCard> OnClicked;
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI progress;

    private AbilityCard abilityCard;
    public AbilityCard AbilityCard => abilityCard;
    
    public void Setup(AbilityCard _abilityCard)
    {
        abilityCard = _abilityCard;
        image.sprite = CardsManager.Instance.GetAbilityImage(_abilityCard.Details.Id);
        button.interactable = abilityCard.CanReturnFromActivationField();
        progress.text = abilityCard.GetTextForActivationField();
        if (!_abilityCard.GetIsMy())
        {
            button.interactable = false;
        }
    }

    private void OnEnable()
    {
        button.onClick.AddListener(Select);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Select);
    }

    private void Select()
    {
        OnClicked?.Invoke(abilityCard);
    }
}
