using System;
using UnityEngine;
using UnityEngine.UI;

public class ShowFactionPreview : MonoBehaviour
{
    public static Action<FactionSO> OnShowFaction;

    private Button showFaction;

    private void Awake()
    {
        showFaction = GetComponent<Button>();
    }

    private void OnEnable()
    {
        showFaction.onClick.AddListener(PreviewFaction);
    }

    private void OnDisable()
    {
        showFaction.onClick.RemoveListener(PreviewFaction);
    }

    private void PreviewFaction()
    {
        ChooseFaction _chooseFaction = gameObject.GetComponentInParent<ChooseFaction>();
        OnShowFaction?.Invoke(_chooseFaction.Faction);
    }
}