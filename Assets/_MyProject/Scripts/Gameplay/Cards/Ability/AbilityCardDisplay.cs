using UnityEngine;
using UnityEngine.UI;

public class AbilityCardDisplay : CardDisplayBase
{
    [SerializeField] private Image foreground;
    private AbilityCard abilityCard;
    
    public override void Setup(AbilityCard _card)
    {
        abilityCard = _card;
        foreground.sprite = abilityCard.Details.Foreground;
    }

    public override void ManageNameDisplay(bool _status)
    {
        
    }

    private void Update()
    {
       SetTransparency();
    }

    private void SetTransparency()
    {
        Color _color = foreground.color;
        _color.a = 1;
        foreground.color = _color;
        
        TablePlaceHandler _place = GetComponentInParent<TablePlaceHandler>();
        if (_place==null)
        {
            return;
        }

        if (!_place.IsActivationField)
        {
            return;
        }

        int _amountOfCardsInActivationField = _place.GetCards().Count;
        float _percentage = 0;
        if (_amountOfCardsInActivationField==0)
        {
            _percentage = 0;
        }
        else
        {
            _percentage = _amountOfCardsInActivationField / (float)abilityCard.Data.PlaceInActivationField;
            if (_percentage>1)
            {
                _percentage = 1;
            }
        }

        _color.a = _percentage;
        foreground.color = _color;
    }
}
