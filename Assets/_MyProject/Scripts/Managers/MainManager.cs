using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainManager: MonoBehaviour
{
    [SerializeField] private GameObject mainMenuHolder;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button aboutButton;

    [SerializeField] private SettingsPanel settingsPanel;
    [SerializeField] private AboutPanel aboutPanel;
    [SerializeField] private GameObject creditsPanel;

    [SerializeField] private TextMeshProUGUI textDisplay;
    
    private void OnEnable()
    {
        settingsButton.onClick.AddListener(ShowSettings);
        creditsButton.onClick.AddListener(ShowCredits);
        playButton.onClick.AddListener(Play);
        aboutButton.onClick.AddListener(ShowAbout);
        
        AudioManager.Instance.Init();
        AudioManager.Instance.PlayBackgroundMusic();
        ShowText();
    }

    private void OnDisable()
    {
        settingsButton.onClick.RemoveListener(ShowSettings);
        creditsButton.onClick.RemoveListener(ShowCredits);
        playButton.onClick.RemoveListener(Play);
        aboutButton.onClick.RemoveListener(ShowAbout);
    }

    private void ShowSettings()
    {
        settingsPanel.Setup();
    }

    private void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    private void Play()
    {
        mainMenuHolder.SetActive(false);
        MatchMakingHandler.Instance.Setup();
    }

    private void ShowAbout()
    {
        aboutPanel.Setup();
    }

    private void ShowText()
    {
        textDisplay.text = DataManager.Instance.GameData.DisplayText;
    }
}