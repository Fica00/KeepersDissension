using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay: CardDisplayBase
{
    [SerializeField] private GameObject redBorder;
    [SerializeField] private Image foregroundDisplay;
    [SerializeField] private Image whiteBox;
    [SerializeField] private TextMeshProUGUI nameHolder;
    private Card card;
    
    public override void Setup(Card _card)
    {
        card = _card;
        SetName();
        TryShowRedBox();
        foregroundDisplay.sprite = card.Details.Foreground;
    }

    private void SetName()
    {
        ManageNameDisplay(false);
        if (card is Wall or Marker)
        {
            nameHolder.text = string.Empty;
            return;
        }
        
        string[] _nameSplit = card.name.Split("_");
        string _name = _nameSplit[^1];
        if (_name == "LifeForce")
        {
            _name = "Life Force";
        }
        nameHolder.text = _name;
        
        //set font color
        Material _newMaterial = Instantiate(nameHolder.fontMaterial);
        _newMaterial.SetColor(ShaderUtilities.ID_OutlineColor, card.GetIsMy() ? card.Details.Faction.NameColor: card.Details.Faction.NameColorOpponent);

        nameHolder.fontMaterial = _newMaterial;
    }

    public override bool ChangeSprite(Sprite _sprite)
    {
        TryShowRedBox();
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

    private void TryShowRedBox()
    {
        if (redBorder==null)
        {
            return;
        }
        redBorder.SetActive(false);
        if (card.My)
        {
            return;
        }
        if (FirebaseManager.Instance.RoomHandler.RoomData.RoomPlayers[0].FactionId == FirebaseManager.Instance.RoomHandler.RoomData.RoomPlayers[1].FactionId)
        {
            redBorder.SetActive(true);
        }
    }

    public override void ManageNameDisplay(bool _status)
    {
        nameHolder.gameObject.SetActive(_status);
    }
}
