using UnityEngine;

public class ChainHandler : MonoBehaviour
{
    [SerializeField] private GameObject chain;

    private void OnEnable()
    {
        ActivationFieldHandler.OnShowed += Hide;
        ActivationFieldHandler.OnHided += Show;
        ArrowPanel.OnShow += Hide;
        ArrowPanel.OnHide += Show;
        ShowCardElarged.OnShowed += Hide;
        ShowCardElarged.OnHided += Show;
        ResolveMultipleActions.OnShowed += Hide;
        ResolveMultipleActions.OnHided += Show;
        ChooseCardPanel.OnShowed += Hide;
        ChooseCardPanel.OnClosed += Show;
        LootInfoPanel.OnClosed += Show;
        LootInfoPanel.OnShowed += Hide;
        ChooseCardImagePanel.OnShowed += Hide;
        ChooseCardImagePanel.OnClosed += Show;
        SettingsPanel.OnOpened += Hide;
        SettingsPanel.OnClosed += Show;
        chain.SetActive(SceneManager.IsGameplayScene);
    }

    private void OnDisable()
    {
        ActivationFieldHandler.OnShowed -= Hide;
        ActivationFieldHandler.OnHided -= Show;
        ArrowPanel.OnShow -= Hide;
        ArrowPanel.OnHide -= Show;
        ShowCardElarged.OnShowed -= Hide;
        ShowCardElarged.OnHided -= Show;
        ResolveMultipleActions.OnShowed -= Hide;
        ResolveMultipleActions.OnHided -= Show;
        ChooseCardPanel.OnShowed -= Hide;
        ChooseCardPanel.OnClosed -= Show;
        LootInfoPanel.OnClosed -= Show;
        LootInfoPanel.OnShowed -= Hide;
        ChooseCardImagePanel.OnShowed -= Hide;
        ChooseCardImagePanel.OnClosed -= Show;
        SettingsPanel.OnOpened -= Hide;
        SettingsPanel.OnClosed -= Show;
    }

    private void Hide()
    {
        chain.SetActive(false);
    }

    private void Show()
    {
        chain.SetActive(true);
    }
}
