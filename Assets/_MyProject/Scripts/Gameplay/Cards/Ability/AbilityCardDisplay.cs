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
        if (_place == null)
        {
            return;
        }

        if (!_place.IsActivationField)
        {
            return;
        }
        
        _color.a = abilityCard.GetBringBackPercentage();
        foreground.color = _color;
    }

}
