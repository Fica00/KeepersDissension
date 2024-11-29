using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreviewFactionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI factionNameDisplay;
    [SerializeField] private GameObject zoomedCardHolder;
    [SerializeField] private PreviewFactionCardDisplay zoomedCard;
    [SerializeField] private PreviewFactionCardDisplay displayPrefab;
    [SerializeField] private Transform displaysHolder;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject holder;

    private List<GameObject> shownObjects = new();

    private void OnEnable()
    {
        PreviewFactionCardDisplay.OnZoomCard += ShowZoomedCard;
        ShowFactionPreview.OnShowFaction += Setup;
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        PreviewFactionCardDisplay.OnZoomCard -= ShowZoomedCard;
        ShowFactionPreview.OnShowFaction -= Setup;
        closeButton.onClick.RemoveListener(Close);
    }

    private void Close()
    {
        holder.SetActive(false);
    }

    private void ShowZoomedCard(Card _card)
    {
        Card _cardToShow = CardsManager.Instance.CreateCard(_card.Details.Id,true);
        zoomedCard.Setup(_cardToShow);
        zoomedCardHolder.SetActive(true);
    }

    private void Setup(FactionSO _faction)
    {
        factionNameDisplay.text = _faction.Name + " faction";
        ClearShownObjects();
        List<Card> _cards = CardsManager.Instance.Get(_faction);
        foreach (var _card in _cards)
        {
            if (_card is Wall)
            {
                continue;
            }

            if (_card is Marker)
            {
                continue;
            }

            PreviewFactionCardDisplay _display = Instantiate(displayPrefab, displaysHolder);
            _display.Setup(_card);
            shownObjects.Add(_display.gameObject);
        }
        holder.SetActive(true);
    }

    private void ClearShownObjects()
    {
        foreach (var _shownObject in shownObjects)
        {
            Destroy(_shownObject);
        }
        
        shownObjects.Clear();
    }
}
