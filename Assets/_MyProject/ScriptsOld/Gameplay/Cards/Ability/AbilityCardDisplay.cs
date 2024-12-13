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
}
