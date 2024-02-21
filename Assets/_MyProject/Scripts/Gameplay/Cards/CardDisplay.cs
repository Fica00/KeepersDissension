using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay: CardDisplayBase
{
    [SerializeField] private Image foregroundDisplay;
    [SerializeField] private Image whiteBox;
    private Card card;
    
    public override void Setup(Card _card)
    {
        card = _card;
        foregroundDisplay.sprite = card.Details.Foreground;
    }

    public override bool ChangeSprite(Sprite _sprite)
    {
        if (foregroundDisplay.sprite==_sprite)
        {
            return false;
        }
        foregroundDisplay.sprite = _sprite;
        return true;
    }

    public override void ShowWhiteBox()
    {
        StartCoroutine(ShowRoutine());
        IEnumerator ShowRoutine()
        {
            whiteBox.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            whiteBox.gameObject.SetActive(false);
        }
        
    }
}
