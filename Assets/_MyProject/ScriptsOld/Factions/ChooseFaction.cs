using System;
using UnityEngine;
using UnityEngine.UI;

public class ChooseFaction : MonoBehaviour
{
    public static Action OnChosenFaction;
    [SerializeField] private FactionSO faction;
    [SerializeField] private Image nameDisplay;

    public FactionSO Faction => faction;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        nameDisplay.sprite = faction.NameSprite;
        
        button.onClick.AddListener(Choose);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(Choose);
    }

    private void Choose()
    {
        DataManager.Instance.PlayerData.FactionId = faction.Id;
        OnChosenFaction?.Invoke();
    }
}
